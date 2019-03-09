using System;
using Akka.Actor;
using Common.Helpers;
using VehicleSimulator.Messages;

namespace VehicleSimulator.Actors
{
    public class SensorActor : ReceiveActor
    {
        private readonly SensorTypes _sensorType;
        private readonly Random _rnd = new Random(Guid.NewGuid().GetHashCode());
        private decimal _value;

        public SensorActor(SensorTypes sensorType, TimeSpan interval, decimal initialValue)
        {
            _sensorType = sensorType;
            _value = initialValue;
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(1), interval, Context.Self, new SensorUpdateMessage(), ActorRefs.NoSender);

            Receive<SensorUpdateMessage>(msg => {
                _value = (_value - 0.1m) > 0 ? (_value - 0.1m) : 0;
                Context.Parent.Tell(new SensorDataMessage {
                        Name = _sensorType.ToString(),
                        Quality = (short)_rnd.Next(0, 200),
                        Timestamp = DateTime.UtcNow.ToEpochTimestamp(),
                        Value = _value
                }, Context.Self);
            });
        }

        public static Props Props(SensorTypes sensorTypes, TimeSpan interval, decimal initialValue = 100) => 
                Akka.Actor.Props.Create(() => new SensorActor(sensorTypes, interval, initialValue));
    }
}