using System;
using System.Collections.Generic;
using Akka.Actor;
using VehicleSimulator.Messages;

namespace VehicleSimulator.Actors
{
    public class VehicleSupervisiorActor : ReceiveActor
    {
        private Dictionary<string, IActorRef> _childs = new Dictionary<string, IActorRef>();
        public VehicleSupervisiorActor()
        {
            this.Receive<StartSimulationMessage>(msg => {
                foreach(var device in msg.Devices)
                {
                    if(! _childs.ContainsKey(device.Key))
                    {
                        var deviceActor = Context.ActorOf(VehicleActor.Props(device.Key, device.Value));
                        Context.Watch(deviceActor);
                        _childs.Add(device.Key, deviceActor);
                    }
                }
            });
        }

        public static Props Props() => Akka.Actor.Props.Create<VehicleSupervisiorActor>();
    }
}