using System;

namespace VehicleSimulator
{
    class Program
    {
        private readonly static string _connectionString = "";

        static void Main(string[] args)
        {
            try
            {
                var vehicleSimulator = new CronSimulator(1, _connectionString);
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
