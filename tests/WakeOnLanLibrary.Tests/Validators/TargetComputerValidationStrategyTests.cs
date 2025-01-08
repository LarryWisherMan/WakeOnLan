using Autofac.Extras.Moq;
using Moq;
using WakeOnLanLibrary.Application.Interfaces.Helpers;
using WakeOnLanLibrary.Application.Interfaces.Validation;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Infrastructure.ValidationStrategies;

namespace WakeOnLanLibrary.Tests.Validators
{
    public class TargetComputerValidationStrategyTests
    {
        [Fact]
        public void Validate_WhenTargetComputerNameIsNull_ReturnsInvalidResult()
        {
            using var mock = AutoMock.GetLoose();
            var strategy = mock.Create<TargetComputerValidationStrategy>();

            var target = new TargetComputer { Name = null, MacAddress = "00-14-22-01-23-45" };
            var result = strategy.Validate(target);

            Assert.False(result.IsValid);
            Assert.Equal("Target computer name cannot be null or empty.", result.Message);
        }

        [Fact]
        public void Validate_WhenMacAddressIsInvalid_ReturnsInvalidResult()
        {
            using var mock = AutoMock.GetLoose();
            var macAddressHelperMock = mock.Mock<IMacAddressHelper>();
            macAddressHelperMock.Setup(x => x.IsValidMacAddress(It.IsAny<string>())).Returns(false);

            var strategy = mock.Create<TargetComputerValidationStrategy>();
            var target = new TargetComputer { Name = "Target01", MacAddress = "InvalidMac" };

            var result = strategy.Validate(target);

            Assert.False(result.IsValid);
            Assert.Equal("Invalid MAC address format.", result.Message);
        }

        [Fact]
        public void Validate_WhenTargetComputerIsOnline_ReturnsInvalidResult()
        {
            using var mock = AutoMock.GetLoose();
            var networkHelperMock = mock.Mock<INetworkHelper>();

            var macAddressHelperMock = mock.Mock<IMacAddressHelper>();
            macAddressHelperMock.Setup(x => x.IsValidMacAddress(It.IsAny<string>())).Returns(true);

            networkHelperMock.Setup(x => x.IsComputerOnlineAsync(It.IsAny<string>(), 5000))
                             .ReturnsAsync(true);

            var strategy = mock.Create<TargetComputerValidationStrategy>();
            var target = new TargetComputer { Name = "Target01", MacAddress = "00:14:22:01:23:45" };

            var result = strategy.Validate(target);

            Assert.False(result.IsValid);
            Assert.Equal("Target computer is already online.", result.Message);
        }

        [Fact]
        public void Validate_WhenAllChecksPass_ReturnsValidResult()
        {
            using var mock = AutoMock.GetLoose();
            var macAddressHelperMock = mock.Mock<IMacAddressHelper>();
            macAddressHelperMock.Setup(x => x.IsValidMacAddress(It.IsAny<string>())).Returns(true);

            var networkHelperMock = mock.Mock<INetworkHelper>();
            networkHelperMock.Setup(x => x.IsComputerOnlineAsync(It.IsAny<string>(), 1000))
                             .ReturnsAsync(false);

            var strategy = mock.Create<TargetComputerValidationStrategy>();
            var target = new TargetComputer { Name = "Target01", MacAddress = "00-14-22-01-23-45" };

            var result = strategy.Validate(target);

            Assert.True(result.IsValid);
            Assert.Equal("Target computer validation passed.", result.Message);
        }
    }
}
