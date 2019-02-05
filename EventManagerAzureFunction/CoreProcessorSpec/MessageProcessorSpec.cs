using AutoFixture;
using AzureFunction.DI;
using Common;
using Common.Dtos;
using Common.Factories;
using Common.Repositories;
using Common.Wrappers;
using CoreProcessor;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CoreProcessorSpec
{
    public class MessageProcessorSpec
    {
        private readonly Fixture _any = new Fixture();
        private readonly IContainer _container;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IRepository _repository;

        public MessageProcessorSpec()
        {
            _repository = Substitute.For<IRepository>();
            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _repositoryFactory.Create().Returns(_repository);
            _container = new ContainerBuilder()
                .AddTranscient<IMessageProcessor, MessageProcessor>()
                .AddInstance(_repositoryFactory)
                .Build();
        }

        [Fact]
        public async Task ShouldSaveDataIntoDocumentDbWhenProcessCalled()
        {
            //Given
            var id = Guid.NewGuid().ToString();
            var messageProcessor = _container.Resolve<IMessageProcessor>();
            var eventData = Substitute.For<EventDataWrapper>();
            eventData.Body.Returns(_any.Create<SensorDto>().ToString());
            eventData.Properties.Returns(new Dictionary<string, object>
            {
                [Constants.VehicleId] = id
            });

            //When
            await messageProcessor.ProcessAsync(new[]{
                eventData
            });

            //Then
            await _repository.Received(1).AddAsync(Arg.Is<VehicleSnapshot>(t => t.Id == id));
        }


    }
}
