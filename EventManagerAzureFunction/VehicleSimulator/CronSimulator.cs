using Common.Dtos;
using Common.Helpers;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace VehicleSimulator
{
    public class CronSimulator
    {
        private readonly List<Task> _tasks = new List<Task>();
        private readonly string _deviceId;
        private readonly string _deviceConnectionString;
        private readonly Random _rnd;
        private CancellationTokenSource _source = new CancellationTokenSource();
        private readonly CancellationToken _ct;

        public CronSimulator(int deviceId, string deviceConnectionString)
        {
            _deviceId = deviceId.ToString();
            _deviceConnectionString = deviceConnectionString;
            _rnd = new Random((int)DateTime.Now.Ticks);
            _ct = _source.Token;
        }

        public CronSimulator SimulateSensor(SensorTypes sensorType, TimeSpan delay)
        {
            var task = new Task(async () =>
            {
                int i = 0;
                var deviceClient = DeviceClient.CreateFromConnectionString(_deviceConnectionString, TransportType.Amqp);
                while (true)
                {
                    if (_ct.IsCancellationRequested == true)
                    {
                        _ct.ThrowIfCancellationRequested();
                    }

                    var sensor = new SensorDto
                    {
                        Name = sensorType.ToString(),
                        Quality = (short)_rnd.Next(0, 200),
                        Timestamp = DateTime.UtcNow.ToEpochTimestamp(),
                        Value = (100 - i) > 0 ? (100 - i) : 0 
                    };

                    var messageString = JsonConvert.SerializeObject(sensor);
                    var message = new Message(Encoding.ASCII.GetBytes(messageString));

                    message.Properties.Add("vehicleId", _deviceId);

                    await deviceClient.SendEventAsync(message);
                    Console.WriteLine("{0} > Sending message: {1}", DateTime.Now, messageString);
                    await Task.Delay(delay);
                    i++;
                }
            }, _ct);
            _tasks.Add(task);
            return this;
        }

        public void Simulate()
        {
            _tasks.ForEach(t => t.Start());
        }

        public void Stop()
        {
             _source.Cancel();
        }
    }
}
