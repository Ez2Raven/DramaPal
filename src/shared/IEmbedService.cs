namespace shared;

public interface IEmbedService
{
    Task<bool> EmbedCsvBlobAsync(Stream blobStream, string blobName);
    Task CreateSearchIndexAsync(string searchIndexName, CancellationToken ct = default);

    Task EnsureSearchIndexAsync(string searchIndexName, CancellationToken ct = default);
}
