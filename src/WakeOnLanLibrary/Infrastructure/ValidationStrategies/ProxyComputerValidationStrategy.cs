using System;
using WakeOnLanLibrary.Application.Interfaces.Helpers;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Core.Interfaces.Validation;
using WakeOnLanLibrary.Core.Validation;

namespace WakeOnLanLibrary.Infrastructure.ValidationStrategies
{
    public class ProxyComputerValidationStrategy : IValidationStrategy<ProxyComputer>
    {
        private readonly INetworkHelper _networkHelper;

        public ProxyComputerValidationStrategy(INetworkHelper networkHelper)
        {
            _networkHelper = networkHelper ?? throw new ArgumentNullException(nameof(networkHelper));
        }

        public ValidationResult Validate(ProxyComputer proxy)
        {
            if (proxy == null)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = "Proxy computer cannot be null."
                };
            }

            if (string.IsNullOrWhiteSpace(proxy.Name))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = "Proxy computer name cannot be null or empty."
                };
            }

            try
            {
                // Check if the proxy is online
                if (!_networkHelper.IsComputerOnlineAsync(proxy.Name).GetAwaiter().GetResult())
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = "Proxy computer is not reachable."
                    };
                }

                // Check if WSMan is available
                if (!_networkHelper.IsWsmanAvailableAsync(proxy.Name).GetAwaiter().GetResult())
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = "Proxy computer does not have WSMan available."
                    };
                }
            }
            catch (Exception ex)
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = $"An error occurred while validating the proxy computer: {ex.Message}"
                };
            }

            return new ValidationResult
            {
                IsValid = true,
                Message = "Proxy computer validation passed."
            };
        }
    }
}


