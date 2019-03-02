using Common.Events;
using Common.Factories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace Common.CircutBreaker
{
    public class CircutBreaker : ICircutBreaker
    {
        private readonly IConfigurationRoot _configuration;
        private readonly IEventSender _eventSender;
        private readonly IStateProviderFactory _stateProviderFactory;
        private readonly ILogger _logger;

        public CircutBreaker(IConfigurationRoot configuration, IEventSender eventSender,
            IStateProviderFactory stateProviderFactory, ILogger logger)
        {
            _configuration = configuration;
            _eventSender = eventSender;
            _stateProviderFactory = stateProviderFactory;
            _logger = logger;
        }

        public void BreakFlow(int exceptionsAllowed, string functionName)
        {
            try
            {
                var stateProvider = _stateProviderFactory.Create(_configuration[Constants.Cache.Endpoint]);
                var exceptionState = stateProvider.GetState(functionName);

                if (!exceptionState.HasValue)
                {
                    stateProvider.SetState(functionName, TimeSpan.FromMinutes(3));
                }
                else if (exceptionState.Value > exceptionsAllowed)
                {
                    _eventSender.SendEvent(functionName);
                    stateProvider.Reset(functionName);
                }
                else
                {
                    stateProvider.IncrementState(functionName);
                }
            }
            catch(Exception ex)
            {
                _logger?.LogError(ex, "Unable to break circut");
            }
        }
    }
}
