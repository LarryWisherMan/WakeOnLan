using System;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Core.Interfaces.Validation;
using WakeOnLanLibrary.Core.Validation;


namespace WakeOnLanLibrary.Core.ValidationStrategies
{
    public class GeneralComputerValidationStrategy : IValidationStrategy<Computer>
    {
        private readonly INameIpValidator _nameIpValidator;

        public GeneralComputerValidationStrategy(INameIpValidator nameIpValidator)
        {
            _nameIpValidator = nameIpValidator ?? throw new ArgumentNullException(nameof(nameIpValidator));
        }

        public ValidationResult Validate(Computer computer)
        {
            if (computer == null)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = "Computer object cannot be null."
                };
            }

            if (string.IsNullOrWhiteSpace(computer.Name))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = "Computer name must be provided."
                };
            }

            if (!IsNameOrIpValid(computer.Name))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = $"Invalid computer name or IP address: '{computer.Name}'."
                };
            }

            return new ValidationResult
            {
                IsValid = true,
                Message = "Validation passed."
            };
        }

        private bool IsNameOrIpValid(string name)
        {
            return _nameIpValidator.IsValidComputerName(name) || _nameIpValidator.IsValidIpAddress(name);
        }
    }


}



