using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Management.Automation;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Application.Interfaces.Validation;
using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Core.Interfaces;
using WakeOnLanLibrary.Core.UseCases;


namespace WakeOnLanLibrary.Tests.WakeOnLanServiceTests
{
    public class WakeOnLanServiceTests_WakeUpAndMonitorAsync : WakeOnLanServiceTestBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IWakeOnLanService _service;
        private readonly Mock<IComputerValidator> _mockComputerValidator;
        private readonly Mock<IRequestScheduler> _mockRequestQueue;
        private readonly IWakeOnLanResultCache _resultCache;
        private readonly Mock<IRunspaceManager> _mockRunspaceManager;
        private readonly Mock<IRunspacePool> _mockRunspacePool;

        public WakeOnLanServiceTests_WakeUpAndMonitorAsync()
        {
            _service = Service;
            _serviceProvider = ServiceProvider;
            _mockComputerValidator = Mock.Get(_serviceProvider.GetRequiredService<IComputerValidator>());
            _mockRequestQueue = Mock.Get(_serviceProvider.GetRequiredService<IRequestScheduler>());
            _resultCache = _serviceProvider.GetRequiredService<IWakeOnLanResultCache>();
            _mockRunspaceManager = Mock.Get(_serviceProvider.GetRequiredService<IRunspaceManager>());
            _mockRunspacePool = new Mock<IRunspacePool>();
        }

        private void ClearCaches()
        {
            _resultCache.Clear();
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Component", "WakeOnLanService")]
        public async Task WakeUpAndMonitorAsync_ShouldReturnEmptyResult_WhenProxyToTargetsIsEmpty()
        {
            // Arrange
            var proxyToTargets = new Dictionary<string, List<(string MacAddress, string ComputerName)>>();

            // Act
            var result = await _service.WakeUpAndMonitorAsync(proxyToTargets);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Component", "WakeOnLanService")]
        public async Task WakeUpAndMonitorAsync_ShouldSkipProxiesWithNoTargets()
        {
            // Arrange
            var proxyToTargets = new Dictionary<string, List<(string MacAddress, string ComputerName)>>
            {
                { "EmptyProxy", new List<(string, string)>() },
            };

            _mockRequestQueue.Setup(q => q.Schedule(It.IsAny<Func<Task>>()));

            // Act
            await _service.WakeUpAndMonitorAsync(proxyToTargets);

            // Assert
            _mockRequestQueue.Verify(q => q.Schedule(It.IsAny<Func<Task>>()), Times.Once);
        }


        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Component", "WakeOnLanService")]
        public async Task WakeUpAndMonitorAsync_ShouldStartMonitoringService()
        {
            // Arrange
            var proxyToTargets = new Dictionary<string, List<(string, string)>>
    {
        { "ValidProxy", new List<(string, string)> { ("00:11:22:33:44:55", "Target1") } }
    };

            var mockMonitorService = Mock.Get(_serviceProvider.GetRequiredService<IMonitorService>());

            mockMonitorService
            .Setup(m => m.StartMonitoringAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

            // Act
            await _service.WakeUpAndMonitorAsync(proxyToTargets);

            // Assert
            mockMonitorService.Verify(
            m => m.StartMonitoringAsync(
                It.Is<int>(attempts => attempts == 5), // Validate maxPingAttempts
                It.Is<int>(timeout => timeout == 60), // Validate timeoutInSeconds
                It.IsAny<CancellationToken>()),      // Allow any CancellationToken
            Times.Once);
        }

        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Component", "WakeOnLanService")]
        public async Task WakeUpAndMonitorAsync_ShouldAddFailureResult_WhenProxyValidationFails()
        {
            // Arrange
            var proxyToTargets = new Dictionary<string, List<(string MacAddress, string ComputerName)>>
    {
        { "InvalidProxy", new List<(string MacAddress, string ComputerName)> { ("00:11:22:33:44:55", "Target1") } }
    };

            var enqueuedTasks = new List<Func<Task>>();

            _mockRequestQueue
                .Setup(q => q.Schedule(It.IsAny<Func<Task>>()))
                .Callback<Func<Task>>(task => enqueuedTasks.Add(task)); // Capture tasks

            _mockRequestQueue
                .Setup(q => q.ExecuteScheduledTasksAsync(It.IsAny<CancellationToken>()))
                .Returns(async () =>
                {
                    foreach (var task in enqueuedTasks)
                    {
                        await task(); // Execute each enqueued task
                    }
                });

            _mockRequestQueue
                .Setup(q => q.ClearScheduledTasksAsync(It.IsAny<CancellationToken>()))
                .Returns(() =>
                {
                    enqueuedTasks.Clear(); // Clear all tasks
                    return Task.CompletedTask;
                });


            // Mock RunspaceManager to simulate runspace pool creation failure
            _mockRunspaceManager
                .Setup(r => r.GetOrCreateRunspacePool(It.IsAny<string>(), It.IsAny<PSCredential>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(new Exception("Runspace pool creation failed"));

            // Act
            var result = await _service.WakeUpAndMonitorAsync(proxyToTargets);

            // Assert
            _mockRunspaceManager.Verify(
                r => r.GetOrCreateRunspacePool(It.IsAny<string>(), It.IsAny<PSCredential>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Once); // Verify the method is called

            Assert.Single(_resultCache.GetAllKeys()); // Ensure result cache has entries
            var failureKey = _resultCache.GetAllKeys().First();
            var failureResult = _resultCache.Get(failureKey);
            Assert.False(failureResult.WolSuccess);
            Assert.Contains("Runspace pool creation failed", failureResult.ErrorMessage);
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



            _mockComputerValidator
                .Setup(v => v.Validate(It.IsAny<ProxyComputer>()))
                .Returns(new ValidationResult { IsValid = true });

            _mockRequestQueue.Setup(q => q.Schedule(It.IsAny<Func<Task>>()));

            // Act
            await _service.WakeUpAndMonitorAsync(proxyToTargets);

            // Assert
            _mockRequestQueue.Verify(q => q.Schedule(It.IsAny<Func<Task>>()), Times.Once);
        }



        [Fact]
        [Trait("Category", "Unit")]
        [Trait("Component", "WakeOnLanService")]
        public async Task WakeUpAndMonitorAsync_ShouldProcessTasksForAllProxies()
        {
            // Arrange
            var proxyToTargets = new Dictionary<string, List<(string MacAddress, string ComputerName)>>
            {
                { "Proxy1", new List<(string, string)> { ("00:11:22:33:44:55", "Target1") } },
                { "Proxy2", new List<(string, string)> { ("66:77:88:99:AA:BB", "Target2") } }
            };

            _mockRequestQueue.Setup(q => q.Schedule(It.IsAny<Func<Task>>()));

            // Act
            await _service.WakeUpAndMonitorAsync(proxyToTargets);

            // Assert
            _mockRequestQueue.Verify(q => q.Schedule(It.IsAny<Func<Task>>()), Times.Exactly(proxyToTargets.Count));
        }

    }
}


