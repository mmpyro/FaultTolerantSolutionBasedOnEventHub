using Common;
using Common.Dtos;
using Common.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace CoreProcessor
{
    public class SnapshotReprocessor : ISnapshotReprocessor
    {
        private readonly IConfigurationRoot _configuration;
        private readonly IRepository _repository;

        public SnapshotReprocessor(IConfigurationRoot configuration, IRepository repository)
        {
            _configuration = configuration;
            _repository = repository;
        }

        public async Task Reprocess()
        {
            var storageAccount = CloudStorageAccount.Parse(_configuration[Constants.Storage.ConnectionString]);
            var tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference(Constants.Storage.VariableSnapshotTableName);

            var query = new TableQuery<VehicleSnapshotEntity>
            {
                TakeCount = 100
            };

            var segmentedTableQuery = await table.ExecuteQuerySegmentedAsync(query, null);
            if (segmentedTableQuery.Results.Any())
            {
                foreach(var row in segmentedTableQuery.Results)
                {
                    var snapshot = JsonConvert.DeserializeObject<VehicleSnapshot>(row.VehicleSnapshotJson);
                    var vehicleSnapshot = _repository.Get(snapshot.Id);
                    foreach (var sensor in snapshot.Sensors)
                    {
                        if (vehicleSnapshot.Sensors.ContainsKey(sensor.Key))
                        {
                            var oldSensor = vehicleSnapshot.Sensors[sensor.Key];
                            if (oldSensor.Timestamp < sensor.Value.Timestamp)
                            {
                                vehicleSnapshot.Sensors[sensor.Key] = sensor.Value;
                            }
                        }
                        else
                        {
                            vehicleSnapshot.Sensors.Add(sensor.Key, sensor.Value);
                        }
                    }
                    await _repository.AddAsync(vehicleSnapshot);
                    await table.ExecuteAsync(TableOperation.Delete(row));
                }
            }
        }
    }
}
