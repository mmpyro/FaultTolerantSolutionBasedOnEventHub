using System;
using Akka.Actor;
using VehicleSimulator.Messages;
using Microsoft.Azure.Devices.Client;
using System.Text;

namespace VehicleSimulator.Actors
{
    public class VehicleActor : ReceiveActor
    {
        private readonly string _deviceId;
        private readonly DeviceClient _deviceClient;

        public VehicleActor(string deviceId, string iotHubConnectionString)
        {
            Console.WriteLine($"{deviceId} is starting");
            _deviceId = deviceId;
            _deviceClient = DeviceClient.CreateFromConnectionString(iotHubConnectionString, TransportType.Amqp);

            Context.Watch(Context.ActorOf(SensorActor.Props(SensorTypes.Fuel, TimeSpan.FromMinutes(1))));
            Context.Watch(Context.ActorOf(SensorActor.Props(SensorTypes.BreakFluid, TimeSpan.FromMinutes(2))));
            
            Receive<SensorDataMessage>(async msg => {
                var messageString = msg.ToString();
                var message = new Message(Encoding.ASCII.GetBytes(messageString));
                message.Properties.Add("vehicleId", _deviceId);
                await _deviceClient.SendEventAsync(message);
                Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);
            });
        }

        public static Props Props(string deviceId, string iotHubConnectionString) =>
                 Akka.Actor.Props.Create(() => new VehicleActor(deviceId, iotHubConnectionString));
    }
}