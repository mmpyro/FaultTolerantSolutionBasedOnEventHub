using Newtonsoft.Json;

namespace VehicleSimulator.Messages
{
    public class SensorDataMessage
    {
        public string Name { get; set; }
        public long Timestamp { get; set; }
        public short Quality { get; set; }
        public decimal Value { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}