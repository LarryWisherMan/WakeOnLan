using System;
using WakeOnLanLibrary.Application.Interfaces.Validation;
using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Core.Entities;

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

        // Ensure the Name property is provided
        if (string.IsNullOrWhiteSpace(computer.Name))
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = "Computer name must be provided."
            };
        }

        // Validate the Name as either a valid computer name or IP address
        if (!_nameIpValidator.IsValidComputerName(computer.Name) &&
            !_nameIpValidator.IsValidIpAddress(computer.Name))
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = $"Invalid computer name or IP address: '{computer.Name}'."
            };
        }

        // If all checks pass
        return new ValidationResult { IsValid = true, Message = "Validation passed." };
    }
}
