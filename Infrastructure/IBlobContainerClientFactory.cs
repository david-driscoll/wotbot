using Azure.Data.Tables;
using Azure.Storage.Blobs;

namespace wotbot.Infrastructure
{
    public interface IBlobContainerClientFactory
    {
        BlobContainerClient CreateClient(string blobContainerName);
    }
    public interface ITableClientFactory
    {
        TableClient CreateClient(string blobContainerName);
    }
}
