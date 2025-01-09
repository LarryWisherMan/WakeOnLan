using Autofac;
using Autofac.Extras.Moq;
using Moq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using WakeOnLanLibrary.Application.Interfaces.Execution;
using WakeOnLanLibrary.Core.Interfaces;
using WakeOnLanLibrary.Infrastructure.Execution;
using WakeOnLanLibrary.Infrastructure.Runspaces;

namespace WakeOnLanLibrary.Tests.Infrastructure.Execution
{
    public class RemotePowerShellExecutorTests
    {
        [Fact]
        public async Task ExecuteAsync_WhenRunspacePoolIsNull_ThrowsArgumentNullException()
        {
            using var mock = AutoMock.GetLoose();
            var executor = mock.Create<RemotePowerShellExecutor>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => executor.ExecuteAsync(null, "ValidScript"));
        }

        [Fact]
        public async Task ExecuteAsync_WhenScriptIsNullOrEmpty_ThrowsArgumentNullException()
        {
            using var mock = AutoMock.GetLoose();
            var executor = mock.Create<RemotePowerShellExecutor>();

            await Assert.ThrowsAsync<ArgumentNullException>(() => executor.ExecuteAsync(Mock.Of<IRunspacePool>(), null));
        }

        [Fact]
        public async Task ExecuteAsync_WhenRunspacePoolStateIsNotOpened_ThrowsInvalidOperationException()
        {
            using var mock = AutoMock.GetLoose();
            var runspacePoolMock = mock.Mock<IRunspacePool>();
            runspacePoolMock.Setup(rp => rp.RunspacePoolStateInfo).Returns(new RunspacePoolStateInfo(RunspacePoolState.Closed, null));

            var executor = mock.Create<RemotePowerShellExecutor>();

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                executor.ExecuteAsync(runspacePoolMock.Object, "ValidScript"));
        }


        [Fact]
        public async Task ExecuteAsync_WhenPowerShellHasErrors_ThrowsInvalidOperationException()
        {
            using var mock = AutoMock.GetLoose();

            // Mock IPowerShellExecutor
            var powerShellExecutorMock = mock.Mock<IPowerShellExecutor>();
            powerShellExecutorMock.Setup(ps => ps.InvokeAsync()).ReturnsAsync(new List<PSObject>());
            powerShellExecutorMock.Setup(ps => ps.HadErrors).Returns(true);
            powerShellExecutorMock.Setup(ps => ps.Errors).Returns(new List<ErrorRecord>
    {
        new ErrorRecord(new Exception("Error1"), "ErrorId1", ErrorCategory.InvalidOperation, null)
    });

            // Mock IRunspacePool
            var runspacePoolMock = mock.Mock<IRunspacePool>();
            runspacePoolMock.Setup(rp => rp.RunspacePoolStateInfo).Returns(new RunspacePoolStateInfo(RunspacePoolState.Opened, null));

            var executor = mock.Create<RemotePowerShellExecutor>();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                executor.ExecuteAsync(runspacePoolMock.Object, "ValidScript"));

            Assert.Contains("RunspacePool is not in an open state.", exception.Message);
        }


        [Fact]
        public async Task ExecuteAsync_WhenExceptionOccurs_ThrowsInvalidOperationException()
        {
            using var mock = AutoMock.GetLoose(config =>
            {
                // Register the Func<IPowerShellExecutor> factory
                config.RegisterInstance<Func<IPowerShellExecutor>>(() =>
                {
                    var powerShellExecutorMock = new Mock<IPowerShellExecutor>();
                    powerShellExecutorMock
                        .Setup(ps => ps.InvokeAsync())
                        .ThrowsAsync(new Exception("Test Exception"));
                    return powerShellExecutorMock.Object;
                });
            });

            // Initialize a real RunspacePool with proper configuration
            RunspacePool runspacePool = null;
            try
            {
                var initialSessionState = InitialSessionState.CreateDefault();
                runspacePool = RunspaceFactory.CreateRunspacePool(initialSessionState);
                runspacePool.Open(); // Ensure the pool is opened

                // Wrap the real RunspacePool in the RunspacePoolWrapper
                var runspacePoolWrapper = new RunspacePoolWrapper(runspacePool);

                // Create RemotePowerShellExecutor
                var executor = mock.Create<RemotePowerShellExecutor>();

                // Act & Assert
                var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                    executor.ExecuteAsync(runspacePoolWrapper, "ValidScript"));

                // Assert that the exception contains the expected message
                Assert.Contains("An error occurred while executing the PowerShell script", exception.Message);
            }
            finally
            {
                // Cleanup the RunspacePool
                runspacePool?.Close();
                runspacePool?.Dispose();
            }
        }



    }
}
