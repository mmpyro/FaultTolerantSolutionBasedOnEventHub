using AzureFunction.DI;
using CoreProcessor;
using EventManagerFunc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using Xunit;

namespace CoreProcessorSpec
{
    public class ModuleSpec
    {

        [Theory]
        [InlineData(typeof(IMessageProcessor))]
        public void ShouldRespolveSpecificTypeFromContainer(Type typeToResolve)
        {
            //Given
            var log = Substitute.For<ILogger>();
            var configuration = Substitute.For<IConfigurationRoot>();
            var container = new ContainerBuilder()
                                .AddModule(new EventManagerModule())
                                .AddInstance(log)
                                .AddInstance(configuration)
                                .Build();
            //When
            var resolvedType = container.Resolve(typeToResolve);

            //Then
            Assert.NotNull(resolvedType);
            Assert.IsAssignableFrom(typeToResolve, resolvedType);
        }
    }
}
