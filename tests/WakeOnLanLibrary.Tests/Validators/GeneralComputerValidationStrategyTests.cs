using Autofac.Extras.Moq;
using Moq;
using WakeOnLanLibrary.Core.Interfaces.Validation;
using WakeOnLanLibrary.Core.ValidationStrategies;
using WakeOnLanLibrary.Tests.TestObjects;

namespace WakeOnLanLibrary.Tests.Validators
{
    public class GeneralComputerValidationStrategyTests
    {
        [Fact]
        public void Validate_WhenComputerIsNull_ReturnsInvalidResult()
        {
            using var mock = AutoMock.GetLoose();
            var strategy = mock.Create<GeneralComputerValidationStrategy>();

            var result = strategy.Validate(null);

            Assert.False(result.IsValid);
            Assert.Equal("Computer object cannot be null.", result.Message);
        }

        [Fact]
        public void Validate_WhenComputerNameIsEmpty_ReturnsInvalidResult()
        {
            using var mock = AutoMock.GetLoose();
            var strategy = mock.Create<GeneralComputerValidationStrategy>();

            var computer = new TestComputer { Name = "" };
            var result = strategy.Validate(computer);

            Assert.False(result.IsValid);
            Assert.Equal("Computer name must be provided.", result.Message);
        }

        [Fact]
        public void Validate_WhenComputerNameIsInvalid_ReturnsInvalidResult()
        {
            using var mock = AutoMock.GetLoose();
            var nameIpValidatorMock = mock.Mock<INameIpValidator>();
            nameIpValidatorMock.Setup(x => x.IsValidComputerName(It.IsAny<string>())).Returns(false);
            nameIpValidatorMock.Setup(x => x.IsValidIpAddress(It.IsAny<string>())).Returns(false);

            var strategy = mock.Create<GeneralComputerValidationStrategy>();
            var computer = new TestComputer { Name = "InvalidName" };

            var result = strategy.Validate(computer);

            Assert.False(result.IsValid);
            Assert.Equal("Invalid computer name or IP address: 'InvalidName'.", result.Message);
        }

        [Fact]
        public void Validate_WhenComputerNameIsValid_ReturnsValidResult()
        {
            using var mock = AutoMock.GetLoose();
            var nameIpValidatorMock = mock.Mock<INameIpValidator>();
            nameIpValidatorMock.Setup(x => x.IsValidComputerName(It.IsAny<string>())).Returns(true);

            var strategy = mock.Create<GeneralComputerValidationStrategy>();
            var computer = new TestComputer { Name = "ValidName" };

            var result = strategy.Validate(computer);

            Assert.True(result.IsValid);
            Assert.Equal("Validation passed.", result.Message);
        }
    }


}

