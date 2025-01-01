using System;
using WakeOnLanLibrary.Application.Interfaces.Helpers;
using WakeOnLanLibrary.Application.Interfaces.Validation;
using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Core.Entities;

public class TargetComputerValidationStrategy : IValidationStrategy<TargetComputer>
{
    private readonly INetworkHelper _networkHelper;
    private readonly IMacAddressHelper _macAddressHelper;

    public TargetComputerValidationStrategy(INetworkHelper networkHelper, IMacAddressHelper macAddressHelper)
    {
        _networkHelper = networkHelper ?? throw new ArgumentNullException(nameof(networkHelper));
        _macAddressHelper = macAddressHelper ?? throw new ArgumentNullException(nameof(macAddressHelper));
    }

    public ValidationResult Validate(TargetComputer target)
    {
        if (string.IsNullOrWhiteSpace(target.Name))
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = "Target computer name cannot be null or empty."
            };
        }

        if (!_macAddressHelper.IsValidMacAddress(target.MacAddress))
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = "Invalid MAC address format."
            };
        }

        // Use INetworkHelper to check if the target is online
        if (_networkHelper.IsComputerOnlineAsync(target.Name).Result)
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = "Target computer is already online."
            };
        }

        return new ValidationResult
        {
            IsValid = true,
            Message = "Target computer validation passed."
        };
    }
}
