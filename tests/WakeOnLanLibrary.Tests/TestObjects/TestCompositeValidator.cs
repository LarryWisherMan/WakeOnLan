using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Core.Interfaces.Validation;
using WakeOnLanLibrary.Core.Validation;

namespace WakeOnLanLibrary.Tests.TestObjects
{
    public class TestCompositeValidator<T> : CompositeValidator<T> where T : Computer
    {
        private readonly ValidationResult _testResult;

        public TestCompositeValidator(ValidationResult testResult)
            : base(Enumerable.Empty<IValidationStrategy<T>>()) // Pass an empty list of strategies
        {
            _testResult = testResult;
        }

        public override ValidationResult Validate(T entity)
        {
            return _testResult;
        }
    }

}
