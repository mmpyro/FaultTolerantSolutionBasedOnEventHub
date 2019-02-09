using Common.Dtos;
using Common.Wrappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace Common.Repositories
{
    public class PoisonMessageRepository : IPoisonMessageRepository
    {
        private readonly IConfigurationRoot _configuration;
        private readonly ILogger _logger;

        public PoisonMessageRepository(IConfigurationRoot configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task Save(EventDataWrapper eventDataWrapper)
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(_configuration[Constants.Storage.ConnectionString]);
                var tableClient = storageAccount.CreateCloudTableClient();
                var table = tableClient.GetTableReference(Constants.Storage.PoisonTableName);
                await table.CreateIfNotExistsAsync();

                var insertOperation = TableOperation.Insert(new PoisonMessageEntity(eventDataWrapper));
                await table.ExecuteAsync(insertOperation);
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, "Unable to save poison message into table");
            }
        }
    }
}
