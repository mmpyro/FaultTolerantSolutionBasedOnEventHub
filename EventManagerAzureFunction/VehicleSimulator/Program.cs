using Akka.Actor;
using Microsoft.Extensions.Configuration;
using System;
using VehicleSimulator.Actors;
using VehicleSimulator.Messages;

namespace VehicleSimulator
{
    class Program
    {
        private const string ConnectionStringSection = "devices";
        static void Main(string[] args)
        {
            try
            {
                var config = new ConfigurationBuilder()
                 .SetBasePath(Environment.CurrentDirectory)
                 .AddJsonFile("local.settings.json", optional: false, reloadOnChange: true)
                 .AddJsonFile($"local.settings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true, reloadOnChange: true)
                 .AddEnvironmentVariables()
                 .Build();

                using( var system = ActorSystem.Create("VehicleSimulatorSystem"))
                {
                    var vehicleSupervisiorActor = system.ActorOf(VehicleSupervisiorActor.Props());
                    vehicleSupervisiorActor.Tell(new StartSimulationMessage(config.GetSection(ConnectionStringSection).Get<string[]>()));
                    Console.ReadKey();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }
    }
}
