using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using shared.Models;

namespace shared.Services;

public sealed partial class AzureSearchEmbedService(
    OpenAIClient openAIClient,
    string embeddingModelName,
    SearchClient searchClient,
    string searchIndexName,
    SearchIndexClient searchIndexClient,
    BlobContainerClient corpusContainerClient,
    ILogger<AzureSearchEmbedService>? logger = null) : IEmbedService
{
    public async Task<bool> EmbedCsvBlobAsync(Stream blobStream, string blobName){
        return false;
    }

    public async Task CreateSearchIndexAsync(string searchIndexName, CancellationToken ct = default)    
    {
        string vectorSearchConfigName = "my-vector-config";
        string vectorSearchProfile = "my-vector-profile";
        var index = new SearchIndex(searchIndexName)
        {
            VectorSearch = new()
            {
                Algorithms =
                {
                    new HnswAlgorithmConfiguration(vectorSearchConfigName)
                },
                Profiles =
                {
                    new VectorSearchProfile(vectorSearchProfile, vectorSearchConfigName)
                }
            },
            Fields =
            {
                new SimpleField("id", SearchFieldDataType.String) { IsKey = true },
                new SearchableField("content") { AnalyzerName = LexicalAnalyzerName.EnMicrosoft },
                new SimpleField("category", SearchFieldDataType.String) { IsFacetable = true },
                new SimpleField("sourcepage", SearchFieldDataType.String) { IsFacetable = true },
                new SimpleField("sourcefile", SearchFieldDataType.String) { IsFacetable = true },
                new SearchField("embedding", SearchFieldDataType.Collection(SearchFieldDataType.Single))
                {
                    VectorSearchDimensions = 1536,
                    IsSearchable = true,
                    VectorSearchProfileName = vectorSearchProfile,
                }
            },
            SemanticSearch = new()
            {
                Configurations =
                {
                    new SemanticConfiguration("default", new()
                    {
                        ContentFields =
                        {
                            new SemanticField("content")
                        }
                    })
                }
            }
        };

        logger?.LogInformation(
            "Creating '{searchIndexName}' search index", searchIndexName);

        await searchIndexClient.CreateIndexAsync(index, ct);
    }

    public async Task EnsureSearchIndexAsync(string searchIndexName, CancellationToken ct = default)
    {
        var indexNames = searchIndexClient.GetIndexNamesAsync(ct);
        await foreach (var page in indexNames.AsPages().WithCancellation(ct))
        {
            if (page.Values.Any(indexName => indexName == searchIndexName))
            {
                logger?.LogWarning(
                    "Search index '{SearchIndexName}' already exists", searchIndexName);
                return;
            }
        }

        await CreateSearchIndexAsync(searchIndexName, ct);
    }

   
}