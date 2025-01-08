using Autofac.Extras.Moq;
using Moq;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Core.Interfaces.Validation;
using WakeOnLanLibrary.Core.Validation;
using WakeOnLanLibrary.Infrastructure.Factories;

namespace WakeOnLanLibrary.Tests.Validators
{
    public class ValidatorFactoryTests
    {
        [Fact]
        public void Constructor_WhenServiceProviderIsNull_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ValidatorFactory(null));
        }

        [Fact]
        public void GetValidator_WhenNoStrategiesRegistered_ThrowsInvalidOperationException()
        {
            using var mock = AutoMock.GetLoose();
            var serviceProviderMock = mock.Mock<IServiceProvider>();

            // Simulate no validation strategies being registered
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IEnumerable<IValidationStrategy<Computer>>)))
                .Returns(new List<IValidationStrategy<Computer>>());

            var factory = new ValidatorFactory(serviceProviderMock.Object);

            var exception = Assert.Throws<InvalidOperationException>(() => factory.GetValidator<Computer>());
            Assert.Equal("No validation strategies registered for type Computer.", exception.Message);
        }

        [Fact]
        public void GetValidator_WhenStrategiesRegistered_ReturnsCompositeValidator()
        {
            using var mock = AutoMock.GetLoose();

            var strategyMock = new Mock<IValidationStrategy<Computer>>();

            var serviceProviderMock = mock.Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IEnumerable<IValidationStrategy<Computer>>)))
                .Returns(new List<IValidationStrategy<Computer>> { strategyMock.Object });

            var factory = new ValidatorFactory(serviceProviderMock.Object);

            var validator = factory.GetValidator<Computer>();

            Assert.NotNull(validator);
            Assert.IsType<CompositeValidator<Computer>>(validator);
        }

        [Fact]
        public void GetValidator_WhenMultipleStrategiesRegistered_ReturnsCompositeValidatorWithAllStrategies()
        {
            using var mock = AutoMock.GetLoose();

            var strategyMock1 = new Mock<IValidationStrategy<Computer>>();
            var strategyMock2 = new Mock<IValidationStrategy<Computer>>();

            var serviceProviderMock = mock.Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetService(typeof(IEnumerable<IValidationStrategy<Computer>>)))
                .Returns(new List<IValidationStrategy<Computer>> { strategyMock1.Object, strategyMock2.Object });

            var factory = new ValidatorFactory(serviceProviderMock.Object);

            var validator = factory.GetValidator<Computer>();

            Assert.NotNull(validator);
            Assert.IsType<CompositeValidator<Computer>>(validator);

            // Ensure the composite validator contains all strategies
            var compositeValidator = (CompositeValidator<Computer>)validator;
            Assert.Equal(2, compositeValidator.Strategies.Count);
            Assert.Contains(strategyMock1.Object, compositeValidator.Strategies);
            Assert.Contains(strategyMock2.Object, compositeValidator.Strategies);
        }
    }

}
