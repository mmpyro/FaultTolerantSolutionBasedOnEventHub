using AutoFixture;
using AzureFunction.DI;
using Common;
using Common.Dtos;
using Common.Factories;
using Common.Policy;
using Common.Repositories;
using Common.Wrappers;
using CoreProcessor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestUtils.Dal;
using Xunit;

namespace CoreProcessorIntegrationTests
{
    public class MessageProcessorSpec : IDisposable
    {
        private const string PrimaryKey = "";
        private const string StorageConnectionString = "UseDevelopmentStorage=true";
        private const string EndpointUrl = "https://mmvehicle.documents.azure.com:443/";
        private const string DatabaseId = "Iot";
        private const string CollectionId = "integrationTests";
        private readonly IContainer _container;
        private readonly Fixture _any = new Fixture();
        private readonly string _id = Guid.NewGuid().ToString();
        private readonly DocumentDBContext _dbContext;
        private readonly TableContext _tableContext;

        public MessageProcessorSpec()
        {
            var configuration = Substitute.For<IConfigurationRoot>();
            configuration[Constants.DocumentDb.PrimaryKey].Returns(PrimaryKey);
            configuration[Constants.DocumentDb.EndpointUrl].Returns(EndpointUrl);
            configuration[Constants.Storage.ConnectionString].Returns(StorageConnectionString);
            configuration[Constants.DocumentDb.DatabaseId].Returns(DatabaseId);
            configuration[Constants.DocumentDb.CollectionId].Returns(CollectionId);

            _container = new ContainerBuilder()
                .AddTranscient<IRepositoryFactory, DocumentDbFactory>()
                .AddTranscient<IMessageProcessor, MessageProcessor>()
                .AddTranscient<IPoisonMessageRepository, PoisonMessageRepository>()
                .AddTranscient<IPolicyRegistry, PolicyRegistry>()
                .AddInstance(Substitute.For<ILogger>())
                .AddInstance(configuration)
                .Build();

            _dbContext = new DocumentDBContext(new Uri(EndpointUrl), PrimaryKey,
                DatabaseId, CollectionId);

            _tableContext = new TableContext(StorageConnectionString, Constants.Storage.PoisonTableName);
        }

        public void Dispose()
        {
            _dbContext.Reset().Wait();
            _tableContext.Reset<PoisonMessageEntity>().Wait();
        }

        [Fact]
        public async Task ShouldAddNewRecordIntoCollection()
        {
            //Given
            const string sensorName = "fuelLevel";
            decimal expectedValue = _any.Create<decimal>();
            var eventData = CreateEventData(_id);
            eventData.Body.Returns(_any.Build<SensorDto>().With(t => t.Name, sensorName)
                .With(t => t.Quality, 100)
                .With(t => t.Value, expectedValue)
                .Create().ToString());
            var messageProcessor = _container.Resolve<IMessageProcessor>();

            //When
            await messageProcessor.ProcessAsync(new[] { eventData});

            //Then
            var doc = _dbContext.Get<VehicleSnapshot>().FirstOrDefault(t => t.Id == _id);
            Assert.NotNull(doc);
            Assert.NotNull(doc.Sensors[sensorName]);
            Assert.Equal(expectedValue, doc.Sensors[sensorName].Value);
        }

        [Fact]
        public async Task ShouldUpdateVehicleSnapshotWithTheNewerValues()
        {
            //Given
            var eventData1 = CreateEventData(_id);
            var eventData2 = CreateEventData(_id);
            eventData1.Body.Returns(_any.Build<SensorDto>().With(t => t.Name, "fuelLevel")
                .With(t => t.Quality, 100)
                .With(t => t.Value, (decimal)99.6)
                .Create().ToString());

            eventData2.Body.Returns(_any.Build<SensorDto>().With(t => t.Name, "oilLevel")
                .With(t => t.Quality, 100)
                .With(t => t.Value, (decimal)88.5)
                .Create().ToString());
            var messageProcessor = _container.Resolve<IMessageProcessor>();

            //When
            await messageProcessor.ProcessAsync(new[] { eventData1 });
            await messageProcessor.ProcessAsync(new[] { eventData2 });

            //Then
            var doc = _dbContext.Get<VehicleSnapshot>().FirstOrDefault(t => t.Id == _id.ToString());
            Assert.NotNull(doc);
            Assert.True(doc.Sensors.Keys.All(t => t== "fuelLevel" || t == "oilLevel"));
            Assert.Equal((decimal)99.6, doc.Sensors["fuelLevel"].Value);
            Assert.Equal((decimal)88.5, doc.Sensors["oilLevel"].Value);
        }

        [Fact]
        public async Task ShouldReplaceSensorValueWhenTimestampIsNewer()
        {
            //Given
            const long oldTimestamp = 10;
            const long newTimestamp = 100;
            var eventData1 = CreateEventData(_id);
            var eventData2 = CreateEventData(_id);
            var eventData3 = CreateEventData(_id);
            var eventData4 = CreateEventData(_id);

            eventData1.Body.Returns(_any.Build<SensorDto>().With(t => t.Name, "fuelLevel")
                .With(t => t.Quality, 100)
                .With(t => t.Value, (decimal)99.6)
                .With(t => t.Timestamp, oldTimestamp)
                .Create().ToString());

            eventData2.Body.Returns(_any.Build<SensorDto>().With(t => t.Name, "oilLevel")
                .With(t => t.Quality, 100)
                .With(t => t.Value, (decimal)88.5)
                .With(t => t.Timestamp, oldTimestamp)
                .Create().ToString());

            eventData3.Body.Returns(_any.Build<SensorDto>().With(t => t.Name, "fuelLevel")
                .With(t => t.Quality, 100)
                .With(t => t.Value, 90)
                .With(t => t.Timestamp, newTimestamp)
                .Create().ToString());

            eventData4.Body.Returns(_any.Build<SensorDto>().With(t => t.Name, "oilLevel")
                .With(t => t.Quality, 100)
                .With(t => t.Value, 80)
                .With(t => t.Timestamp, 8)
                .Create().ToString());
            var messageProcessor = _container.Resolve<IMessageProcessor>();

            //When
            await messageProcessor.ProcessAsync(new[] { eventData1, eventData2 });
            await messageProcessor.ProcessAsync(new[] { eventData3, eventData4 });

            //Then
            var doc = _dbContext.Get<VehicleSnapshot>().FirstOrDefault(t => t.Id == _id.ToString());
            Assert.NotNull(doc);
            Assert.True(doc.Sensors.Keys.All(t => t == "fuelLevel" || t == "oilLevel"));
            Assert.Equal((decimal)90, doc.Sensors["fuelLevel"].Value);
            Assert.Equal((decimal)88.5, doc.Sensors["oilLevel"].Value);
        }

        [Fact]
        public async Task ShouldSaveInavlidMessageIntoPoisonMessageTable()
        {
            //Given
            var eventData = CreateEventData(_id);
            eventData.Body.Returns(_any.Create<string>());
            var messageProcessor = _container.Resolve<IMessageProcessor>();

            //When
            await messageProcessor.ProcessAsync(new[] { eventData });
            var poisonMessages = await _tableContext.Get<PoisonMessageEntity>(_id);

            //Then
            Assert.True(poisonMessages.Any());
        }

        private EventDataWrapper CreateEventData(string id)
        {
            var eventData = Substitute.For<EventDataWrapper>();
            eventData.Properties.Returns(new Dictionary<string, object>
            {
                [Constants.VehicleId] = _id.ToString()
            });
            return eventData;
        }
    }
}
