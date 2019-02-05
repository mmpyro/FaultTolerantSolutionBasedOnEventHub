namespace Common.Dtos
{
    public class VehicleSensorData
    {
        public long Timestamp { get; set; }
        public short Quality { get; set; }
        public decimal Value { get; set; }

        public static VehicleSensorData Create(SensorDto sensorDto)
        {
            return new VehicleSensorData
            {
                 Timestamp = sensorDto.Timestamp,
                 Quality = sensorDto.Quality,
                 Value = sensorDto.Value
            };
        }
    }
}
