using Common.Clients;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Events
{
    public class EventGridSender : IEventSender
    {
        private readonly IConfigurationRoot _configurationRoot;
        private readonly IHttpClient _httpClient;

        public EventGridSender(IConfigurationRoot configurationRoot, IHttpClient httpClient)
        {
            _configurationRoot = configurationRoot;
            _httpClient = httpClient;
        }

        public async Task SendEvent(string functionName)
        {
            var sasKey = _configurationRoot[Constants.EventGrid.Key];
            var topic = _configurationRoot[Constants.EventGrid.Topic];
            var uri = _configurationRoot[Constants.EventGrid.Uri];

            var eventList = new List<EventGridEvent>
            {
                new EventGridEvent
                {
                    Subject = $"Stop {functionName}",
                    EventType = "stop",
                    EventTime = DateTime.UtcNow,
                    Id = Guid.NewGuid().ToString()
                }
            };

            await _httpClient.PostAsync(new Dtos.CircutBreakerEventDto
            {
                Key = sasKey,
                Topic = topic,
                Data = eventList,
                Uri = uri
            });
        }
    }
}
