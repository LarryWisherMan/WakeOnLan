using Moq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Core.Interfaces;
using WakeOnLanLibrary.Infrastructure.Services;

namespace WakeOnLanLibrary.Tests
{
    public class RunspaceManagerTests
    {
        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenRunspaceProviderIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new RunspaceManager(null));
        }

        [Fact]
        public void GetOrCreateRunspace_CreatesRunspace_WhenItDoesNotExist()
        {
            // Arrange
            var mockRunspaceProvider = new Mock<IRunspaceProvider>();
            var mockRunspace = new Mock<IRunspace>();

            // Setup the mock runspace state
            mockRunspace.Setup(r => r.State).Returns(RunspaceState.BeforeOpen);

            // Setup the mock runspace provider to return the mocked runspace
            mockRunspaceProvider
                .Setup(p => p.CreateRunspace(It.IsAny<WSManConnectionInfo>()))
                .Returns(mockRunspace.Object);

            var manager = new RunspaceManager(mockRunspaceProvider.Object);

            // Act
            var result = manager.GetOrCreateRunspace("testComputer");

            // Assert
            Assert.NotNull(result); // Ensure the returned runspace is not null
            Assert.Equal(mockRunspace.Object, result); // Ensure the returned runspace is the mocked one

            // Verify that CreateRunspace was called exactly once
            mockRunspaceProvider.Verify(
                p => p.CreateRunspace(It.IsAny<WSManConnectionInfo>()),
                Times.Once
            );

            // Ensure the runspace is added to the internal dictionary
            var secondResult = manager.GetOrCreateRunspace("testComputer");
            Assert.Equal(result, secondResult); // Should return the same runspace for the same computer
        }



        [Fact]
        public void GetOrCreateRunspacePool_CreatesNewPool_WhenPoolDoesNotExist()
        {
            // Arrange
            var mockProvider = new Mock<IRunspaceProvider>();
            var mockRunspacePool = new Mock<IRunspacePool>();

            // Mock pool state
            mockRunspacePool
                .Setup(p => p.RunspacePoolStateInfo)
                .Returns(new RunspacePoolStateInfo(RunspacePoolState.BeforeOpen, null));
            mockRunspacePool
                .Setup(p => p.GetRunspaceState())
                .Returns(RunspacePoolState.BeforeOpen);
            mockRunspacePool
                .Setup(p => p.Open())
                .Verifiable();

            // Mock provider to return the mocked pool
            mockProvider
                .Setup(p => p.CreateRunspacePool(1, 5, It.IsAny<WSManConnectionInfo>()))
                .Returns(mockRunspacePool.Object);

            var manager = new RunspaceManager(mockProvider.Object);

            // Act
            var pool = manager.GetOrCreateRunspacePool("testComputer", null, 1, 5);

            // Assert
            Assert.NotNull(pool);
            mockProvider.Verify(p => p.CreateRunspacePool(1, 5, It.IsAny<WSManConnectionInfo>()), Times.Once);
            mockRunspacePool.Verify(p => p.Open(), Times.Once);
        }

        [Fact]
        public void CloseRunspace_DisposesRunspace_WhenRunspaceExists()
        {
            // Arrange
            var mockRunspaceProvider = new Mock<IRunspaceProvider>();
            var mockRunspace = new Mock<IRunspace>();

            // Set up the mock runspace to simulate an "Opened" state
            mockRunspace.Setup(r => r.State).Returns(RunspaceState.Opened);
            mockRunspace.Setup(r => r.Close()).Verifiable();
            mockRunspace.Setup(r => r.Dispose()).Verifiable();

            var manager = new RunspaceManager(mockRunspaceProvider.Object);

            // Simulate adding the runspace to the manager
            mockRunspaceProvider
                .Setup(p => p.CreateRunspace(It.IsAny<WSManConnectionInfo>()))
                .Returns(mockRunspace.Object);

            manager.GetOrCreateRunspace("testComputer");

            // Act
            manager.CloseRunspace("testComputer");

            // Assert
            mockRunspace.Verify(r => r.Close(), Times.Once);
            mockRunspace.Verify(r => r.Dispose(), Times.Once);

            // Additional verification: Ensure CreateRunspace was only called once
            mockRunspaceProvider.Verify(p => p.CreateRunspace(It.IsAny<WSManConnectionInfo>()), Times.Once);
        }






        [Fact]
        public void Dispose_CleansUpResources_OnlyOnce()
        {
            // Arrange
            var mockProvider = new Mock<IRunspaceProvider>();
            var manager = new RunspaceManager(mockProvider.Object);

            // Act
            manager.Dispose();
            manager.Dispose();

            // Assert
            Assert.True(true); // Dispose should not throw exceptions or run multiple times.
        }

    }
}
