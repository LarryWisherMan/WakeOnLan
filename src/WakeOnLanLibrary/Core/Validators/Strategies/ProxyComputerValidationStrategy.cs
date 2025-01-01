using System;
using WakeOnLanLibrary.Application.Interfaces.Helpers;
using WakeOnLanLibrary.Application.Interfaces.Validation;
using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Core.Entities;

public class ProxyComputerValidationStrategy : IValidationStrategy<ProxyComputer>
{
    private readonly INetworkHelper _networkHelper;

    public ProxyComputerValidationStrategy(INetworkHelper networkHelper)
    {
        _networkHelper = networkHelper ?? throw new ArgumentNullException(nameof(networkHelper));
    }

    public ValidationResult Validate(ProxyComputer proxy)
    {
        if (string.IsNullOrWhiteSpace(proxy.Name))
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = "Proxy computer name cannot be null or empty."
            };
        }

        // Use INetworkHelper to check if the proxy is online
        if (!_networkHelper.IsComputerOnlineAsync(proxy.Name).Result)
        {
            return new ValidationResult
            {
                IsValid = false,
                Message = "Proxy computer is not reachable."
            };
        }

        return new ValidationResult
        {
            IsValid = true,
            Message = "Proxy computer validation passed."
        };
    }
}

