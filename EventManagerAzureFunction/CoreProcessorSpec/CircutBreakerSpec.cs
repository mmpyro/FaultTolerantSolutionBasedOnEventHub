using AutoFixture;
using AzureFunction.DI;
using Common;
using Common.CircutBreaker;
using Common.Clients;
using Common.Dtos;
using Common.Events;
using Common.Factories;
using Common.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Xunit;

namespace CoreProcessorSpec
{
    public class CircutBreakerSpec
    {
        private readonly IContainer _container;
        private readonly IHttpClient _httpClient;
        private readonly IStateProvider _stateProvider;
        private readonly IConfigurationRoot _configuration;
        private readonly Fixture _any = new Fixture();
        private const string FunctionName = "snapshot-manager-circut-breaker";
        private const int TTL = 3;

        public CircutBreakerSpec()
        {
            _httpClient = Substitute.For<IHttpClient>();
            _configuration = Substitute.For<IConfigurationRoot>();
            var stateProviderFactory = Substitute.For<IStateProviderFactory>();
            _stateProvider = Substitute.For<IStateProvider>();
            stateProviderFactory.Create(Arg.Any<string>()).Returns(_stateProvider);
            _configuration[Constants.EventGrid.Key].Returns(_any.Create<string>());
            _configuration[Constants.EventGrid.Topic].Returns(_any.Create<string>());
            _configuration[Constants.EventGrid.Uri].Returns(_any.Create<string>());
            _configuration[Constants.Cache.Endpoint].Returns(_any.Create<string>());

            _container = new ContainerBuilder()
                .AddTranscient<ICircutBreaker, CircutBreaker>()
                .AddTranscient<IEventSender, EventGridSender>()
                .AddInstance(_httpClient)
                .AddInstance(stateProviderFactory)
                .AddInstance(Substitute.For<ILogger>())
                .AddInstance(_configuration)
                .Build();
        }

        [Fact]
        public async Task ShouldSendNotificationWhenNumberOfErrorIsGreaterThanAcceptedLimit()
        {
            //Given
            const string eventGridUri = "https://circutbreaker-eventgrid.westeurope-1.eventgrid.azure.net/api/events";
            var circutBreaker = _container.Resolve<ICircutBreaker>();
            _stateProvider.GetState(FunctionName).Returns(2);
            _configuration[Constants.EventGrid.Uri].Returns(eventGridUri);

            //When
            circutBreaker.BreakFlow(exceptionsAllowed: 1, functionName: FunctionName);

            //Then
            await _httpClient.Received(1).PostAsync(Arg.Is<CircutBreakerEventDto>(x => x.Uri == eventGridUri));
            _stateProvider.Received(1).Reset(FunctionName);

        }

        [Fact]
        public void ShouldSetInitialValueWhenKeyIsNotInCache()
        {
            //Given
            var circutBreaker = _container.Resolve<ICircutBreaker>();
            _stateProvider.GetState(FunctionName).Returns((long?) null);

            //When
            circutBreaker.BreakFlow(exceptionsAllowed: 1, functionName: FunctionName);

            //Then
            _stateProvider.Received(1).SetState(FunctionName, Arg.Is<TimeSpan>(t => t.TotalMinutes == TTL));
        }

        [Fact]
        public void ShouldIncrementStateValueWhenExceptionOccurs()
        {
            //Given
            int value = 0;
            var circutBreaker = _container.Resolve<ICircutBreaker>();
            _stateProvider.GetState(FunctionName).Returns(1);
            _stateProvider.When(x => x.IncrementState(FunctionName)).Do(_ => value++);

            //When
            circutBreaker.BreakFlow(exceptionsAllowed: 1, functionName: FunctionName);

            //Then
            Assert.Equal(1, value);
        }
    }
}
