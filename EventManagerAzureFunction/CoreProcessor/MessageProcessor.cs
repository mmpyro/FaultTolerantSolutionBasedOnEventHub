using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Dtos;
using Common.Factories;
using Common.Policy;
using Common.Repositories;
using Common.Wrappers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CoreProcessor
{
    public class MessageProcessor : IMessageProcessor
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IPoisonMessageRepository _poisonMessageRepository;
        private readonly IPolicyRegistry _policyRegistry;
        private readonly ILogger _logger;

        public MessageProcessor(IRepositoryFactory repositoryFactory, IPoisonMessageRepository poisonMessageRepository,
            IPolicyRegistry policyRegistry, ILogger logger)
        {
            _repositoryFactory = repositoryFactory;
            _poisonMessageRepository = poisonMessageRepository;
            _policyRegistry = policyRegistry;
            _logger = logger;
        }

        public async Task ProcessAsync(IEnumerable<EventDataWrapper> messages)
        {
            var tab = messages.ToArray();
            var policies = _policyRegistry.CreateAsyncPolicies();
            var repository = _repositoryFactory.Create();
            var groupedMessages = messages.Where(t => t.Properties.ContainsKey(Constants.VehicleId))
                .GroupBy(t => t.Properties[Constants.VehicleId]);

            foreach (var message in groupedMessages)
            {
                var id = message.Key.ToString();
                _logger?.LogDebug($"Process event for vehicle {id}");
                var vehicleSnapshot = repository.Get(id) ?? new VehicleSnapshot { Id = id };

                message
                    .Select(t => Deserialize(t))
                    .Where(t => t != null && t.Quality >= Constants.QualityGate)
                    .ToList().ForEach(s =>
                    {
                        if (vehicleSnapshot.Sensors.ContainsKey(s.Name))
                        {
                            ReplaceSensorData(s, vehicleSnapshot);
                        }
                        else
                        {
                            vehicleSnapshot.Sensors.Add(s.Name, VehicleSensorData.Create(s));
                        }
                    });
                await Polly.Policy.WrapAsync(policies).ExecuteAsync(async () =>
                {
                    await repository.AddAsync(vehicleSnapshot);
                    _logger?.LogInformation($"Save snapshot for vehicle {id}");
                });
            };
        }

        private SensorDto Deserialize(EventDataWrapper eventDataWrapper)
        {
            try
            {
                return JsonConvert.DeserializeObject<SensorDto>(eventDataWrapper.Body);
            }
            catch(JsonException)
            {
                _poisonMessageRepository.Save(eventDataWrapper).Wait();
                return null;
            }
        }

        private static void ReplaceSensorData(SensorDto sensorDto, VehicleSnapshot vehicleSnapshot)
        {
            var sensor = vehicleSnapshot.Sensors[sensorDto.Name];
            if (sensorDto.Timestamp >= sensor.Timestamp)
            {
                vehicleSnapshot.Sensors[sensorDto.Name] = VehicleSensorData.Create(sensorDto);
            }
        }
    }
}
