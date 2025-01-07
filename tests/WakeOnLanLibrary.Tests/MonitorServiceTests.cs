using Autofac;
using Autofac.Extras.Moq;
using Moq;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Core.UseCases;
using WakeOnLanLibrary.Infrastructure.Monitoring;
namespace WakeOnLanLibrary.Tests
{



    public class MonitorServiceTests
    {
        [Fact]
        public void Constructor_NullMonitorCache_ThrowsArgumentNullException()
        {
            using (var mock = AutoMock.GetLoose())
            {
                Assert.Throws<ArgumentNullException>(() => new MonitorService(null, mock.Mock<IMonitorTask>().Object));
            }
        }

        [Fact]
        public void Constructor_NullMonitorTask_ThrowsArgumentNullException()
        {
            using (var mock = AutoMock.GetLoose())
            {
                Assert.Throws<ArgumentNullException>(() => new MonitorService(mock.Mock<IMonitorCache>().Object, null));
            }
        }

        [Fact]
        public async Task StartMonitoringAsync_EmptyCache_NoTasksExecuted()
        {
            using (var mock = AutoMock.GetLoose(builder =>
            {
                builder.RegisterInstance((Func<Task>)(() => Task.CompletedTask)).As<Func<Task>>();
            }))
            {
                // Arrange
                var mockCache = mock.Mock<IMonitorCache>();
                mockCache.Setup(c => c.GetAll()).Returns(new List<MonitorEntry>());

                var service = mock.Create<MonitorService>();

                // Act
                await service.StartMonitoringAsync();

                // Assert
                mock.Mock<IMonitorTask>().Verify(t => t.ExecuteAsync(It.IsAny<MonitorEntry>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [Fact]
        public async Task StartMonitoringAsync_MultipleEntries_TasksExecuted()
        {
            using (var mock = AutoMock.GetLoose(builder =>
            {
                // Register _delayAction as a no-op Task
                builder.RegisterInstance((Func<Task>)(() => Task.CompletedTask)).As<Func<Task>>();
            }))
            {
                // Arrange
                var mockCache = mock.Mock<IMonitorCache>();
                var entries = new List<MonitorEntry>
        {
            new MonitorEntry { ComputerName = "PC1", IsMonitoringComplete = false },
            new MonitorEntry { ComputerName = "PC2", IsMonitoringComplete = false }
        };
                mockCache.Setup(c => c.GetAll()).Returns(entries);

                var mockTask = mock.Mock<IMonitorTask>();
                mockTask.Setup(t => t.ExecuteAsync(
                        It.IsAny<MonitorEntry>(),
                        5,
                        60,
                        It.IsAny<CancellationToken>())
                    ).ReturnsAsync(true);

                var service = mock.Create<MonitorService>();

                // Act
                await service.StartMonitoringAsync();

                // Assert
                mockTask.Verify(t => t.ExecuteAsync(
                    It.IsAny<MonitorEntry>(),
                    5,
                    60,
                    It.IsAny<CancellationToken>()),
                    Times.Exactly(2));
            }
        }

        [Fact]
        public async Task StartMonitoringAsync_CompletedEntries_NoTasksExecuted()
        {
            using (var mock = AutoMock.GetLoose(builder =>
            {
                builder.RegisterInstance((Func<Task>)(() => Task.CompletedTask)).As<Func<Task>>();
            }))
            {
                // Arrange
                var mockCache = mock.Mock<IMonitorCache>();
                var entries = new List<MonitorEntry>
                {
                    new MonitorEntry { ComputerName = "PC1", IsMonitoringComplete = true },
                    new MonitorEntry { ComputerName = "PC2", IsMonitoringComplete = true }
                };
                mockCache.Setup(c => c.GetAll()).Returns(entries);

                var service = mock.Create<MonitorService>();

                // Act
                await service.StartMonitoringAsync();

                // Assert
                mock.Mock<IMonitorTask>().Verify(t => t.ExecuteAsync(It.IsAny<MonitorEntry>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        [Fact]
        public async Task StartMonitoringAsync_TriggersMonitoringCompletedEvent()
        {
            using (var mock = AutoMock.GetLoose(builder =>
            {
                builder.RegisterInstance((Func<Task>)(() => Task.CompletedTask)).As<Func<Task>>();
            }))
            {
                // Arrange
                var mockCache = mock.Mock<IMonitorCache>();
                var entries = new List<MonitorEntry>
                {
                    new MonitorEntry { ComputerName = "PC1", IsMonitoringComplete = false }
                };
                mockCache.Setup(c => c.GetAll()).Returns(entries);

                var mockTask = mock.Mock<IMonitorTask>();
                mockTask.Setup(t => t.ExecuteAsync(
                        It.IsAny<MonitorEntry>(),
                        5,
                        60,
                        It.IsAny<CancellationToken>())
                    ).ReturnsAsync(true);

                var service = mock.Create<MonitorService>();

                string receivedComputerName = null;
                bool receivedIsAwake = false;
                string receivedErrorMessage = null;

                service.MonitoringCompleted += (computerName, isAwake, errorMessage) =>
                {
                    receivedComputerName = computerName;
                    receivedIsAwake = isAwake;
                    receivedErrorMessage = errorMessage;
                };

                // Act
                await service.StartMonitoringAsync();

                // Assert
                Assert.Equal("PC1", receivedComputerName);
                Assert.True(receivedIsAwake);
                Assert.Null(receivedErrorMessage);
            }
        }

        [Fact]
        public async Task StartMonitoringAsync_RespectsThrottleLimits()
        {
            using (var mock = AutoMock.GetLoose(builder =>
            {
                builder.RegisterInstance((Func<Task>)(() => Task.CompletedTask)).As<Func<Task>>();
            }))
            {
                // Arrange
                var mockCache = mock.Mock<IMonitorCache>();
                var entries = Enumerable.Range(1, 10).Select(i => new MonitorEntry { ComputerName = $"PC{i}", IsMonitoringComplete = false }).ToList();
                mockCache.Setup(c => c.GetAll()).Returns(entries);

                var mockTask = mock.Mock<IMonitorTask>();
                var semaphore = new SemaphoreSlim(2);
                mockTask.Setup(t => t.ExecuteAsync(It.IsAny<MonitorEntry>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                    .Returns(async () =>
                    {
                        await semaphore.WaitAsync(); // Simulate throttling
                        semaphore.Release();
                        return true;
                    });

                var service = new MonitorService(mock.Mock<IMonitorCache>().Object, mock.Mock<IMonitorTask>().Object, () => Task.CompletedTask, () => DateTime.UtcNow, 2, 10);

                // Act
                await service.StartMonitoringAsync();

                // Assert
                Assert.True(semaphore.CurrentCount == 2); // Verify throttle limit was respected
            }
        }


        [Fact]
        public async Task MonitorEntryAsync_UpdatesCacheAndReleasesThrottle()
        {
            using (var mock = AutoMock.GetLoose())
            {
                // Arrange
                var entry = new MonitorEntry { ComputerName = "PC1", IsMonitoringComplete = false };
                var mockTask = mock.Mock<IMonitorTask>();
                mockTask.Setup(t => t.ExecuteAsync(entry, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

                var mockCache = mock.Mock<IMonitorCache>();
                mockCache.Setup(c => c.AddOrUpdate(It.IsAny<string>(), It.IsAny<Func<MonitorEntry>>(), It.IsAny<Action<MonitorEntry>>()));

                var service = mock.Create<MonitorService>();

                // Use reflection to access the private MonitorEntryAsync method
                var method = typeof(MonitorService).GetMethod("MonitorEntryAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                // Act
                await (Task)method.Invoke(service, new object[] { entry, 5, 60, CancellationToken.None });

                // Assert
                mockCache.Verify(c => c.AddOrUpdate(entry.ComputerName, It.IsAny<Func<MonitorEntry>>(), It.IsAny<Action<MonitorEntry>>()), Times.Once);
                mockTask.Verify(t => t.ExecuteAsync(entry, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        [Fact]
        public async Task MonitorEntryAsync_TaskThrowsException_HandlesGracefully()
        {
            using (var mock = AutoMock.GetLoose())
            {
                // Arrange
                var entry = new MonitorEntry { ComputerName = "PC1", IsMonitoringComplete = false };
                var mockTask = mock.Mock<IMonitorTask>();
                mockTask.Setup(t => t.ExecuteAsync(entry, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new InvalidOperationException("Test exception"));

                var mockCache = mock.Mock<IMonitorCache>();

                var service = mock.Create<MonitorService>();
                var method = typeof(MonitorService).GetMethod("MonitorEntryAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                // Act & Assert
                await Assert.ThrowsAsync<InvalidOperationException>(() => (Task)method.Invoke(service, new object[] { entry, 5, 60, CancellationToken.None }));
                mockCache.Verify(c => c.AddOrUpdate(It.IsAny<string>(), It.IsAny<Func<MonitorEntry>>(), It.IsAny<Action<MonitorEntry>>()), Times.Never);
            }
        }


        [Fact]
        public void MonitoringCompleted_EventTriggered_CorrectArguments()
        {
            using (var mock = AutoMock.GetLoose())
            {
                // Arrange
                var entry = new MonitorEntry { ComputerName = "PC1", IsMonitoringComplete = true };
                var service = mock.Create<MonitorService>();

                string receivedComputerName = null;
                bool receivedIsAwake = false;
                string receivedErrorMessage = null;

                service.MonitoringCompleted += (computerName, isAwake, errorMessage) =>
                {
                    receivedComputerName = computerName;
                    receivedIsAwake = isAwake;
                    receivedErrorMessage = errorMessage;
                };

                // Use reflection to invoke the private InvokeMonitoringCompleted method
                var method = typeof(MonitorService).GetMethod("InvokeMonitoringCompleted", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                // Act
                method.Invoke(service, new object[] { entry, true });

                // Assert
                Assert.Equal("PC1", receivedComputerName);
                Assert.True(receivedIsAwake);
                Assert.Null(receivedErrorMessage);
            }
        }

        [Fact]
        public async Task StartMonitoringAsync_CancellationRequested_StopsExecution()
        {
            using (var mock = AutoMock.GetLoose())
            {
                // Arrange
                var mockCache = mock.Mock<IMonitorCache>();
                mockCache.Setup(c => c.GetAll()).Returns(new List<MonitorEntry>
            {
                new MonitorEntry { ComputerName = "PC1", IsMonitoringComplete = false }
            });

                var mockTask = mock.Mock<IMonitorTask>();
                mockTask.Setup(t => t.ExecuteAsync(It.IsAny<MonitorEntry>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

                var cancellationTokenSource = new CancellationTokenSource();
                var service = mock.Create<MonitorService>();

                // Act
                cancellationTokenSource.Cancel(); // Cancel immediately
                await service.StartMonitoringAsync(cancellationToken: cancellationTokenSource.Token);

                // Assert
                mockTask.Verify(t => t.ExecuteAsync(It.IsAny<MonitorEntry>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
            }
        }

        private AutoMock CreateMockContext()
        {
            return AutoMock.GetLoose(builder =>
            {
                builder.RegisterInstance((Func<Task>)(() => Task.CompletedTask)).As<Func<Task>>();
                builder.RegisterInstance((Func<DateTime>)(() => DateTime.UtcNow)).As<Func<DateTime>>();
            });
        }
    }

}