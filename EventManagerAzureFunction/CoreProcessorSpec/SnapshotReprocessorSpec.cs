using AutoFixture;
using AzureFunction.DI;
using Common.Dtos;
using Common.Factories;
using Common.Repositories;
using CoreProcessor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace CoreProcessorSpec
{
    public class SnapshotReprocessorSpec
    {
        private readonly Fixture _any = new Fixture();
        private readonly IContainer _container;
        private readonly IConfigurationRoot _configuration;
        private readonly IRepository _repository;
        private readonly IPoisonMessageRepository _poisonMessageRepository;
        private readonly string _id = Guid.NewGuid().ToString();

        public SnapshotReprocessorSpec()
        {
            var repositryFactory = Substitute.For<IRepositoryFactory>();
            _repository = Substitute.For<IRepository>();
            _poisonMessageRepository = Substitute.For<IPoisonMessageRepository>();
            _configuration = Substitute.For<IConfigurationRoot>();
            repositryFactory.Create().Returns(_repository);
            
            _container = new ContainerBuilder()
                .AddInstance(Substitute.For<ILogger>())
                .AddInstance(repositryFactory)
                .AddInstance(_poisonMessageRepository)
                .AddInstance(_configuration)
                .AddTranscient<ISnapshotReprocessor, SnapshotReprocessor>()
                .Build();
        }

        [Fact]
        public async Task ShouldAddSectionToPresentSnapshotWhichHasNewerTimestamp()
        {
            //Given
            VehicleSnapshot finalSnapshot = null;
            var reprocessor = _container.Resolve<ISnapshotReprocessor>();
            _repository.Get(Arg.Any<string>()).Returns(new VehicleSnapshot
            {
                Id = _id,
                Sensors =
                {
                    ["fueal"] = new VehicleSensorData
                    {
                        Quality = 100,
                        Timestamp = 1,
                        Value = 1
                    },
                    ["oil"] = new VehicleSensorData
                    {
                        Quality = 100,
                        Timestamp = 1,
                        Value = 1
                    }
                }
            });
            _repository.When(x => x.AddAsync(Arg.Any<VehicleSnapshot>())).Do(x => finalSnapshot = x.ArgAt<VehicleSnapshot>(0) );

            _poisonMessageRepository.GetUnprocessedSnapshots().Returns(Task.FromResult(CreateSnapshot()));

            //When
            await reprocessor.Reprocess();

            //Then
            await _poisonMessageRepository.Received(1).DeleteSnapshot(Arg.Any<VehicleSnapshotEntity>());
            Assert.Equal(3, finalSnapshot.Sensors.Count);
        }

        private List<VehicleSnapshotEntity> CreateSnapshot()
        {
            var snpshot = new VehicleSnapshot
            {
                Id = _id,
                Sensors =
                {
                    ["fueal"] = new VehicleSensorData
                    {
                        Quality = 100,
                        Timestamp = 2,
                        Value = 2
                    },
                    ["oil"] = new VehicleSensorData
                    {
                        Quality = 100,
                        Timestamp = 1,
                        Value = 2
                    },
                    ["break"] = new VehicleSensorData
                    {
                        Quality = 100,
                        Timestamp = 1,
                        Value = 1
                    },
                    ["powerStering"] = new VehicleSensorData
                    {
                        Quality = 99,
                        Timestamp = 1,
                        Value = 1
                    }
                }
            };

            return new List<VehicleSnapshotEntity>
            {
                new VehicleSnapshotEntity
                {
                    VehicleSnapshotJson = JsonConvert.SerializeObject(snpshot)
                }
            };
        }
    }
}
