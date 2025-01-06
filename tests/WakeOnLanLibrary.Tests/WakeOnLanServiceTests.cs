using Autofac.Extras.Moq;
using Microsoft.Extensions.Options;
using Moq;
using System.Management.Automation;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Application.Services;


namespace WakeOnLanLibrary.Tests
{
    public class WakeOnLanServiceTests
    {

        [Theory]
        [InlineData(null, "resultManager", "requestScheduler", "monitoringManager", "taskRunner", "config")]
        [InlineData("proxyRequestProcessor", null, "requestScheduler", "monitoringManager", "taskRunner", "config")]
        [InlineData("proxyRequestProcessor", "resultManager", null, "monitoringManager", "taskRunner", "config")]
        [InlineData("proxyRequestProcessor", "resultManager", "requestScheduler", null, "taskRunner", "config")]
        [InlineData("proxyRequestProcessor", "resultManager", "requestScheduler", "monitoringManager", null, "config")]
        [InlineData("proxyRequestProcessor", "resultManager", "requestScheduler", "monitoringManager", "taskRunner", null)]
        public void Constructor_ShouldThrowArgumentNullException_WhenDependenciesAreNull(
        string proxyRequestProcessorParam,
        string resultManagerParam,
        string requestSchedulerParam,
        string monitoringManagerParam,
        string taskRunnerParam,
        string configParam)
        {
            // Arrange
            var proxyRequestProcessor = proxyRequestProcessorParam == null ? null : Mock.Of<IProxyRequestProcessor>();
            var resultManager = resultManagerParam == null ? null : Mock.Of<IResultManager>();
            var requestScheduler = requestSchedulerParam == null ? null : Mock.Of<IRequestScheduler>();
            var monitoringManager = monitoringManagerParam == null ? null : Mock.Of<IMonitoringManager>();
            var taskRunner = taskRunnerParam == null ? null : Mock.Of<ITaskRunner>();
            var config = configParam == null ? null : Options.Create(new WakeOnLanConfiguration());

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new WakeOnLanService(proxyRequestProcessor, resultManager, requestScheduler, monitoringManager, taskRunner, config));
        }

        [Fact]
        public void Constructor_ShouldSubscribeToMonitoringCompleted()
        {
            using (var mock = AutoMock.GetLoose())
            {
                // Arrange


                var config = new WakeOnLanConfiguration
                {
                    DefaultPort = 9,
                    MaxPingAttempts = 5,
                    DefaultTimeoutInSeconds = 30,
                    RunspacePoolMinThreads = 1,
                    RunspacePoolMaxThreads = 5
                };

                mock.Mock<IOptions<WakeOnLanConfiguration>>()
                    .Setup(x => x.Value)
                    .Returns(config);


                var monitoringManager = mock.Mock<IMonitoringManager>();
                var wakeOnLanService = mock.Create<WakeOnLanService>();

                // Act
                monitoringManager.Raise(m => m.MonitoringCompleted += null, "Computer1", true, "Error");

                // Assert
                mock.Mock<IResultManager>().Verify(r => r.UpdateMonitoringResult("Computer1", true, "Error"), Times.Once);
            }
        }


        [Fact]
        public async Task WakeUpAndMonitorAsync_ShouldReturnResults()
        {
            using (var mock = AutoMock.GetLoose())
            {
                // Arrange
                var proxyToTargets = new Dictionary<string, List<(string MacAddress, string ComputerName)>>
                        {
                            { "Proxy1", new List<(string, string)> { ("00:11:22:33:44:55", "Computer1") } }
                        };

                var expectedResults = new List<WakeOnLanReturn>
                        {
                            new WakeOnLanReturn
                            {
                                TargetComputerName = "Computer1",
                                TargetMacAddress = "00:11:22:33:44:55",
                                ProxyComputerName = "Proxy1",
                                Port = 9,
                                Timestamp = DateTime.Now,
                                RequestSent = true,
                                WolSuccess = true,
                                ErrorMessage = null
                            }
                        };

                var config = new WakeOnLanConfiguration
                {
                    DefaultPort = 9,
                    MaxPingAttempts = 5,
                    DefaultTimeoutInSeconds = 30,
                    RunspacePoolMinThreads = 1,
                    RunspacePoolMaxThreads = 5
                };

                mock.Mock<IOptions<WakeOnLanConfiguration>>()
                    .Setup(x => x.Value)
                    .Returns(config);

                mock.Mock<IResultManager>()
                    .Setup(r => r.GetAllResults())
                    .Returns(expectedResults);

                var wakeOnLanService = mock.Create<WakeOnLanService>();

                // Act
                var results = await wakeOnLanService.WakeUpAndMonitorAsync(proxyToTargets);

                // Assert
                Assert.NotNull(results);
                Assert.Equal(expectedResults, results);
            }
        }

        [Fact]
        public async Task WakeUpAndMonitorAsync_ShouldNotScheduleTasks_WhenProxyToTargetsIsEmpty()
        {
            using (var mock = AutoMock.GetLoose())
            {
                // Arrange
                var proxyToTargets = new Dictionary<string, List<(string MacAddress, string ComputerName)>>();

                var config = new WakeOnLanConfiguration
                {
                    DefaultPort = 9,
                    MaxPingAttempts = 5,
                    DefaultTimeoutInSeconds = 30,
                    RunspacePoolMinThreads = 1,
                    RunspacePoolMaxThreads = 5
                };

                mock.Mock<IOptions<WakeOnLanConfiguration>>()
                    .Setup(x => x.Value)
                    .Returns(config);


                var wakeOnLanService = mock.Create<WakeOnLanService>();

                // Act
                await wakeOnLanService.WakeUpAndMonitorAsync(proxyToTargets);

                // Assert
                mock.Mock<IRequestScheduler>().Verify(
                 s => s.Schedule(It.IsAny<Func<Task>>()),
                 Times.Never
);
            }
        }

        [Fact]
        public async Task WakeUpAndMonitorAsync_ShouldExecuteScheduledTasks()
        {
            using (var mock = AutoMock.GetLoose())
            {
                // Arrange
                var proxyToTargets = new Dictionary<string, List<(string MacAddress, string ComputerName)>>
        {
            { "Proxy1", new List<(string, string)> { ("00:11:22:33:44:55", "Computer1") } }
        };

                var config = new WakeOnLanConfiguration
                {
                    DefaultPort = 9,
                    MaxPingAttempts = 5,
                    DefaultTimeoutInSeconds = 30,
                    RunspacePoolMinThreads = 1,
                    RunspacePoolMaxThreads = 5
                };

                mock.Mock<IOptions<WakeOnLanConfiguration>>()
                    .Setup(x => x.Value)
                    .Returns(config);

                var taskCompleted = false;

                mock.Mock<IRequestScheduler>()
                    .Setup(s => s.Schedule(It.IsAny<Func<Task>>()))
                    .Callback<Func<Task>>(async func =>
                    {
                        await func();
                        taskCompleted = true;
                    });

                var wakeOnLanService = mock.Create<WakeOnLanService>();

                // Act
                await wakeOnLanService.WakeUpAndMonitorAsync(proxyToTargets);

                // Assert
                Assert.True(taskCompleted);
            }
        }


        [Fact]
        public async Task WakeUpAndMonitorAsync_ShouldHandleExceptionsInProxyProcessing()
        {
            using (var mock = AutoMock.GetLoose())
            {
                // Arrange
                var proxyToTargets = new Dictionary<string, List<(string MacAddress, string ComputerName)>>
        {
            { "Proxy1", new List<(string, string)> { ("00:11:22:33:44:55", "Computer1") } }
        };

                var config = new WakeOnLanConfiguration
                {
                    DefaultPort = 9,
                    MaxPingAttempts = 5,
                    DefaultTimeoutInSeconds = 30,
                    RunspacePoolMinThreads = 1,
                    RunspacePoolMaxThreads = 5
                };

                mock.Mock<IOptions<WakeOnLanConfiguration>>()
                    .Setup(x => x.Value)
                    .Returns(config);

                var taskCompletionSource = new TaskCompletionSource();

                mock.Mock<IRequestScheduler>()
                    .Setup(s => s.Schedule(It.IsAny<Func<Task>>()))
                    .Callback<Func<Task>>(async func =>
                    {
                        try
                        {
                            await func();
                        }
                        finally
                        {
                            taskCompletionSource.SetResult();
                        }
                    });

                mock.Mock<IProxyRequestProcessor>()
                    .Setup(p => p.ProcessProxyRequestsAsync(
                        It.IsAny<string>(),
                        It.IsAny<List<(string, string)>>(),
                        It.IsAny<int>(),
                        It.IsAny<PSCredential>(),
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<int>()))
                    .ThrowsAsync(new Exception("Test Exception"));

                mock.Mock<IResultManager>()
                    .Setup(r => r.AddFailureResults(
                        It.IsAny<string>(),
                        It.IsAny<List<(string, string)>>(),
                        It.IsAny<int>(),
                        It.IsAny<string>()))
                    .Verifiable();

                var wakeOnLanService = mock.Create<WakeOnLanService>();

                // Act
                await wakeOnLanService.WakeUpAndMonitorAsync(proxyToTargets);

                // Wait for the scheduled task to complete
                await taskCompletionSource.Task;

                // Assert
                mock.Mock<IResultManager>().Verify(
                    r => r.AddFailureResults(
                        "Proxy1",
                        proxyToTargets["Proxy1"],
                        config.DefaultPort,
                        It.Is<string>(msg => msg.Contains("Test Exception"))),
                    Times.Once);
            }
        }

        [Fact]
        public async Task WakeUpAndMonitorAsync_ShouldHandleNullProxyToTargets()
        {
            using (var mock = AutoMock.GetLoose())
            {
                // Arrange
                var config = new WakeOnLanConfiguration
                {
                    DefaultPort = 9,
                    MaxPingAttempts = 5,
                    DefaultTimeoutInSeconds = 30,
                    RunspacePoolMinThreads = 1,
                    RunspacePoolMaxThreads = 5
                };

                mock.Mock<IOptions<WakeOnLanConfiguration>>()
                    .Setup(x => x.Value)
                    .Returns(config);

                var wakeOnLanService = mock.Create<WakeOnLanService>();

                // Act
                var results = await wakeOnLanService.WakeUpAndMonitorAsync(null);

                // Assert
                Assert.Empty(results);
                mock.Mock<IRequestScheduler>().Verify(s => s.Schedule(It.IsAny<Func<Task>>()), Times.Never);
            }
        }


        [Fact]
        public async Task WakeUpAndMonitorAsync_ShouldScheduleTasksForProxies()
        {
            using (var mock = AutoMock.GetLoose())
            {
                // Arrange
                var proxyToTargets = new Dictionary<string, List<(string MacAddress, string ComputerName)>>
        {
            { "Proxy1", new List<(string, string)> { ("00:11:22:33:44:55", "Computer1") } }
        };

                var config = new WakeOnLanConfiguration
                {
                    DefaultPort = 9,
                    MaxPingAttempts = 5,
                    DefaultTimeoutInSeconds = 30,
                    RunspacePoolMinThreads = 1,
                    RunspacePoolMaxThreads = 5
                };

                mock.Mock<IOptions<WakeOnLanConfiguration>>()
                    .Setup(x => x.Value)
                    .Returns(config);

                mock.Mock<IRequestScheduler>()
                    .Setup(s => s.Schedule(It.IsAny<Func<Task>>()))
                    .Verifiable();

                var wakeOnLanService = mock.Create<WakeOnLanService>();

                // Act
                await wakeOnLanService.WakeUpAndMonitorAsync(proxyToTargets);

                // Assert
                mock.Mock<IRequestScheduler>().Verify(s => s.Schedule(It.IsAny<Func<Task>>()), Times.Once);
            }
        }


        [Fact]
        public async Task WakeUpAndMonitorAsync_ShouldStartMonitoringTask()
        {
            using (var mock = AutoMock.GetLoose())
            {
                // Arrange
                var proxyToTargets = new Dictionary<string, List<(string MacAddress, string ComputerName)>>
                    {
                        { "Proxy1", new List<(string, string)> { ("00:11:22:33:44:55", "Computer1") } }
                    };

                var config = new WakeOnLanConfiguration
                {
                    DefaultPort = 9,
                    MaxPingAttempts = 5,
                    DefaultTimeoutInSeconds = 30,
                    RunspacePoolMinThreads = 1,
                    RunspacePoolMaxThreads = 5
                };

                mock.Mock<IOptions<WakeOnLanConfiguration>>()
                    .Setup(x => x.Value)
                    .Returns(config);

                mock.Mock<ITaskRunner>()
                    .Setup(r => r.Run(It.IsAny<Func<Task>>()))
                    .Verifiable();

                var wakeOnLanService = mock.Create<WakeOnLanService>();

                // Act
                await wakeOnLanService.WakeUpAndMonitorAsync(proxyToTargets);

                // Assert
                mock.Mock<ITaskRunner>().Verify(r => r.Run(It.IsAny<Func<Task>>()), Times.Once);
            }
        }

        [Fact]
        public async Task WakeUpAndMonitorAsync_ShouldReturnResultsFromResultManager()
        {
            using (var mock = AutoMock.GetLoose())
            {
                // Arrange
                var expectedResults = new List<WakeOnLanReturn>
                    {
                        new WakeOnLanReturn
                        {
                            TargetComputerName = "Computer1",
                            TargetMacAddress = "00:11:22:33:44:55",
                            ProxyComputerName = "Proxy1",
                            Port = 9,
                            Timestamp = DateTime.UtcNow,
                            RequestSent = true,
                            WolSuccess = true,
                            ErrorMessage = null
                        }
                    };

                // Arrange
                var config = new WakeOnLanConfiguration
                {
                    DefaultPort = 9,
                    MaxPingAttempts = 5,
                    DefaultTimeoutInSeconds = 30,
                    RunspacePoolMinThreads = 1,
                    RunspacePoolMaxThreads = 5
                };

                mock.Mock<IOptions<WakeOnLanConfiguration>>()
                    .Setup(x => x.Value)
                    .Returns(config);

                mock.Mock<IResultManager>()
                    .Setup(r => r.GetAllResults())
                    .Returns(expectedResults);

                var wakeOnLanService = mock.Create<WakeOnLanService>();

                // Act
                var results = await wakeOnLanService.WakeUpAndMonitorAsync(new Dictionary<string, List<(string, string)>>());

                // Assert
                Assert.Equal(expectedResults, results);
            }
        }

        [Fact]
        public async Task WakeUpAndMonitorAsync_ShouldUseConfigurationDefaults()
        {
            using (var mock = AutoMock.GetLoose())
            {
                // Arrange
                var proxyToTargets = new Dictionary<string, List<(string MacAddress, string ComputerName)>>
        {
            { "Proxy1", new List<(string, string)> { ("00:11:22:33:44:55", "Computer1") } }
        };

                var config = new WakeOnLanConfiguration
                {
                    DefaultPort = 7,
                    MaxPingAttempts = 8,
                    DefaultTimeoutInSeconds = 20,
                    RunspacePoolMinThreads = 2,
                    RunspacePoolMaxThreads = 10
                };

                mock.Mock<IOptions<WakeOnLanConfiguration>>()
                    .Setup(x => x.Value)
                    .Returns(config);

                mock.Mock<IRequestScheduler>()
                    .Setup(s => s.Schedule(It.IsAny<Func<Task>>()))
                    .Verifiable();

                var wakeOnLanService = mock.Create<WakeOnLanService>();

                // Act
                await wakeOnLanService.WakeUpAndMonitorAsync(proxyToTargets);

                // Assert
                mock.Mock<IRequestScheduler>().Verify(s => s.Schedule(It.IsAny<Func<Task>>()), Times.Once);
            }
        }


        [Fact]
        public void MonitoringCompleted_ShouldTriggerUpdateMonitoringResult()
        {
            using (var mock = AutoMock.GetLoose())
            {
                // Arrange
                var config = new WakeOnLanConfiguration
                {
                    DefaultPort = 9,
                    MaxPingAttempts = 5,
                    DefaultTimeoutInSeconds = 30,
                    RunspacePoolMinThreads = 1,
                    RunspacePoolMaxThreads = 5
                };

                mock.Mock<IOptions<WakeOnLanConfiguration>>()
                    .Setup(x => x.Value)
                    .Returns(config);

                var monitoringManager = mock.Mock<IMonitoringManager>();
                var resultManager = mock.Mock<IResultManager>();

                var wakeOnLanService = mock.Create<WakeOnLanService>();

                // Act
                monitoringManager.Raise(m => m.MonitoringCompleted += null, "Computer1", true, "Error");

                // Assert
                resultManager.Verify(r => r.UpdateMonitoringResult("Computer1", true, "Error"), Times.Once);
            }
        }


        [Fact]
        public void ResolveParameters_ShouldReturnResolvedParameters()
        {
            using (var mock = AutoMock.GetLoose())
            {
                // Arrange
                var config = new WakeOnLanConfiguration
                {
                    DefaultPort = 9,
                    MaxPingAttempts = 5,
                    DefaultTimeoutInSeconds = 30,
                    RunspacePoolMinThreads = 1,
                    RunspacePoolMaxThreads = 5
                };

                mock.Mock<IOptions<WakeOnLanConfiguration>>()
                    .Setup(x => x.Value)
                    .Returns(config);

                var wakeOnLanService = mock.Create<WakeOnLanService>();

                // Act
                //[assembly: InternalsVisibleTo("WakeOnLanLibrary.Tests")]
                var parameters = wakeOnLanService.ResolveParameters(null, null, null);

                // Assert
                Assert.Equal((9, 5, 30, 1, 5), parameters);
            }
        }

        [Fact]
        public void ResolveParameters_ShouldResolveDefaultValues_WhenInputsAreNull()
        {
            using (var mock = AutoMock.GetLoose())
            {
                // Arrange
                var config = new WakeOnLanConfiguration
                {
                    DefaultPort = 9,
                    MaxPingAttempts = 5,
                    DefaultTimeoutInSeconds = 30,
                    RunspacePoolMinThreads = 1,
                    RunspacePoolMaxThreads = 5
                };

                mock.Mock<IOptions<WakeOnLanConfiguration>>()
                    .Setup(x => x.Value)
                    .Returns(config);

                var wakeOnLanService = mock.Create<WakeOnLanService>();

                // Act
                var parameters = wakeOnLanService.ResolveParameters(null, null, null);

                // Assert
                Assert.Equal((9, 5, 30, 1, 5), parameters);
            }
        }


        [Fact]
        public void ResolveParameters_ShouldOverrideDefaultValues_WhenInputsAreProvided()
        {
            using (var mock = AutoMock.GetLoose())
            {
                // Arrange
                var config = new WakeOnLanConfiguration
                {
                    DefaultPort = 9,
                    MaxPingAttempts = 5,
                    DefaultTimeoutInSeconds = 30,
                    RunspacePoolMinThreads = 1,
                    RunspacePoolMaxThreads = 5
                };

                mock.Mock<IOptions<WakeOnLanConfiguration>>()
                    .Setup(x => x.Value)
                    .Returns(config);

                var wakeOnLanService = mock.Create<WakeOnLanService>();

                // Act
                var parameters = wakeOnLanService.ResolveParameters(7, 10, 15);

                // Assert
                Assert.Equal((7, 10, 15, 1, 5), parameters);
            }
        }

    }
}
