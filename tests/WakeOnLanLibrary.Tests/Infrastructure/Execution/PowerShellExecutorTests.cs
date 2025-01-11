using System.Management.Automation;
using WakeOnLanLibrary.Infrastructure.Execution;

namespace WakeOnLanLibrary.Tests.Infrastructure.Execution
{
    public class PowerShellExecutorIntegrationTests
    {
        [Fact]
        public async Task InvokeAsync_ExecutesRealPowerShellScript()
        {
            // Arrange

            var Powershell = PowerShell.Create();
            var realPowerShell = new PowerShellWrapper(Powershell);
            var executor = new PowerShellExecutor(realPowerShell);

            executor.AddScript("Write-Output 'Hello, World'");

            // Act
            var results = await executor.InvokeAsync();

            // Assert
            Assert.Single(results);
            Assert.Equal("Hello, World", results.First().BaseObject);
            Assert.False(executor.HadErrors);
        }

        [Fact]
        public async Task InvokeAsync_CollectsErrorsFromRealPowerShell()
        {
            // Arrange
            var realPowerShell = new PowerShellWrapper(); // Ensure proper instantiation
            var executor = new PowerShellExecutor(realPowerShell);

            executor.AddScript("Write-Error 'This is a test error'"); // Should not throw NullReferenceException

            // Act
            var results = await executor.InvokeAsync();

            // Assert
            Assert.Empty(results);
            Assert.True(executor.HadErrors);
            var errors = executor.Errors.ToList();
            Assert.Single(errors);
            Assert.Equal("This is a test error", errors.First().Message);
        }

        [Fact]
        public void AddScript_ThrowsArgumentNullException_WhenScriptIsNull()
        {
            // Arrange
            var realPowerShell = new PowerShellWrapper();
            var executor = new PowerShellExecutor(realPowerShell);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => executor.AddScript(null));
        }
    }

}
