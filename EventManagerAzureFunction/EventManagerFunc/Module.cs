using AzureFunction.DI;
using Common.Factories;
using Common.Policy;
using Common.Repositories;
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
            services.AddTransient<IPoisonMessageRepository, PoisonMessageRepository>();
            services.AddTransient<IPolicyRegistry, PolicyRegistry>();
        }
    }
}
