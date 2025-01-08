using Autofac.Extras.Moq;
using Moq;
using WakeOnLanLibrary.Application.Validation;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Core.Interfaces.Validation;
using WakeOnLanLibrary.Core.Validation;

namespace WakeOnLanLibrary.Tests.Validators
{
    public class ComputerValidatorTests
    {
        [Fact]
        public void Constructor_WhenValidatorFactoryIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ComputerValidator(null));
        }

        [Fact]
        public void Validate_WhenComputerIsNull_ThrowsArgumentNullException()
        {
            using var mock = AutoMock.GetLoose();
            var factoryMock = mock.Mock<IValidatorFactory>();

            var computerValidator = new ComputerValidator(factoryMock.Object);

            var exception = Assert.Throws<ArgumentNullException>(() => computerValidator.Validate<Computer>(null));

            Assert.Equal("The computer object cannot be null. (Parameter 'computer')", exception.Message);
        }

        [Fact]
        public void Validate_WhenValidationSucceeds_ReturnsValidResult_TargetComputer()
        {
            using var mock = AutoMock.GetLoose();

            // Mock a validation strategy for `TargetComputer`
            var strategyMock = new Mock<IValidationStrategy<TargetComputer>>();
            strategyMock
                .Setup(s => s.Validate(It.IsAny<TargetComputer>()))
                .Returns(new ValidationResult { IsValid = true, Message = "Validation passed." });

            // Use the real CompositeValidator with the mocked strategy for `TargetComputer`
            var factoryMock = mock.Mock<IValidatorFactory>();
            factoryMock
                .Setup(f => f.GetValidator<TargetComputer>())
                .Returns(new CompositeValidator<TargetComputer>(new[] { strategyMock.Object }));

            var computerValidator = new ComputerValidator(factoryMock.Object);
            var targetComputer = new TargetComputer { Name = "ValidTargetComputer", MacAddress = "00:14:22:01:23:45" };

            // Act
            var result = computerValidator.Validate(targetComputer);

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal("All validations passed.", result.Message);

            // Verify
            factoryMock.Verify(f => f.GetValidator<TargetComputer>(), Times.Once);
            strategyMock.Verify(s => s.Validate(targetComputer), Times.Once);
        }


        [Fact]
        public void Validate_WhenValidationSucceeds_ReturnsValidResult_ProxyComputer()
        {
            using var mock = AutoMock.GetLoose();

            // Mock a validation strategy for `TargetComputer`
            var strategyMock = new Mock<IValidationStrategy<ProxyComputer>>();
            strategyMock
                .Setup(s => s.Validate(It.IsAny<ProxyComputer>()))
                .Returns(new ValidationResult { IsValid = true, Message = "Validation passed." });

            // Use the real CompositeValidator with the mocked strategy for `TargetComputer`
            var factoryMock = mock.Mock<IValidatorFactory>();
            factoryMock
                .Setup(f => f.GetValidator<ProxyComputer>())
                .Returns(new CompositeValidator<ProxyComputer>(new[] { strategyMock.Object }));

            var computerValidator = new ComputerValidator(factoryMock.Object);
            var targetComputer = new ProxyComputer { Name = "ValidTargetComputer" };

            // Act
            var result = computerValidator.Validate(targetComputer);

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal("All validations passed.", result.Message);

            // Verify
            factoryMock.Verify(f => f.GetValidator<ProxyComputer>(), Times.Once);
            strategyMock.Verify(s => s.Validate(targetComputer), Times.Once);
        }


        [Fact]
        public void Validate_WhenValidationFails_ReturnsInvalidResult_TargetComputer()
        {
            using var mock = AutoMock.GetLoose();

            // Mock a validation strategy for `TargetComputer`
            var strategyMock = new Mock<IValidationStrategy<TargetComputer>>();
            strategyMock
                .Setup(s => s.Validate(It.IsAny<TargetComputer>()))
                .Returns(new ValidationResult { IsValid = false, Message = "Validation failed." });

            // Use the real CompositeValidator with the mocked strategy for `TargetComputer`
            var factoryMock = mock.Mock<IValidatorFactory>();
            factoryMock
                .Setup(f => f.GetValidator<TargetComputer>())
                .Returns(new CompositeValidator<TargetComputer>(new[] { strategyMock.Object }));

            var computerValidator = new ComputerValidator(factoryMock.Object);
            var targetComputer = new TargetComputer { Name = "ValidTargetComputer", MacAddress = "00:14:22:01:23:45" };

            // Act
            var result = computerValidator.Validate(targetComputer);

            Assert.False(result.IsValid);
            Assert.Equal("Validation failed.", result.Message);

            factoryMock.Verify(f => f.GetValidator<Computer>(), Times.Once);
            strategyMock.Verify(s => s.Validate(targetComputer), Times.Once);
        }

        [Fact]
        public void Validate_WhenValidationFails_ReturnsInvalidResult_ProxyComputer()
        {
            using var mock = AutoMock.GetLoose();

            // Mock a validation strategy for `TargetComputer`
            var strategyMock = new Mock<IValidationStrategy<ProxyComputer>>();
            strategyMock
                .Setup(s => s.Validate(It.IsAny<ProxyComputer>()))
                .Returns(new ValidationResult { IsValid = false, Message = "Validation failed." });

            // Use the real CompositeValidator with the mocked strategy for `TargetComputer`
            var factoryMock = mock.Mock<IValidatorFactory>();
            factoryMock
                .Setup(f => f.GetValidator<ProxyComputer>())
                .Returns(new CompositeValidator<ProxyComputer>(new[] { strategyMock.Object }));

            var computerValidator = new ComputerValidator(factoryMock.Object);
            var targetComputer = new ProxyComputer { Name = "ValidTargetComputer" };

            // Act
            var result = computerValidator.Validate(targetComputer);

            Assert.False(result.IsValid);
            Assert.Equal("Validation failed.", result.Message);

            factoryMock.Verify(f => f.GetValidator<Computer>(), Times.Once);
            strategyMock.Verify(s => s.Validate(targetComputer), Times.Once);
        }

    }

}
