using Common;
using Common.Dtos;
using Common.Factories;
using Common.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;

namespace CoreProcessor
{
    public class SnapshotReprocessor : ISnapshotReprocessor
    {
        private readonly IConfigurationRoot _configuration;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IPoisonMessageRepository _poisonMessageRepository;
        private readonly ILogger _logger;

        public SnapshotReprocessor(IConfigurationRoot configuration, IRepositoryFactory repositoryFactory,
            IPoisonMessageRepository poisonMessageRepository, ILogger logger)
        {
            _configuration = configuration;
            _repositoryFactory = repositoryFactory;
            _poisonMessageRepository = poisonMessageRepository;
            _logger = logger;
        }

        public async Task Reprocess()
        {
            var snaphots = await _poisonMessageRepository.GetUnprocessedSnapshots();
            foreach (var row in snaphots)
            {
                var snapshot = JsonConvert.DeserializeObject<VehicleSnapshot>(row.VehicleSnapshotJson);
                var repository = _repositoryFactory.Create();
                var vehicleSnapshot = repository.Get(snapshot.Id);
                var sensors = snapshot?.Sensors?.Where(s => s.Value.Quality >= Constants.QualityGate);
                foreach (var sensor in sensors)
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
                await repository.AddAsync(vehicleSnapshot);
                _logger?.LogInformation($"VehicleSnapshot saved for {vehicleSnapshot.Id} vehicleId.");
                await _poisonMessageRepository.DeleteSnapshot(row);
            }
        }
    }
}
