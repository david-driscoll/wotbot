using Azure.Storage.Blobs;

namespace wotbot.Infrastructure
{
    public interface IBlobContainerClientFactory
    {
        BlobContainerClient CreateClient(string blobContainerName);
    }
}
