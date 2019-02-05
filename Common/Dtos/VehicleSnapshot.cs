using Newtonsoft.Json;
using System.Collections.Generic;

namespace Common.Dtos
{
    public class VehicleSnapshot
    {
        public VehicleSnapshot()
        {
            Sensors = new Dictionary<string, VehicleSensorData>();
        }

        [JsonProperty(PropertyName ="id")]
        public string Id { get; set; }

        public Dictionary<string, VehicleSensorData> Sensors { get; set; }

    }
}
