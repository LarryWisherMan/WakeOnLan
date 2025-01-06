using Autofac.Extras.Moq;
using Moq;
using System.Management.Automation;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Application.Interfaces.Validation;
using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Application.Services;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Core.Interfaces;

namespace WakeOnLanLibrary.Tests
{
    public class ProxyRequestProcessorTests
    {
        [Fact]
        public async Task ProcessProxyRequestsAsync_ShouldNotProceed_WhenRunspacePoolIsNull()
        {
            using var mock = AutoMock.GetLoose();
            var processor = mock.Create<ProxyRequestProcessor>();

            // Arrange
            var proxyComputerName = "ProxyComputer";
            var targets = new List<(string MacAddress, string ComputerName)>
            {
                ("00-14-22-01-23-45", "TargetComputer")
            };
            var port = 5985;
            var credential = new PSCredential("user", new System.Security.SecureString());
            var minRunspaces = 1;
            var maxRunspaces = 5;

            mock.Mock<IRunspaceManager>()
                .Setup(r => r.GetOrCreateRunspacePool(proxyComputerName, credential, minRunspaces, maxRunspaces))
                .Returns((IRunspacePool)null);


            mock.Mock<IComputerFactory>()
               .Setup(f => f.CreateProxyComputer(proxyComputerName, port))
               .Returns(new ProxyComputer());

            mock.Mock<IComputerValidator>()
                .Setup(v => v.Validate(It.IsAny<ProxyComputer>()))
                .Returns(new ValidationResult { IsValid = true });

            // Act
            await processor.ProcessProxyRequestsAsync(proxyComputerName, targets, port, credential, minRunspaces, maxRunspaces, 3, 30);

            // Assert
            mock.Mock<IMagicPacketSender>()
                .Verify(sender => sender.SendPacketAsync(It.IsAny<List<WakeOnLanRequest>>()), Times.Never);
        }

        [Fact]
        public async Task ProcessProxyRequestsAsync_ShouldSendPackets_WhenValidTargetsExist()
        {
            using var mock = AutoMock.GetLoose();
            var processor = mock.Create<ProxyRequestProcessor>();

            // Arrange
            var proxyComputerName = "ProxyComputer";
            var targets = new List<(string MacAddress, string ComputerName)>
            {
                ("00-14-22-01-23-45", "TargetComputer")
            };

            var port = 5985;
            var credential = new PSCredential("user", new System.Security.SecureString());
            var minRunspaces = 1;
            var maxRunspaces = 5;
            var runspacePool = Mock.Of<IRunspacePool>();

            // Ensure `GetOrCreateRunspacePool` returns a valid mock runspace pool
            mock.Mock<IRunspaceManager>()
                .Setup(r => r.GetOrCreateRunspacePool(proxyComputerName, credential, minRunspaces, maxRunspaces))
                .Returns(runspacePool);

            // Mock `CreateProxyComputer` to return a valid ProxyComputer instance
            mock.Mock<IComputerFactory>()
                .Setup(f => f.CreateProxyComputer(proxyComputerName, port))
                .Returns(new ProxyComputer());

            // Mock `Validate` for the proxy computer to return a valid result
            mock.Mock<IComputerValidator>()
                .Setup(v => v.Validate(It.IsAny<ProxyComputer>()))
                .Returns(new ValidationResult { IsValid = true });

            // Mock `CreateTargetComputer` to return a valid TargetComputer instance
            mock.Mock<IComputerFactory>()
                .Setup(f => f.CreateTargetComputer("TargetComputer", "00-14-22-01-23-45"))
                .Returns(new TargetComputer());

            // Mock `Validate` for the target computer to return a valid result
            mock.Mock<IComputerValidator>()
                .Setup(v => v.Validate(It.IsAny<TargetComputer>()))
                .Returns(new ValidationResult { IsValid = true });

            // Act
            await processor.ProcessProxyRequestsAsync(proxyComputerName, targets, port, credential, minRunspaces, maxRunspaces, 3, 30);

            // Assert
            mock.Mock<IMagicPacketSender>()
                .Verify(sender => sender.SendPacketAsync(It.Is<List<WakeOnLanRequest>>(r => r.Count == 1)), Times.Once);
        }

        [Fact]
        public async Task ProcessProxyRequestsAsync_ShouldNotProceed_WhenProxyComputerValidationFails()
        {
            using var mock = AutoMock.GetLoose();
            var processor = mock.Create<ProxyRequestProcessor>();

            // Arrange
            var proxyComputerName = "ProxyComputer";
            var targets = new List<(string MacAddress, string ComputerName)>
            {
                ("00-14-22-01-23-45", "TargetComputer")
            };

            var port = 5985;
            var credential = new PSCredential("user", new System.Security.SecureString());
            var minRunspaces = 1;
            var maxRunspaces = 5;

            // Mock `CreateProxyComputer` to return a valid proxy computer
            mock.Mock<IComputerFactory>()
                .Setup(f => f.CreateProxyComputer(proxyComputerName, port))
                .Returns(new ProxyComputer());

            // Mock `Validate` for the proxy computer to fail validation
            mock.Mock<IComputerValidator>()
                .Setup(v => v.Validate(It.IsAny<ProxyComputer>()))
                .Returns(new ValidationResult { IsValid = false, Message = "Validation failed" });

            // Act
            await processor.ProcessProxyRequestsAsync(proxyComputerName, targets, port, credential, minRunspaces, maxRunspaces, 3, 30);

            // Assert
            mock.Mock<IMagicPacketSender>()
                .Verify(sender => sender.SendPacketAsync(It.IsAny<List<WakeOnLanRequest>>()), Times.Never);
            mock.Mock<IResultManager>()
                .Verify(r => r.AddFailureResults(
                    proxyComputerName,
                    targets,
                    port,
                    "Validation failed"),
                    Times.Once);
        }

        [Fact]
        public async Task ProcessProxyRequestsAsync_ShouldDoNothing_WhenNoTargetsProvided()
        {
            using var mock = AutoMock.GetLoose();
            var processor = mock.Create<ProxyRequestProcessor>();

            // Arrange
            var proxyComputerName = "ProxyComputer";
            var targets = new List<(string MacAddress, string ComputerName)>(); // Empty list
            var port = 5985;
            var credential = new PSCredential("user", new System.Security.SecureString());
            var minRunspaces = 1;
            var maxRunspaces = 5;

            // Mock `GetOrCreateRunspacePool` to return a valid runspace pool
            var runspacePool = Mock.Of<IRunspacePool>();
            mock.Mock<IRunspaceManager>()
                .Setup(r => r.GetOrCreateRunspacePool(proxyComputerName, credential, minRunspaces, maxRunspaces))
                .Returns(runspacePool);

            // Act
            await processor.ProcessProxyRequestsAsync(proxyComputerName, targets, port, credential, minRunspaces, maxRunspaces, 3, 30);

            // Assert
            mock.Mock<IMagicPacketSender>()
                .Verify(sender => sender.SendPacketAsync(It.IsAny<List<WakeOnLanRequest>>()), Times.Never);
            mock.Mock<IResultManager>()
                .Verify(r => r.AddFailureResults(It.IsAny<string>(), It.IsAny<List<(string, string)>>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ProcessProxyRequestsAsync_ShouldLogFailures_WhenPacketSenderThrowsException()
        {
            using var mock = AutoMock.GetLoose();
            var processor = mock.Create<ProxyRequestProcessor>();

            // Arrange
            var proxyComputerName = "ProxyComputer";
            var targets = new List<(string MacAddress, string ComputerName)>
            {
                ("00-14-22-01-23-45", "TargetComputer")
            };


            var port = 5985;
            var credential = new PSCredential("user", new System.Security.SecureString());
            var minRunspaces = 1;
            var maxRunspaces = 5;

            // Mock the runspace pool
            var runspacePool = Mock.Of<IRunspacePool>();
            mock.Mock<IRunspaceManager>()
                .Setup(r => r.GetOrCreateRunspacePool(proxyComputerName, credential, minRunspaces, maxRunspaces))
                .Returns(runspacePool);

            // Mock `CreateProxyComputer` to return a valid proxy computer
            mock.Mock<IComputerFactory>()
                .Setup(f => f.CreateProxyComputer(proxyComputerName, port))
                .Returns(new ProxyComputer());

            // Mock `Validate` for the proxy computer to return a valid result
            mock.Mock<IComputerValidator>()
                .Setup(v => v.Validate(It.IsAny<ProxyComputer>()))
                .Returns(new ValidationResult { IsValid = true });

            // Mock `CreateTargetComputer` to return a valid target computer
            mock.Mock<IComputerFactory>()
                .Setup(f => f.CreateTargetComputer("TargetComputer", "00-14-22-01-23-45"))
                .Returns(new TargetComputer());

            // Mock `Validate` for the target computer to return a valid result
            mock.Mock<IComputerValidator>()
                .Setup(v => v.Validate(It.IsAny<TargetComputer>()))
                .Returns(new ValidationResult { IsValid = true });

            // Mock `_packetSender.SendPacketAsync` to throw an exception
            mock.Mock<IMagicPacketSender>()
                .Setup(sender => sender.SendPacketAsync(It.IsAny<List<WakeOnLanRequest>>()))
                .ThrowsAsync(new InvalidOperationException("Packet sending failed"));

            // Mock `_resultManager.AddFailureResults` to ensure it's called
            mock.Mock<IResultManager>()
                .Setup(r => r.AddFailureResults(
                    It.IsAny<string>(),
                    It.IsAny<List<(string, string)>>(),
                    It.IsAny<int>(),
                    It.IsAny<string>()));

            // Act
            await processor.ProcessProxyRequestsAsync(proxyComputerName, targets, port, credential, minRunspaces, maxRunspaces, 3, 30);

            // Assert
            // Assert
            mock.Mock<IResultManager>()
                .Verify(r => r.AddFailureResults(
                    proxyComputerName,
                    It.Is<List<(string, string)>>(list => list.Any(t => t.Item2 == "TargetComputer")),
                    port,
                    It.Is<string>(message => message.Contains("Packet sending failed"))), // Match the message
                    Times.Once);
        }

        [Fact]
        public async Task ProcessProxyRequestsAsync_ShouldHandleNullCredentials()
        {
            using var mock = AutoMock.GetLoose();
            var processor = mock.Create<ProxyRequestProcessor>();

            // Arrange
            var proxyComputerName = "ProxyComputer";
            var targets = new List<(string MacAddress, string ComputerName)>
            {
                ("00-14-22-01-23-45", "TargetComputer")
            };

            var port = 5985;
            var credential = (PSCredential)null; // Null credentials
            var minRunspaces = 1;
            var maxRunspaces = 5;

            // Mock `IComputerValidator` to validate proxy and target computers
            mock.Mock<IComputerValidator>()
                .Setup(v => v.Validate(It.IsAny<ProxyComputer>()))
                .Returns(new ValidationResult { IsValid = true }); // Proxy is valid
            mock.Mock<IComputerValidator>()
                .Setup(v => v.Validate(It.IsAny<TargetComputer>()))
                .Returns(new ValidationResult { IsValid = true }); // Target is valid

            // Mock `GetOrCreateRunspacePool` to return a valid runspace pool
            var runspacePool = Mock.Of<IRunspacePool>();
            mock.Mock<IRunspaceManager>()
                .Setup(r => r.GetOrCreateRunspacePool(proxyComputerName, credential, minRunspaces, maxRunspaces))
                .Returns(runspacePool);

            // Mock `IComputerFactory` to create proxy and target computers
            mock.Mock<IComputerFactory>()
                .Setup(f => f.CreateProxyComputer(proxyComputerName, port))
                .Returns(new ProxyComputer());
            mock.Mock<IComputerFactory>()
                .Setup(f => f.CreateTargetComputer("TargetComputer", "00-14-22-01-23-45"))
                .Returns(new TargetComputer { Name = "TargetComputer", MacAddress = "00-14-22-01-23-45" });

            // Mock `IMagicPacketSender` to ensure packets are sent
            mock.Mock<IMagicPacketSender>()
                .Setup(sender => sender.SendPacketAsync(It.IsAny<List<WakeOnLanRequest>>()));

            // Act
            await processor.ProcessProxyRequestsAsync(proxyComputerName, targets, port, credential, minRunspaces, maxRunspaces, 3, 30);

            // Assert
            mock.Mock<IMagicPacketSender>()
                .Verify(sender => sender.SendPacketAsync(It.Is<List<WakeOnLanRequest>>(r => r.Count == 1)), Times.Once);
        }

        [Fact]
        public void PrepareWakeOnLanRequests_ShouldExcludeInvalidMacAddresses()
        {
            using var mock = AutoMock.GetLoose();
            var processor = mock.Create<ProxyRequestProcessor>();

            // Arrange
            var proxyComputerName = "ProxyComputer";
            var targets = new List<(string MacAddress, string ComputerName)>
                {
                    ("Invalid-MAC", "InvalidComputer"),
                    ("00-14-22-01-23-45", "ValidComputer")
                };
            var port = 5985;
            var timeoutInSeconds = 30;

            var runspacePool = Mock.Of<IRunspacePool>();
            mock.Mock<IComputerFactory>()
                .Setup(f => f.CreateTargetComputer("ValidComputer", "00-14-22-01-23-45"))
                .Returns(new TargetComputer { Name = "ValidComputer", MacAddress = "00-14-22-01-23-45" });

            mock.Mock<IComputerFactory>()
                .Setup(f => f.CreateTargetComputer("InvalidComputer", "Invalid-MAC"))
                .Returns(new TargetComputer { Name = "InvalidComputer", MacAddress = "Invalid-MAC" });

            mock.Mock<IComputerValidator>()
                .Setup(v => v.Validate(It.IsAny<TargetComputer>()))
                .Returns((TargetComputer tc) => new ValidationResult { IsValid = tc.MacAddress != "Invalid-MAC" });

            // Act
            var result = processor.PrepareWakeOnLanRequests(proxyComputerName, targets, port, runspacePool, timeoutInSeconds);

            // Assert
            Assert.Single(result); // Only the valid target should be included
            Assert.Equal("ValidComputer", result[0].TargetComputerName);
            Assert.Equal("00-14-22-01-23-45", result[0].TargetMacAddress);
        }

        [Fact]
        public async Task ProcessProxyRequestsAsync_ShouldDoNothing_WhenProxyComputerNameIsNullOrEmpty()
        {
            using var mock = AutoMock.GetLoose();
            var processor = mock.Create<ProxyRequestProcessor>();

            // Arrange
            var proxyComputerName = ""; // Empty or null
            var targets = new List<(string MacAddress, string ComputerName)>
            {
                ("00-14-22-01-23-45", "TargetComputer")
            };
            var port = 5985;
            var credential = new PSCredential("user", new System.Security.SecureString());
            var minRunspaces = 1;
            var maxRunspaces = 5;

            // Act
            await processor.ProcessProxyRequestsAsync(proxyComputerName, targets, port, credential, minRunspaces, maxRunspaces, 3, 30);

            // Assert
            mock.Mock<IMagicPacketSender>()
                .Verify(sender => sender.SendPacketAsync(It.IsAny<List<WakeOnLanRequest>>()), Times.Never);
            mock.Mock<IResultManager>()
                .Verify(r => r.AddFailureResults(It.IsAny<string>(), It.IsAny<List<(string, string)>>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }



        [Fact]
        public void PrepareWakeOnLanRequests_ShouldExcludeInvalidTargets()
        {
            using var mock = AutoMock.GetLoose();
            var processor = mock.Create<ProxyRequestProcessor>();

            // Arrange
            var proxyComputerName = "ProxyComputer";
            var targets = new List<(string MacAddress, string ComputerName)>
            {
                ("00-14-22-01-23-45", "ValidComputer"),
                ("00-00-00-00-00-00", "InvalidComputer")
            };

            var port = 5985;
            var timeoutInSeconds = 30;

            // Mock the runspace pool
            var runspacePool = Mock.Of<IRunspacePool>();
            mock.Mock<IRunspaceManager>()
                .Setup(r => r.GetOrCreateRunspacePool(proxyComputerName, It.IsAny<PSCredential>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(runspacePool);

            // Mock `IComputerFactory` to create non-null TargetComputer objects
            mock.Mock<IComputerFactory>()
                .Setup(f => f.CreateTargetComputer("ValidComputer", "00-14-22-01-23-45"))
                .Returns(new TargetComputer { Name = "ValidComputer", MacAddress = "00-14-22-01-23-45" });

            mock.Mock<IComputerFactory>()
                .Setup(f => f.CreateTargetComputer("InvalidComputer", "00-00-00-00-00-00"))
                .Returns(new TargetComputer { Name = "InvalidComputer", MacAddress = "00-00-00-00-00-00" });

            // Mock `IComputerValidator` to return valid/invalid results
            mock.Mock<IComputerValidator>()
                .Setup(v => v.Validate(It.Is<TargetComputer>(tc => tc.Name == "ValidComputer")))
                .Returns(new ValidationResult { IsValid = true });

            mock.Mock<IComputerValidator>()
                .Setup(v => v.Validate(It.Is<TargetComputer>(tc => tc.Name == "InvalidComputer")))
                .Returns(new ValidationResult { IsValid = false });

            // Mock `IResultManager` to handle failure results without throwing
            mock.Mock<IResultManager>()
                .Setup(r => r.AddFailureResults(
                    It.IsAny<string>(),
                    It.IsAny<List<(string, string)>>(),
                    It.IsAny<int>(),
                    It.IsAny<string>()));

            // Act
            var result = processor.PrepareWakeOnLanRequests(proxyComputerName, targets, port, runspacePool, timeoutInSeconds);

            // Assert
            Assert.Single(result); // Only the valid target should be included
            Assert.Equal("ValidComputer", result[0].TargetComputerName);
            Assert.Equal("00-14-22-01-23-45", result[0].TargetMacAddress);
            // Verify that AddFailureResults was called once for the invalid target
            mock.Mock<IResultManager>()
                .Verify(r => r.AddFailureResults(
                    proxyComputerName,
                    It.Is<List<(string, string)>>(list => list.Count == 1 && list[0].Item2 == "InvalidComputer"),
                    port,
                    It.IsAny<string>()),
                    Times.Once);
        }

        [Fact]
        public void PrepareWakeOnLanRequests_ShouldUpdateMonitorCache_ForValidTargets()
        {
            using var mock = AutoMock.GetLoose();
            var processor = mock.Create<ProxyRequestProcessor>();

            // Arrange
            var proxyComputerName = "ProxyComputer";
            var targets = new List<(string MacAddress, string ComputerName)>
            {
                ("00-14-22-01-23-45", "ValidComputer")
            };

            var port = 5985;
            var timeoutInSeconds = 30;

            var runspacePool = Mock.Of<IRunspacePool>();
            mock.Mock<IComputerFactory>()
                .Setup(f => f.CreateTargetComputer("ValidComputer", "00-14-22-01-23-45"))
                .Returns(new TargetComputer { Name = "ValidComputer", MacAddress = "00-14-22-01-23-45" });

            mock.Mock<IComputerValidator>()
                .Setup(v => v.Validate(It.IsAny<TargetComputer>()))
                .Returns(new ValidationResult { IsValid = true });

            mock.Mock<IMonitorCache>()
                .Setup(c => c.AddOrUpdate(It.IsAny<string>(), It.IsAny<MonitorEntry>()));

            // Act
            processor.PrepareWakeOnLanRequests(proxyComputerName, targets, port, runspacePool, timeoutInSeconds);

            // Assert
            mock.Mock<IMonitorCache>()
                .Verify(c => c.AddOrUpdate(
                    "ValidComputer",
                    It.Is<MonitorEntry>(entry =>
                        entry.ComputerName == "ValidComputer" &&
                        entry.ProxyComputerName == proxyComputerName &&
                        entry.WolSuccess == false &&
                        entry.IsMonitoringComplete == false)),
                    Times.Once);
        }

        [Fact]
        public void PrepareWakeOnLanRequests_ShouldContinue_WhenMonitorCacheThrowsException()
        {
            using var mock = AutoMock.GetLoose();
            var processor = mock.Create<ProxyRequestProcessor>();

            // Arrange
            var proxyComputerName = "ProxyComputer";
            var targets = new List<(string MacAddress, string ComputerName)>
            {
                ("00-14-22-01-23-45", "ValidComputer"),
                ("00-00-00-00-00-00", "AnotherValidComputer")
            };

            var port = 5985;
            var timeoutInSeconds = 30;

            var runspacePool = Mock.Of<IRunspacePool>();
            mock.Mock<IRunspaceManager>()
                .Setup(r => r.GetOrCreateRunspacePool(It.IsAny<string>(), It.IsAny<PSCredential>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(runspacePool);

            // Mock `IComputerFactory` to create valid target computers
            mock.Mock<IComputerFactory>()
                .Setup(f => f.CreateTargetComputer("ValidComputer", "00-14-22-01-23-45"))
                .Returns(new TargetComputer { Name = "ValidComputer", MacAddress = "00-14-22-01-23-45" });

            mock.Mock<IComputerFactory>()
                .Setup(f => f.CreateTargetComputer("AnotherValidComputer", "00-00-00-00-00-00"))
                .Returns(new TargetComputer { Name = "AnotherValidComputer", MacAddress = "00-00-00-00-00-00" });

            // Mock `IComputerValidator` to return valid results
            mock.Mock<IComputerValidator>()
                .Setup(v => v.Validate(It.IsAny<TargetComputer>()))
                .Returns(new ValidationResult { IsValid = true });

            // Mock `IMonitorCache` to throw an exception for one of the targets
            mock.Mock<IMonitorCache>()
                .Setup(c => c.AddOrUpdate("ValidComputer", It.IsAny<MonitorEntry>()))
                .Throws(new InvalidOperationException("Simulated cache failure"));

            // Mock `IMonitorCache` to succeed for the other target
            mock.Mock<IMonitorCache>()
                .Setup(c => c.AddOrUpdate("AnotherValidComputer", It.IsAny<MonitorEntry>()));

            // Mock `_resultManager` to handle failure results
            mock.Mock<IResultManager>()
                .Setup(r => r.AddFailureResults(
                    proxyComputerName,
                    It.IsAny<List<(string, string)>>(),
                    port,
                    It.IsAny<string>()));

            // Act
            var result = processor.PrepareWakeOnLanRequests(proxyComputerName, targets, port, runspacePool, timeoutInSeconds);

            // Assert
            Assert.Equal(2, result.Count); // Both targets should be processed
            Assert.Contains(result, r => r.TargetComputerName == "ValidComputer");
            Assert.Contains(result, r => r.TargetComputerName == "AnotherValidComputer");

            // Verify failure was logged for the exception
            mock.Mock<IResultManager>()
                .Verify(r => r.AddFailureResults(
                    proxyComputerName,
                    It.Is<List<(string, string)>>(list => list.Count == 1 && list[0].Item2 == "ValidComputer"),
                    port,
                    It.Is<string>(msg => msg.Contains("Simulated cache failure"))),
                    Times.Once);

            // Verify `AddOrUpdate` was called for both targets
            mock.Mock<IMonitorCache>()
                .Verify(c => c.AddOrUpdate("ValidComputer", It.IsAny<MonitorEntry>()), Times.Once);
            mock.Mock<IMonitorCache>()
                .Verify(c => c.AddOrUpdate("AnotherValidComputer", It.IsAny<MonitorEntry>()), Times.Once);
        }

    }
}
