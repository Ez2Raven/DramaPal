using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

s_rootCommand.SetHandler(
    async (context) =>
    {
        var options = GetParsedAppOptions(context);
        if (options.RemoveAll)
        {
            await RemoveBlobsAsync(options);
            await RemoveFromIndexAsync(options);
        }
        else
        {
            var searchIndexName = options.SearchIndexName ?? throw new ArgumentNullException(nameof(options.SearchIndexName));
            var embedService = await GetAzureSearchEmbedService(options);
            await embedService.EnsureSearchIndexAsync(options.SearchIndexName);

            Matcher matcher = new();
            // From bash, the single quotes surrounding the path (to avoid expansion of the wildcard), are included in the argument value.
            matcher.AddInclude(options.Files.Replace("'", string.Empty));

            var results = matcher.Execute(
                new DirectoryInfoWrapper(
                    new DirectoryInfo(Directory.GetCurrentDirectory())));

            var files = results.HasMatches
                ? results.Files.Select(f => f.Path).ToArray()
                : Array.Empty<string>();

            context.Console.WriteLine($"Processing {files.Length} files...");

            var tasks = Enumerable.Range(0, files.Length)
                .Select(i =>
                {
                    var fileName = files[i];
                    return ProcessSingleFileAsync(options, fileName, embedService);                    
                });

            await Task.WhenAll(tasks);

            static async Task ProcessSingleFileAsync(AppOptions options, string fileName, IEmbedService embedService)
            {
                if (options.Verbose)
                {
                    options.Console.WriteLine($"Processing '{fileName}'");
                }

                if (options.Remove)
                {
                    await RemoveBlobsAsync(options, fileName);
                    await RemoveFromIndexAsync(options, fileName);
                    return;
                }

                await UploadBlobsAndCreateIndexAsync(options, fileName, embedService);
            }
        }
    });

return await s_rootCommand.InvokeAsync(args);

static async ValueTask RemoveBlobsAsync(
    AppOptions options, string? fileName = null)
{
    if (options.Verbose)
    {
        options.Console.WriteLine($"Removing blobs for '{fileName ?? "all"}'");
    }

    var prefix = string.IsNullOrWhiteSpace(fileName)
        ? Path.GetFileName(fileName)
        : null;

    var getContainerClientTask = GetBlobContainerClientAsync(options);
    var getCorpusClientTask = GetCorpusBlobContainerClientAsync(options);
    var clientTasks = new[] { getContainerClientTask, getCorpusClientTask };

    await Task.WhenAll(clientTasks);

    foreach (var clientTask in clientTasks)
    {
        var client = await clientTask;
        await DeleteAllBlobsFromContainerAsync(client, prefix);
    }

    static async Task DeleteAllBlobsFromContainerAsync(BlobContainerClient client, string? prefix)
    {
        await foreach (var blob in client.GetBlobsAsync())
        {
            if (string.IsNullOrWhiteSpace(prefix) ||
                blob.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                await client.DeleteBlobAsync(blob.Name);
            }
        }
    };
}

static async ValueTask RemoveFromIndexAsync(
    AppOptions options, string? fileName = null)
{
    if (options.Verbose)
    {
        options.Console.WriteLine($"""
            Removing sections from '{fileName ?? "all"}' from search index '{options.SearchIndexName}.'
            """);
    }

    var searchClient = await GetSearchClientAsync(options);

    while (true)
    {
        var filter = (fileName is null) ? null : $"sourcefile eq '{Path.GetFileName(fileName)}'";

        var response = await searchClient.SearchAsync<SearchDocument>("",
            new SearchOptions
            {
                Filter = filter,
                Size = 1_000,
                IncludeTotalCount = true
            });

        var documentsToDelete = new List<SearchDocument>();
        await foreach (var result in response.Value.GetResultsAsync())
        {
            documentsToDelete.Add(new SearchDocument
            {
                ["id"] = result.Document["id"]
            });
        }

        if (documentsToDelete.Count == 0)
        {
            break;
        }
        Azure.Response<IndexDocumentsResult> deleteResponse =
            await searchClient.DeleteDocumentsAsync(documentsToDelete);

        if (options.Verbose)
        {
            Console.WriteLine($"""
                    Removed {deleteResponse.Value.Results.Count} sections from index
                """);
        }

        // It can take a few seconds for search results to reflect changes, so wait a bit
        await Task.Delay(TimeSpan.FromMilliseconds(2_000));
    }
}

static async ValueTask UploadBlobsAndCreateIndexAsync(
    AppOptions options, string filePath, IEmbedService embeddingService)
{
    var containerClient = await GetBlobContainerClientAsync(options);

    // If it's a CSV, split it by character name.
    if (Path.GetExtension(filePath).Equals(".csv", StringComparison.OrdinalIgnoreCase))
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            PrepareHeaderForMatch = args => args.Header.ToLower(),
        };
        // Read the CSV file, group by character name
        using (var reader = new StreamReader(filePath))
        using (var csv = new CsvReader(reader, config))
        {
            var records = csv.GetRecords<TranscriptLine>().ToList();

            var groupedRecords = records
                .GroupBy(r => r.Name);
                
            foreach (var group in groupedRecords)
            {
                var documentName = $"{Path.GetFileNameWithoutExtension(filePath)}-{group.Key}.txt";
                var blobClient = containerClient.GetBlobClient(documentName);
                if (await blobClient.ExistsAsync())
                {
                    continue;
                }

                using (var memoryStream = new MemoryStream())
                using (var writer = new StreamWriter(memoryStream))
                {                  
                    foreach (var item in group)
                    {
                        writer.WriteLine($"Line: {item.Line}");
                    }
                    writer.Flush();
                    memoryStream.Position = 0;
                    await blobClient.UploadAsync(memoryStream, new BlobHttpHeaders
                    {
                        ContentType = "text/csv"
                    });

                    memoryStream.Position=0;
                    await embeddingService.EmbedCsvBlobAsync(memoryStream, documentName);
                }
            }
        }

        // create the embedding 
    }
    else
    {
        Console.WriteLine($"{filePath} is not a CSV extension");
    }
}

static async Task<string> UploadBlobAsync(string fileName, string blobName, BlobContainerClient container)
{
    var blobClient = container.GetBlobClient(blobName);
    var url = blobClient.Uri.AbsoluteUri;

    if (await blobClient.ExistsAsync())
    {
        return url;
    }

    var blobHttpHeaders = new BlobHttpHeaders
    {
        ContentType = GetContentType(fileName)
    };

    await using var fileStream = File.OpenRead(fileName);
    await blobClient.UploadAsync(fileStream, blobHttpHeaders);


    return url;
}

static string GetContentType(string fileName)
{
    var extension = Path.GetExtension(fileName);
    return extension switch
    {
        ".csv" => "text/csv",
        ".pdf" => "application/pdf",
        ".txt" => "text/plain",

        _ => "application/octet-stream"
    };
}

static string BlobNameFromCharacterNameField(string filename, string characterName) => Path.GetExtension(filename).ToLower() is ".csv"
        ? $"{Path.GetFileNameWithoutExtension(filename)}-{characterName}.csv"
        : Path.GetFileName(filename);

internal static partial class Program
{
    [GeneratedRegex("[^0-9a-zA-Z_-]")]
    private static partial Regex MatchInSetRegex();

    internal static DefaultAzureCredential DefaultCredential { get; } = new();
}
