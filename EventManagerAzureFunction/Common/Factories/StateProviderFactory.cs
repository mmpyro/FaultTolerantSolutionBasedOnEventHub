using Common.Providers;
using Polly;
using System;

namespace Common.Factories
{
    public class StateProviderFactory : IStateProviderFactory
    {
        public IStateProvider Create(string endpoint)
        {
            return Polly.Policy.Handle<Exception>()
                    .WaitAndRetry(3, (retry) => TimeSpan.FromSeconds(1 * retry))
                    .Execute(() =>  new StateProvider(endpoint));
        }
    }
}
