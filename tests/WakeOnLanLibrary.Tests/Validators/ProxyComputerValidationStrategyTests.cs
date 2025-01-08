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
        public void Validate_WhenProxyIsReachable_ReturnsValidResult()
        {
            using var mock = AutoMock.GetLoose();
            var networkHelperMock = mock.Mock<INetworkHelper>();
            networkHelperMock
                .Setup(x => x.IsComputerOnlineAsync(It.IsAny<string>(), 5000))
                .ReturnsAsync(true);

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
            Assert.Contains("An error occurred while checking if the proxy computer is reachable", result.Message);
            Assert.Contains("Network error", result.Message);

            // Verify the network helper was called
            networkHelperMock.Verify(
                x => x.IsComputerOnlineAsync(proxy.Name, 5000),
                Times.Once
            );
        }
    }
}
