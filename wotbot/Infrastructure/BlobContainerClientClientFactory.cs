using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace wotbot.Infrastructure
{
    class BlobContainerClientClientFactory : IBlobContainerClientFactory
    {
        private readonly IConfiguration _configuration;

        public BlobContainerClientClientFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public BlobContainerClient CreateClient(string blobContainerName)
        {
            return new(_configuration.GetConnectionString("BlobStorage"), blobContainerName);
        }
    }
}
