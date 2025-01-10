using Autofac.Extras.Moq;
using Moq;
using WakeOnLanLibrary.Application.Interfaces.Helpers;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Infrastructure.ValidationStrategies;

namespace WakeOnLanLibrary.Tests.Validators
{
    public class ProxyComputerValidationStrategyTests
    {
        [Fact]
        public void Validate_WhenProxyIsNull_ReturnsInvalidResult()
        {
            using var mock = AutoMock.GetLoose();
            var strategy = mock.Create<ProxyComputerValidationStrategy>();

            var result = strategy.Validate(null);

            Assert.False(result.IsValid);
            Assert.Equal("Proxy computer cannot be null.", result.Message);
        }

        [Fact]
        public void Validate_WhenProxyNameIsNullOrWhiteSpace_ReturnsInvalidResult()
        {
            using var mock = AutoMock.GetLoose();
            var strategy = mock.Create<ProxyComputerValidationStrategy>();

            var proxy = new ProxyComputer { Name = " " }; // Whitespace
            var result = strategy.Validate(proxy);

            Assert.False(result.IsValid);
            Assert.Equal("Proxy computer name cannot be null or empty.", result.Message);
        }

        [Fact]
        public void Validate_WhenProxyIsNotReachable_ReturnsInvalidResult()
        {
            using var mock = AutoMock.GetLoose();
            var networkHelperMock = mock.Mock<INetworkHelper>();
            networkHelperMock
                .Setup(x => x.IsComputerOnlineAsync(It.IsAny<string>(), 5000))
                .ReturnsAsync(false);

            var strategy = mock.Create<ProxyComputerValidationStrategy>();
            var proxy = new ProxyComputer { Name = "Proxy01" };

            var result = strategy.Validate(proxy);

            Assert.False(result.IsValid);
            Assert.Equal("Proxy computer is not reachable.", result.Message);

            // Verify the network helper was called
            networkHelperMock.Verify(
                x => x.IsComputerOnlineAsync(proxy.Name, 5000),
                Times.Once
            );
        }

        [Fact]
        public void Validate_WhenProxyIsReachableButWsmanUnavailable_ReturnsInvalidResult()
        {
            using var mock = AutoMock.GetLoose();
            var networkHelperMock = mock.Mock<INetworkHelper>();
            networkHelperMock
                .Setup(x => x.IsComputerOnlineAsync(It.IsAny<string>(), 5000))
                .ReturnsAsync(true); // Simulate the proxy is reachable
            networkHelperMock
                .Setup(x => x.IsWsmanAvailableAsync(It.IsAny<string>()))
                .ReturnsAsync(false); // Simulate WSMan is unavailable

            var strategy = mock.Create<ProxyComputerValidationStrategy>();
            var proxy = new ProxyComputer { Name = "Proxy01" };

            var result = strategy.Validate(proxy);

            Assert.False(result.IsValid);
            Assert.Equal("Proxy computer does not have WSMan available.", result.Message);

            // Verify the network helper was called
            networkHelperMock.Verify(
                x => x.IsComputerOnlineAsync(proxy.Name, 5000),
                Times.Once
            );
            networkHelperMock.Verify(
                x => x.IsWsmanAvailableAsync(proxy.Name),
                Times.Once
            );
        }

        [Fact]
        public void Validate_WhenProxyIsReachableAndWsmanAvailable_ReturnsValidResult()
        {
            using var mock = AutoMock.GetLoose();
            var networkHelperMock = mock.Mock<INetworkHelper>();
            networkHelperMock
                .Setup(x => x.IsComputerOnlineAsync(It.IsAny<string>(), 5000))
                .ReturnsAsync(true); // Simulate the proxy is reachable
            networkHelperMock
                .Setup(x => x.IsWsmanAvailableAsync(It.IsAny<string>()))
                .ReturnsAsync(true); // Simulate WSMan is available

            var strategy = mock.Create<ProxyComputerValidationStrategy>();
            var proxy = new ProxyComputer { Name = "Proxy01" };

            var result = strategy.Validate(proxy);

            Assert.True(result.IsValid);
            Assert.Equal("Proxy computer validation passed.", result.Message);

            // Verify the network helper was called
            networkHelperMock.Verify(
                x => x.IsComputerOnlineAsync(proxy.Name, 5000),
                Times.Once
            );
            networkHelperMock.Verify(
                x => x.IsWsmanAvailableAsync(proxy.Name),
                Times.Once
            );
        }

        [Fact]
        public void Validate_WhenNetworkHelperThrowsException_ReturnsInvalidResult()
        {
            using var mock = AutoMock.GetLoose();
            var networkHelperMock = mock.Mock<INetworkHelper>();
            networkHelperMock
                .Setup(x => x.IsComputerOnlineAsync(It.IsAny<string>(), 5000))
                .ThrowsAsync(new Exception("Network error"));

            var strategy = mock.Create<ProxyComputerValidationStrategy>();
            var proxy = new ProxyComputer { Name = "Proxy01" };

            var result = strategy.Validate(proxy);

            Assert.False(result.IsValid);
            Assert.Contains("An error occurred while validating the proxy computer: Network error", result.Message);
            Assert.Contains("Network error", result.Message);

            // Verify the network helper was called
            networkHelperMock.Verify(
                x => x.IsComputerOnlineAsync(proxy.Name, 5000),
                Times.Once
            );
        }
    }
}
