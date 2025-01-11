using Moq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using WakeOnLanLibrary.Application.Common;
using WakeOnLanLibrary.Application.Interfaces.Execution;
using WakeOnLanLibrary.Core.Interfaces;
using WakeOnLanLibrary.Infrastructure.Execution;

namespace WakeOnLanLibrary.Tests.Infrastructure.Execution
{

    public class RemotePowerShellExecutorTests
    {
        [Fact]
        public async Task ExecuteAsync_ThrowsArgumentNullException_WhenRunspacePoolIsNull()
        {
            // Arrange
            var mockPowerShellExecutor = new Mock<IPowerShellExecutor>();
            var powerShellExecutorFactory = new Mock<Func<IPowerShellExecutor>>();
            powerShellExecutorFactory.Setup(factory => factory()).Returns(mockPowerShellExecutor.Object);

            var executor = new RemotePowerShellExecutor(powerShellExecutorFactory.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                executor.ExecuteAsync(null, "Test-Script"));

            Assert.Equal("runspacePool", exception.ParamName);
        }


        [Fact]
        public async Task ExecuteAsync_ThrowsArgumentNullException_WhenScriptIsNull()
        {
            // Arrange
            var mockRunspacePool = new Mock<IRunspacePool>();
            var mockPowerShellExecutor = new Mock<IPowerShellExecutor>();
            var powerShellExecutorFactory = new Mock<Func<IPowerShellExecutor>>();
            powerShellExecutorFactory.Setup(factory => factory()).Returns(mockPowerShellExecutor.Object);

            var executor = new RemotePowerShellExecutor(powerShellExecutorFactory.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() =>
                executor.ExecuteAsync(mockRunspacePool.Object, null));

            Assert.Equal("script", exception.ParamName);
        }


        [Fact]
        public async Task ExecuteAsync_ThrowsInvalidOperationException_WhenRunspacePoolIsNotOpened()
        {
            // Arrange
            var mockRunspacePool = new Mock<IRunspacePool>();
            mockRunspacePool
                .Setup(r => r.GetRunspaceState())
                .Returns(RunspacePoolState.Closed);

            var mockPowerShellExecutor = new Mock<IPowerShellExecutor>();
            var powerShellExecutorFactory = new Mock<Func<IPowerShellExecutor>>();
            powerShellExecutorFactory.Setup(factory => factory()).Returns(mockPowerShellExecutor.Object);

            var executor = new RemotePowerShellExecutor(powerShellExecutorFactory.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                executor.ExecuteAsync(mockRunspacePool.Object, "Test-Script"));

            Assert.Equal("RunspacePool is not in an open state.", exception.Message);
        }

        [Fact]
        public async Task ExecuteAsync_ExecutesScriptSuccessfully_WhenInputsAreValid()
        {
            // Arrange

            // Mock IRunspacePool
            var mockRunspacePool = new Mock<IRunspacePool>();
            mockRunspacePool
                .Setup(r => r.GetRunspaceState())
                .Returns(RunspacePoolState.Opened);

            // Mock IPowerShellExecutor
            var mockPowerShellExecutor = new Mock<IPowerShellExecutor>();
            mockPowerShellExecutor
                .Setup(p => p.InvokeAsync())
                .ReturnsAsync(new List<PSObject>()); // Simulate successful execution

            // Mock the factory to return the mocked executor
            var powerShellExecutorFactory = new Mock<Func<IPowerShellExecutor>>();
            powerShellExecutorFactory
                .Setup(factory => factory())
                .Returns(mockPowerShellExecutor.Object);

            // Create the RemotePowerShellExecutor with mocked dependencies
            var executor = new RemotePowerShellExecutor(powerShellExecutorFactory.Object);

            // Act
            await executor.ExecuteAsync(mockRunspacePool.Object, "Test-Script");

            // Assert
            mockPowerShellExecutor.Verify(p => p.AddScript("Test-Script"), Times.Once);
            mockPowerShellExecutor.Verify(p => p.InvokeAsync(), Times.Once);
        }


        [Fact]
        public async Task ExecuteAsync_ThrowsInvalidOperationException_WhenErrorsOccurDuringExecution()
        {
            // Arrange
            var mockPowerShellExecutor = new Mock<IPowerShellExecutor>();

            // Mock InvokeAsync to return an empty result set
            mockPowerShellExecutor
                .Setup(p => p.InvokeAsync())
                .ReturnsAsync(new List<PSObject>());

            // Mock HadErrors to return true, indicating errors occurred
            mockPowerShellExecutor
                .SetupGet(p => p.HadErrors)
                .Returns(true);

            // Mock Errors to return a list of PowerShellError objects
            mockPowerShellExecutor
                .SetupGet(p => p.Errors)
                .Returns(new List<PowerShellError>
                {
            new PowerShellError("Test Error 1", "ErrorId1", "Target1"),
            new PowerShellError("Test Error 2", "ErrorId2", "Target2")
                });

            // Mock the PowerShellExecutor factory
            var powerShellExecutorFactory = new Mock<Func<IPowerShellExecutor>>();
            powerShellExecutorFactory
                .Setup(factory => factory())
                .Returns(mockPowerShellExecutor.Object);

            // Mock the RunspacePool to always return an Opened state
            var mockRunspacePool = new Mock<IRunspacePool>();
            mockRunspacePool
                .Setup(r => r.GetRunspaceState())
                .Returns(RunspacePoolState.Opened);

            var executor = new RemotePowerShellExecutor(powerShellExecutorFactory.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AggregateException>(() =>
                executor.ExecuteAsync(mockRunspacePool.Object, "Test-Script"));

            // Assert the exception message contains details about the errors
            Assert.Contains("Errors occurred during script execution.", exception.Message);

            // Assert that the exception contains the individual errors
            Assert.Equal(2, exception.InnerExceptions.Count);
            Assert.Contains(exception.InnerExceptions, ex => ex.Message.Contains("Test Error 1"));
            Assert.Contains(exception.InnerExceptions, ex => ex.Message.Contains("ErrorId1"));
            Assert.Contains(exception.InnerExceptions, ex => ex.Message.Contains("Target1"));

            Assert.Contains(exception.InnerExceptions, ex => ex.Message.Contains("Test Error 2"));
            Assert.Contains(exception.InnerExceptions, ex => ex.Message.Contains("ErrorId2"));
            Assert.Contains(exception.InnerExceptions, ex => ex.Message.Contains("Target2"));
        }



        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenFactoryIsNull()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new RemotePowerShellExecutor(null));
            Assert.Equal("powerShellExecutorFactory", exception.ParamName);
        }

        [Fact]
        public async Task ExecuteAsync_DoesNotThrow_WhenExecutionIsSuccessful()
        {
            // Arrange
            var mockRunspacePool = new Mock<IRunspacePool>();
            mockRunspacePool.Setup(r => r.GetRunspaceState()).Returns(RunspacePoolState.Opened);

            var mockPowerShellExecutor = new Mock<IPowerShellExecutor>();
            mockPowerShellExecutor.Setup(p => p.InvokeAsync()).ReturnsAsync(new List<PSObject>());

            var powerShellExecutorFactory = new Mock<Func<IPowerShellExecutor>>();
            powerShellExecutorFactory.Setup(factory => factory()).Returns(mockPowerShellExecutor.Object);

            var executor = new RemotePowerShellExecutor(powerShellExecutorFactory.Object);

            // Act & Assert
            await executor.ExecuteAsync(mockRunspacePool.Object, "Test-Script");
        }


        [Fact]
        public async Task ExecuteAsync_ExecutesMultipleScriptsSuccessfully()
        {
            // Arrange
            var mockRunspacePool = new Mock<IRunspacePool>();
            mockRunspacePool.Setup(r => r.GetRunspaceState()).Returns(RunspacePoolState.Opened);

            var mockPowerShellExecutor = new Mock<IPowerShellExecutor>();
            mockPowerShellExecutor.Setup(p => p.InvokeAsync()).ReturnsAsync(new List<PSObject>());

            var powerShellExecutorFactory = new Mock<Func<IPowerShellExecutor>>();
            powerShellExecutorFactory.Setup(factory => factory()).Returns(mockPowerShellExecutor.Object);

            var executor = new RemotePowerShellExecutor(powerShellExecutorFactory.Object);

            // Act
            await executor.ExecuteAsync(mockRunspacePool.Object, "Test-Script-1");
            await executor.ExecuteAsync(mockRunspacePool.Object, "Test-Script-2");

            // Assert
            mockPowerShellExecutor.Verify(p => p.AddScript("Test-Script-1"), Times.Once);
            mockPowerShellExecutor.Verify(p => p.AddScript("Test-Script-2"), Times.Once);
            mockPowerShellExecutor.Verify(p => p.InvokeAsync(), Times.Exactly(2));
        }

        [Fact]
        public async Task ExecuteAsync_ThrowsInvalidOperationException_WhenUnexpectedErrorOccurs()
        {
            // Arrange
            var mockRunspacePool = new Mock<IRunspacePool>();
            mockRunspacePool.Setup(r => r.GetRunspaceState()).Returns(RunspacePoolState.Opened);

            var mockPowerShellExecutor = new Mock<IPowerShellExecutor>();
            mockPowerShellExecutor
                .Setup(p => p.InvokeAsync())
                .ThrowsAsync(new Exception("Unexpected error"));

            var powerShellExecutorFactory = new Mock<Func<IPowerShellExecutor>>();
            powerShellExecutorFactory.Setup(factory => factory()).Returns(mockPowerShellExecutor.Object);

            var executor = new RemotePowerShellExecutor(powerShellExecutorFactory.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                executor.ExecuteAsync(mockRunspacePool.Object, "Test-Script"));

            Assert.Contains("An error occurred while executing the PowerShell script.", exception.Message);
            Assert.NotNull(exception.InnerException);
            Assert.Contains("Unexpected error", exception.InnerException.Message);
        }

        [Fact]
        public async Task ExecuteAsync_AggregatesErrorMessagesCorrectly_WhenErrorsOccur()
        {
            // Arrange
            var mockPowerShellExecutor = new Mock<IPowerShellExecutor>();
            mockPowerShellExecutor.Setup(p => p.InvokeAsync()).ReturnsAsync(new List<PSObject>());
            mockPowerShellExecutor.Setup(p => p.HadErrors).Returns(true);
            mockPowerShellExecutor.Setup(p => p.Errors).Returns(new List<PowerShellError>
            {
                new PowerShellError("Error 1 occurred", "ErrorId1", "Target1"),
                new PowerShellError("Error 2 occurred", "ErrorId2", "Target2")
            });

            var executorFactory = new Func<IPowerShellExecutor>(() => mockPowerShellExecutor.Object);
            var remoteExecutor = new RemotePowerShellExecutor(executorFactory);

            var runspacePool = Mock.Of<IRunspacePool>(r => r.GetRunspaceState() == RunspacePoolState.Opened);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<AggregateException>(() =>
                remoteExecutor.ExecuteAsync(runspacePool, "Test-Script"));

            // Verify
            Assert.Equal(2, exception.InnerExceptions.Count);
            Assert.Contains(exception.InnerExceptions, ex => ex.Message.Contains("Error 1 occurred"));
            Assert.Contains(exception.InnerExceptions, ex => ex.Message.Contains("ErrorId1"));
            Assert.Contains(exception.InnerExceptions, ex => ex.Message.Contains("Target1"));

            Assert.Contains(exception.InnerExceptions, ex => ex.Message.Contains("Error 2 occurred"));
            Assert.Contains(exception.InnerExceptions, ex => ex.Message.Contains("ErrorId2"));
            Assert.Contains(exception.InnerExceptions, ex => ex.Message.Contains("Target2"));
        }

    }

}
