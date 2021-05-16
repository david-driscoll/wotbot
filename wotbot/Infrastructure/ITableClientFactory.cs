using Azure.Data.Tables;

namespace wotbot.Infrastructure
{
    public interface ITableClientFactory
    {
        TableClient CreateClient(string blobContainerName);
    }
}