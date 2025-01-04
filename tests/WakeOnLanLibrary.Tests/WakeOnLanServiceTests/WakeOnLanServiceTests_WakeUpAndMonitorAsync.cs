using Moq;
using System.Management.Automation;
using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Core.Entities;

namespace WakeOnLanLibrary.Tests.WakeOnLanServiceTests
{
    public class WakeOnLanServiceTests_WakeUpAndMonitorAsync : WakeOnLanServiceTestBase
    {

        public WakeOnLanServiceTests_WakeUpAndMonitorAsync()
        {
            ClearCaches();
        }


        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Component", "WakeOnLanService")]
        public async Task WakeUpAndMonitorAsync_ShouldReturnEmptyResult_WhenProxyToTargetsIsEmpty()
        {
            // Arrange
            var proxyToTargets = new Dictionary<string, List<(string MacAddress, string ComputerName)>>();

            // Act
            var result = await Service.WakeUpAndMonitorAsync(proxyToTargets);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Component", "WakeOnLanService")]
        public async Task WakeUpAndMonitorAsync_ShouldAddFailureResult_WhenProxyValidationFails()
        {
            // Arrange
            var proxyToTargets = new Dictionary<string, List<(string MacAddress, string ComputerName)>>
    {
        { "InvalidProxy", new List<(string MacAddress, string ComputerName)>
            {
                ("00:11:22:33:44:55", "Target1")
            }
        }
    };

            MockComputerValidator
                .Setup(v => v.Validate(It.IsAny<ProxyComputer>()))
                .Returns(new ValidationResult { IsValid = false, Message = "Invalid proxy" });

            // Act
            var result = await Service.WakeUpAndMonitorAsync(proxyToTargets);

            // Assert
            Assert.Single(ResultCache.GetAllKeys());
            var failureKey = ResultCache.GetAllKeys().First();
            var failureResult = ResultCache.Get(failureKey);
            Assert.False(failureResult.WolSuccess);
            Assert.Equal("Invalid proxy", failureResult.ErrorMessage);
        }


        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Component", "WakeOnLanService")]
        public async Task WakeUpAndMonitorAsync_ShouldEnqueueRequest_ForValidProxyAndTargets()
        {
            // Arrange
            var proxyToTargets = new Dictionary<string, List<(string MacAddress, string ComputerName)>>
            {
                { "ValidProxy", new List<(string, string)> { ("00:11:22:33:44:55", "Target1") } }
            };

            MockComputerValidator
                .Setup(v => v.Validate(It.IsAny<ProxyComputer>()))
                .Returns(new ValidationResult { IsValid = true });

            MockRequestQueue.Setup(q => q.Enqueue(It.IsAny<Func<Task>>()));

            // Act
            await Service.WakeUpAndMonitorAsync(proxyToTargets);

            // Assert
            MockRequestQueue.Verify(q => q.Enqueue(It.IsAny<Func<Task>>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Component", "WakeOnLanService")]
        public async Task WakeUpAndMonitorAsync_ShouldAddFailureResult_WhenRunspacePoolCreationFails()
        {
            // Arrange
            var proxyToTargets = new Dictionary<string, List<(string MacAddress, string ComputerName)>>
    {
        { "ProxyWithError", new List<(string, string)> { ("00:11:22:33:44:55", "Target1") } }
    };

            MockComputerValidator
                .Setup(v => v.Validate(It.IsAny<ProxyComputer>()))
                .Returns(new ValidationResult { IsValid = true });

            MockRunspaceManager
                .Setup(r => r.GetOrCreateRunspacePool(It.IsAny<string>(), It.IsAny<PSCredential>(), 1, 5))
                .Throws(new Exception("Runspace pool creation failed"));

            MockRequestQueue
                .Setup(q => q.Enqueue(It.IsAny<Func<Task>>()))
                .Callback<Func<Task>>(async task => await task());

            // Act
            var result = await Service.WakeUpAndMonitorAsync(proxyToTargets);

            // Assert
            Assert.NotEmpty(result);
            var failure = result.First();
            Assert.False(failure.WolSuccess);
            Assert.Contains("Runspace pool creation failed", failure.ErrorMessage);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Component", "WakeOnLanService")]
        public async Task WakeUpAndMonitorAsync_ShouldStartMonitoring_WhenRequestsAreProcessed()
        {
            // Arrange
            var proxyToTargets = new Dictionary<string, List<(string MacAddress, string ComputerName)>>
            {
                { "ValidProxy", new List<(string, string)> { ("00:11:22:33:44:55", "Target1") } }
            };

            MockComputerValidator
                .Setup(v => v.Validate(It.IsAny<ProxyComputer>()))
                .Returns(new ValidationResult { IsValid = true });

            MockRequestQueue.Setup(q => q.Enqueue(It.IsAny<Func<Task>>()));

            MockMonitorService
                .Setup(m => m.StartMonitoringAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await Service.WakeUpAndMonitorAsync(proxyToTargets);

            // Assert
            MockMonitorService.Verify(m => m.StartMonitoringAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Component", "WakeOnLanService")]
        public async Task WakeUpAndMonitorAsync_ShouldReturnAllResults_FromResultCache()
        {
            // Arrange
            var proxyToTargets = new Dictionary<string, List<(string MacAddress, string ComputerName)>>
            {
                { "ValidProxy", new List<(string, string)> { ("00:11:22:33:44:55", "Target1") } }
            };

            MockComputerValidator
                .Setup(v => v.Validate(It.IsAny<ProxyComputer>()))
                .Returns(new ValidationResult { IsValid = true });

            MockRequestQueue.Setup(q => q.Enqueue(It.IsAny<Func<Task>>()));

            ResultCache.AddOrUpdate("Target1:00:11:22:33:44:55", new WakeOnLanReturn
            {
                TargetComputerName = "Target1",
                TargetMacAddress = "00:11:22:33:44:55",
                WolSuccess = true
            });

            // Act
            var result = await Service.WakeUpAndMonitorAsync(proxyToTargets);

            // Assert
            Assert.NotEmpty(result);
            var successResult = result.First();
            Assert.True(successResult.WolSuccess);
            Assert.Equal("Target1", successResult.TargetComputerName);
            Assert.Equal("00:11:22:33:44:55", successResult.TargetMacAddress);
        }
    }
}

