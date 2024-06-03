namespace data_processing
{
    internal record class AppOptions(
        string Files,
        string? StorageServiceBlobEndpoint,
        string? Container,
        string? TenantId,
        string? SearchServiceEndpoint,
        string? AzureOpenAIServiceEndpoint,
        string? SearchIndexName,
        string? EmbeddingModelName,
        bool Remove,
        bool RemoveAll,
        bool Verbose,
        IConsole Console) : AppConsole(Console);

    internal record class AppConsole(IConsole Console);
}
