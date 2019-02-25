using AzureFunction.DI;
using Common.Wrappers;
using CoreProcessor;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace EventManagerFunc
{
    public static class SnapshotManagerFunc
    {
        private static readonly IContainerBuilder _containerBuilder = new ContainerBuilder()
            .AddModule(new EventManagerModule());


        [FunctionName("SnapshotManager-WithoutErrorHandling")]
        public static void Run([EventHubTrigger("vehicles", Connection = "vehicleHubConnectionString", ConsumerGroup = "without-error-hndling")] EventData[] messages,
            ILogger log, ExecutionContext context)
        {
            var config = new ConfigurationBuilder()
                 .SetBasePath(context.FunctionAppDirectory)
                 .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                 .AddEnvironmentVariables()
                 .Build();

            var container = _containerBuilder
                .AddInstance(log)
                .AddInstance(config)
                .Build();

            try
            {
                var messageProcessor = container.Resolve<IMessageProcessor>();
                messageProcessor.ProcessAsync(messages.Select(m => new EventDataWrapper(m)).ToArray());
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Malfunction unable to process data.");
            }
        }
    }
}
