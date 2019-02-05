using AzureFunction.DI;
using Common.Factories;
using CoreProcessor;
using Microsoft.Extensions.DependencyInjection;

namespace EventManagerFunc
{
    public class EventManagerModule : IModule
    {
        public void Load(IServiceCollection services)
        {
            services.AddTransient<IRepositoryFactory, DocumentDbFactory>();
            services.AddTransient<IMessageProcessor, MessageProcessor>();
        }
    }
}
