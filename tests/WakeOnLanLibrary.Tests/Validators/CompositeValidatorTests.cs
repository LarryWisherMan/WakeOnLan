using Autofac.Extras.Moq;
using Moq;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Core.Interfaces.Validation;
using WakeOnLanLibrary.Core.Validation;
using WakeOnLanLibrary.Tests.TestObjects;

namespace WakeOnLanLibrary.Tests.Validators
{
    public class CompositeValidatorTests
    {
        [Fact]
        public void Validate_WhenAllStrategiesPass_ReturnsValidResult()
        {
            using var mock = AutoMock.GetLoose();
            var strategyMock1 = new Mock<IValidationStrategy<Computer>>();
            strategyMock1.Setup(x => x.Validate(It.IsAny<Computer>())).Returns(new ValidationResult { IsValid = true });

            var strategyMock2 = new Mock<IValidationStrategy<Computer>>();
            strategyMock2.Setup(x => x.Validate(It.IsAny<Computer>())).Returns(new ValidationResult { IsValid = true });

            var compositeValidator = new CompositeValidator<Computer>(new[] { strategyMock1.Object, strategyMock2.Object });
            var computer = new TestComputer { Name = "TestComputer" };

            var result = compositeValidator.Validate(computer);

            Assert.True(result.IsValid);
            Assert.Equal("All validations passed.", result.Message);
        }

        [Fact]
        public void Validate_WhenAnyStrategyFails_ReturnsFirstInvalidResult()
        {
            using var mock = AutoMock.GetLoose();
            var strategyMock1 = new Mock<IValidationStrategy<Computer>>();
            strategyMock1.Setup(x => x.Validate(It.IsAny<Computer>()))
                         .Returns(new ValidationResult { IsValid = true });

            var strategyMock2 = new Mock<IValidationStrategy<Computer>>();
            strategyMock2.Setup(x => x.Validate(It.IsAny<Computer>()))
                         .Returns(new ValidationResult { IsValid = false, Message = "Failed validation." });

            var compositeValidator = new CompositeValidator<Computer>(new[] { strategyMock1.Object, strategyMock2.Object });
            var computer = new TestComputer { Name = "TestComputer" };

            var result = compositeValidator.Validate(computer);

            Assert.False(result.IsValid);
            Assert.Equal("Failed validation.", result.Message);
        }
    }

}
