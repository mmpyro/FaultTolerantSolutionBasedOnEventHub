using System.Collections.Generic;
using System.Linq;

namespace VehicleSimulator.Messages
{
    public class StartSimulationMessage
    {
        public StartSimulationMessage()
        {
            Devices = new Dictionary<string, string>();
        }

        public StartSimulationMessage(string[] deviceConnectionStrings)
        {
            Devices = deviceConnectionStrings.Select(connectionString => {
                string deviceId = connectionString.Split(";")[1];
                return new {Id = deviceId.Replace("DeviceId=", ""), ConnectionString = connectionString};
            }).ToDictionary(k => k.Id, v => v.ConnectionString);
        }

        public Dictionary<string, string> Devices { get; set; }
    }
}