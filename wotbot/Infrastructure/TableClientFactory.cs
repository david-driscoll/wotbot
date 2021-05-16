using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;

namespace wotbot.Infrastructure
{
    class TableClientFactory : ITableClientFactory
    {
        private readonly IConfiguration _configuration;

        public TableClientFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public TableClient CreateClient(string blobContainerName)
        {
            return new(_configuration.GetConnectionString("TableStorage"), blobContainerName);
        }
    }
}