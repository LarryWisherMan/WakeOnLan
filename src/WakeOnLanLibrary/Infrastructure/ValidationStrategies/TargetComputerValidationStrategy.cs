using System;
using WakeOnLanLibrary.Application.Interfaces.Helpers;
using WakeOnLanLibrary.Application.Interfaces.Validation;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Core.Interfaces.Validation;
using WakeOnLanLibrary.Core.Validation;

namespace WakeOnLanLibrary.Infrastructure.ValidationStrategies
{
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
            if (target == null)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = "Target computer cannot be null."
                };
            }

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
            try
            {
                if (_networkHelper.IsComputerOnlineAsync(target.Name).GetAwaiter().GetResult())
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = "Target computer is already online."
                    };
                }
            }
            catch (Exception ex)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = $"An error occurred while checking if the target computer is online: {ex.Message}"
                };
            }

            return new ValidationResult
            {
                IsValid = true,
                Message = "Target computer validation passed."
            };
        }
    }


}

