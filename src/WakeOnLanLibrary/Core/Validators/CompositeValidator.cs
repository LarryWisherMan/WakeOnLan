using System.Collections.Generic;
using System.Linq;
using WakeOnLanLibrary.Application.Interfaces.Validation;
using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Core.Entities;

namespace WakeOnLanLibrary.Core.Validators
{
    public class CompositeValidator<TComputer> where TComputer : Computer
    {
        private readonly List<IValidationStrategy<TComputer>> _validationStrategies;

        public CompositeValidator(IEnumerable<IValidationStrategy<TComputer>> validationStrategies)
        {
            _validationStrategies = validationStrategies.ToList();
        }

        public ValidationResult Validate(TComputer computer)
        {
            foreach (var strategy in _validationStrategies)
            {
                var result = strategy.Validate(computer);
                if (!result.IsValid)
                {
                    return result; // Return the first failure
                }
            }

            return new ValidationResult { IsValid = true, Message = "All validations passed." };
        }
    }

}
