using System;
using System.Threading.Tasks;
using AzureFunction.DI;
using CoreProcessor;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventManagerFunc
{
    public static class ReprocesorFunc
    {
        private static readonly IContainerBuilder _containerBuilder = new ContainerBuilder()
            .AddModule(new EventManagerModule());

        [FunctionName("ReprocesorFunc")]
        public static async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            try
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

                var reprocessor = container.Resolve<ISnapshotReprocessor>();
                await reprocessor.Reprocess();
            }
            catch(Exception ex)
            {
                log.LogError(ex, "Malfunction unable to process data.");
            }
        }
    }
}
