using AutoFixture;
using AzureFunction.DI;
using Common;
using Common.Dtos;
using Common.Factories;
using Common.Repositories;
using Common.Wrappers;
using CoreProcessor;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            _container = new ContainerBuilder()
                .AddTranscient<IMessageProcessor, MessageProcessor>()
                .AddInstance(Substitute.For<ILogger>())
                .AddInstance(_repositoryFactory)
                .AddInstance(_poisonMessageRepository)
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

    }
}
