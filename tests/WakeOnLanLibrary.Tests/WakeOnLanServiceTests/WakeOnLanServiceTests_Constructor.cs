using WakeOnLanLibrary.Application.Services;

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
        [InlineData("packetSender", null)]
        [InlineData("computerValidator", null)]
        [InlineData("computerFactory", null)]
        [InlineData("runspaceManager", null)]
        [InlineData("requestQueue", null)]
        [InlineData("resultCache", null)]
        [InlineData("monitorService", null)]
        [InlineData("monitorCache", null)]
        public void Constructor_ShouldThrowArgumentNullException_WhenDependenciesAreNull(
            string paramName,
            object dependency
        )
        {
            // Arrange
            var packetSender = paramName == "packetSender" ? null : MockPacketSender.Object;
            var computerValidator = paramName == "computerValidator" ? null : MockComputerValidator.Object;
            var computerFactory = paramName == "computerFactory" ? null : MockComputerFactory.Object;
            var runspaceManager = paramName == "runspaceManager" ? null : MockRunspaceManager.Object;
            var requestQueue = paramName == "requestQueue" ? null : MockRequestQueue.Object;
            var resultCache = paramName == "resultCache" ? null : ResultCache;
            var monitorService = paramName == "monitorService" ? null : MockMonitorService.Object;
            var monitorCache = paramName == "monitorCache" ? null : MonitorCache;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                new WakeOnLanService(
                    packetSender,
                    computerValidator,
                    computerFactory,
                    runspaceManager,
                    requestQueue,
                    resultCache,
                    monitorService,
                    monitorCache
                )
            );

            Assert.Equal(paramName, exception.ParamName);
        }

        #endregion
    }
}
