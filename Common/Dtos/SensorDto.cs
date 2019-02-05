using Newtonsoft.Json;

namespace Common.Dtos
{
    public class SensorDto
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
