internal static partial class Program
{
    private static readonly Argument<string> s_files =
        new(name: "files", description: "Files to be processed");

    private static readonly Option<bool> s_remove =
       new(name: "--remove", description: "Remove references to this document from blob storage and the search index");

    private static readonly Option<bool> s_removeAll =
        new(name: "--removeall", description: "Remove all blobs from blob storage and documents from the search index");

    private static readonly Option<string> s_storageEndpoint =
        new(name: "--storageendpoint", description: "URI of Azure Blob Storage service endpoint");

    private static readonly Option<string> s_container =
        new(name: "--container", description: "Name of Azure Blob Storage container");

    private static readonly Option<string> s_tenantId =
        new(name: "--tenantid", description: "Optional. The specific Azure directory to authenticate against)");

    private static readonly Option<string> s_searchService =
        new(name: "--searchendpoint", description: "The Azure AI Search service endpoint where content should be indexed (must exist prior to running this app)");

    private static readonly Option<string> s_searchIndexName =
        new(name: "--searchindex", description: "Name of the Azure AI Search index where content should be indexed (will be created if it doesn't exist)");

    private static readonly Option<string> s_azureOpenAIService =
        new(name: "--openaiendpoint", description: "Optional. The Azure OpenAI service endpoint which will be used to extract text, tables and layout from the documents (must exist prior to running this app)");

    private static readonly Option<string> s_embeddingModelName =
        new(name: "--embeddingmodel", description: "Optional. Name of the Azure AI Search embedding model to use for embedding content in the search index (will be created if it doesn't exist)");

    private static readonly Option<bool> s_verbose =
       new(aliases: new[] { "--verbose", "-v" }, description: "Verbose output");

    private static readonly RootCommand s_rootCommand =
        new(description: """
        Prepare documents by extracting content from csv files, splitting content by character,
        uploading to blob storage, and indexing in a search index.
        """)
        {
            s_files,
            s_storageEndpoint,
            s_container,
            s_tenantId,
            s_searchService,
            s_searchIndexName,
            s_azureOpenAIService,
            s_embeddingModelName,
            s_remove,
            s_removeAll,
            s_verbose,
        };
    private static AppOptions GetParsedAppOptions(InvocationContext context) => new(
            Files: context.ParseResult.GetValueForArgument(s_files),
            StorageServiceBlobEndpoint: context.ParseResult.GetValueForOption(s_storageEndpoint),
            Container: context.ParseResult.GetValueForOption(s_container),
            TenantId: context.ParseResult.GetValueForOption(s_tenantId),
            SearchServiceEndpoint: context.ParseResult.GetValueForOption(s_searchService),
            SearchIndexName: context.ParseResult.GetValueForOption(s_searchIndexName),
            AzureOpenAIServiceEndpoint: context.ParseResult.GetValueForOption(s_azureOpenAIService),
            EmbeddingModelName: context.ParseResult.GetValueForOption(s_embeddingModelName),
            Remove: context.ParseResult.GetValueForOption(s_remove),
            RemoveAll: context.ParseResult.GetValueForOption(s_removeAll),
            Verbose: context.ParseResult.GetValueForOption(s_verbose),
            Console: context.Console);
}