using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace TestUtils.Dal
{
    public class TableContext
    {
        private readonly string _storageConnectionString;
        private readonly string _tableName;

        public TableContext(string storageConnectionString, string tableName)
        {
            _storageConnectionString = storageConnectionString;
            _tableName = tableName;
        }

        public async Task Reset<T>() where T : TableEntity, new()
        {
            var table = CreateTableReference();
            await table.DeleteIfExistsAsync();
        }

        public async Task<T[]> Get<T>(string id) where T : TableEntity, new()
        {
            var table = CreateTableReference();
            var query = new TableQuery<T>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, id));
            var items = new List<T>();
            TableContinuationToken token = null;
            do
            {
                TableQuerySegment<T> seg = await table.ExecuteQuerySegmentedAsync<T>(query, token);
                token = seg.ContinuationToken;
                items.AddRange(seg);
            } while (token != null);
            return items.ToArray();
        }

        private CloudTable CreateTableReference()
        {
            var storageAccount = CloudStorageAccount.Parse(_storageConnectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            return tableClient.GetTableReference(_tableName);
        }
    }
}
