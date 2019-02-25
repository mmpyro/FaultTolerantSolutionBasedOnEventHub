using Microsoft.Extensions.Configuration;
using System;

namespace VehicleSimulator
{
    class Program
    {
        private const string DeviceConnectionString = "connectionString";
        static void Main(string[] args)
        {
            try
            {
                var config = new ConfigurationBuilder()
                 .SetBasePath(Environment.CurrentDirectory)
                 .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                 .AddJsonFile($"local.settings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
                 .AddEnvironmentVariables()
                 .Build();

                var vehicleSimulator = new CronSimulator(1, config[DeviceConnectionString]);
                vehicleSimulator
                    .SimulateSensor(SensorTypes.Break, TimeSpan.FromSeconds(10))
                    .SimulateSensor(SensorTypes.Fuel, TimeSpan.FromSeconds(30))
                    .Simulate();

                Console.ReadKey();
                vehicleSimulator.Stop();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }
    }
}
