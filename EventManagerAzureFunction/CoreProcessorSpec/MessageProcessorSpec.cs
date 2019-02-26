using AutoFixture;
using AzureFunction.DI;
using Common;
using Common.Classifier;
using Common.Dtos;
using Common.Factories;
using Common.Policy;
using Common.Repositories;
using Common.Wrappers;
using CoreProcessor;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Polly;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TestUtils.Helpers;
using Xunit;

namespace CoreProcessorSpec
{
    public class MessageProcessorSpec
    {
        private readonly string _id = Guid.NewGuid().ToString();
        private readonly Fixture _any = new Fixture();
        private readonly IContainer _container;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IRepository _repository;
        private readonly IPoisonMessageRepository _poisonMessageRepository;

        public MessageProcessorSpec()
        {
            _repository = Substitute.For<IRepository>();
            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _repositoryFactory.Create().Returns(_repository);
            _poisonMessageRepository = Substitute.For<IPoisonMessageRepository>();
            var policyRegistry = Substitute.For<IPolicyRegistry>();
            policyRegistry.CreateAsyncPolicies().Returns(new[]
            {
                Policy.Handle<DocumentClientException>(ex => ExceptionClassifier.IsInternalServerError(ex))
                    .RetryAsync(2),
                Policy.Handle<DocumentClientException>(ex => ExceptionClassifier.IsServiceUnavaiable(ex))
                    .RetryAsync(2),
            });

            _container = new ContainerBuilder()
                .AddTranscient<IMessageProcessor, MessageProcessor>()
                .AddInstance(Substitute.For<ILogger>())
                .AddInstance(_repositoryFactory)
                .AddInstance(_poisonMessageRepository)
                .AddInstance(policyRegistry)
                .Build();
        }

        [Fact]
        public async Task ShouldSaveDataIntoDocumentDbWhenProcessCalled()
        {
            //Given
            var messageProcessor = _container.Resolve<IMessageProcessor>();
            var eventData = Substitute.For<EventDataWrapper>();
            eventData.Body.Returns(_any.Create<SensorDto>().ToString());
            eventData.Properties.Returns(new Dictionary<string, object>
            {
                [Constants.VehicleId] = _id
            });

            //When
            await messageProcessor.ProcessAsync(new[]{
                eventData
            });

            //Then
            await _repository.Received(1).AddAsync(Arg.Is<VehicleSnapshot>(t => t.Id == _id));
        }

        [Fact]
        public async Task ShouldSaveMessageIntoPoisonTableWhenCannotBeDeserialized()
        {
            //Given
            var messageProcessor = _container.Resolve<IMessageProcessor>();
            var eventData = Substitute.For<EventDataWrapper>();
            eventData.Body.Returns(_any.Create<string>());
            eventData.Properties.Returns(new Dictionary<string, object>
            {
                [Constants.VehicleId] = _id
            });

            //When
            await messageProcessor.ProcessAsync(new[]{
                eventData
            });

            //Then
            await _poisonMessageRepository.Received(1).Save(Arg.Is(eventData));
        }

        [Fact]
        public async Task ShouldSaveVariableSnapshotWhenExceptionOccur()
        {
            //Given
            const int expectedNumberOfRetries = 3;
            var messageProcessor = _container.Resolve<IMessageProcessor>();
            var eventData = Substitute.For<EventDataWrapper>();
            eventData.Body.Returns(_any.Create<string>());
            eventData.Properties.Returns(new Dictionary<string, object>
            {
                [Constants.VehicleId] = _id
            });
            int retry = 0;

            _repository.When(t => t.AddAsync(Arg.Any<VehicleSnapshot>())).Do(_ =>
            {
                retry++;
                throw DocumentExceptionHelper.Create(_any.Create<Error>(), System.Net.HttpStatusCode.InternalServerError);
            });

            //When
            await messageProcessor.ProcessAsync(new[]{
                eventData
            });

            //Then
            Assert.Equal(expectedNumberOfRetries, retry);
            await _poisonMessageRepository.Received(1).Save(Arg.Any<VehicleSnapshot>(), Arg.Any<Exception>());
        }
    }
}
