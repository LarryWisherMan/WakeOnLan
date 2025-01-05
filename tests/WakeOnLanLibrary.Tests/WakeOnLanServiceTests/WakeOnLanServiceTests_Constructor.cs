using Microsoft.Extensions.DependencyInjection;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Application.Services;
using WakeOnLanLibrary.Core.UseCases;
using WakeOnLanLibrary.Tests.Mocks;

namespace WakeOnLanLibrary.Tests.WakeOnLanServiceTests
{
    public class WakeOnLanServiceTests_Constructor : WakeOnLanServiceTestBase
    {
        #region Constructor Tests

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Component", "WakeOnLanService")]
        public void Constructor_ShouldInitialize_WhenAllDependenciesAreProvided()
        {
            // Assert
            Assert.NotNull(Service); // Service is initialized in the base class
        }

        [Theory]
        [Trait("Category", "Unit")]
        [Trait("Component", "WakeOnLanService")]
        [InlineData("proxyRequestProcessor", typeof(IProxyRequestProcessor))]
        [InlineData("resultManager", typeof(IResultManager))]
        [InlineData("runspaceManager", typeof(IRunspaceManager))]
        [InlineData("requestQueue", typeof(IRequestQueue))]
        [InlineData("monitorService", typeof(IMonitorService))]
        public void Constructor_ShouldThrowException_WhenDependenciesAreMissing(
            string paramName,
            Type serviceType)
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddMockServices();

            // Remove the service matching the type under test
            var descriptorToRemove = services.FirstOrDefault(descriptor => descriptor.ServiceType == serviceType);
            Assert.NotNull(descriptorToRemove); // Ensure the service exists
            services.Remove(descriptorToRemove);

            var provider = services.BuildServiceProvider();

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                provider.GetRequiredService<IWakeOnLanService>()
            );


            Assert.Contains($"Unable to resolve service for type '{serviceType.FullName}' while attempting to activate '{typeof(WakeOnLanService).FullName}'", exception.Message);

        }
        #endregion
    }
}

