using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Dtos;
using Common.Factories;
using Common.Wrappers;
using Newtonsoft.Json;

namespace CoreProcessor
{
    public class MessageProcessor : IMessageProcessor
    {
        private readonly IRepositoryFactory _repositoryFactory;

        public MessageProcessor(IRepositoryFactory repositoryFactory)
        {
            _repositoryFactory = repositoryFactory;
        }

        public async Task ProcessAsync(IEnumerable<EventDataWrapper> messages)
        {
            var repository = _repositoryFactory.Create();
            var groupedMessages = messages.Where(t => t.Properties.ContainsKey(Constants.VehicleId))
                .GroupBy(t => t.Properties[Constants.VehicleId]);

            foreach (var message in groupedMessages)
            {
                var id = message.Key.ToString();
                var vehicleSnapshot = repository.Get(id) ?? new VehicleSnapshot { Id = id };
    
                message
                    .Select(t => JsonConvert.DeserializeObject<SensorDto>(t.Body))
                    .Where(t => t.Quality >= Constants.QualityGate)
                    .ToList().ForEach(s =>
                    {
                        if(vehicleSnapshot.Sensors.ContainsKey(s.Name))
                        {
                            ReplaceSensorData(s, vehicleSnapshot);
                        }
                        else
                        {
                            vehicleSnapshot.Sensors.Add(s.Name, VehicleSensorData.Create(s));
                        }
                    });
                await repository.AddAsync(vehicleSnapshot);
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
