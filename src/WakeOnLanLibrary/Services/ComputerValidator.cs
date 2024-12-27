using WakeOnLanLibrary.Helpers;
using WakeOnLanLibrary.interfaces;
using WakeOnLanLibrary.Models;


namespace WakeOnLanLibrary.Services
{

    public class ComputerValidator : IComputerValidator
    {
        public ValidationResult Validate<TComputer>(TComputer computer) where TComputer : Computer
        {
            // Common validation for all computers
            if (string.IsNullOrWhiteSpace(computer.Name) && string.IsNullOrWhiteSpace(computer.IpAddress))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = "Computer name or IP address must be provided."
                };
            }

            if (!NameIpValidator.IsValidComputerName(computer.Name) &&
                !NameIpValidator.IsValidIpAddress(computer.IpAddress))
            {
                return new ValidationResult
                {
                    IsValid = false,
                    Message = "Invalid computer name or IP address format."
                };
            }

            // Specific validation for subclasses
            if (computer is TargetComputer target)
            {


                if (NetworkHelper.IsComputerOnline(target.Name))
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = "Target Computer is already Online"
                    };
                }


                if (!MacAddressHelper.IsValidMacAddress(target.MacAddress))
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = "Invalid MAC address format."
                    };
                }
            }
            else if (computer is ProxyComputer proxy)
            {
                if (!NetworkHelper.IsComputerOnline(proxy.Name))
                {
                    return new ValidationResult
                    {
                        IsValid = false,
                        Message = "Proxy computer is not reachable."
                    };
                }
            }

            return new ValidationResult
            {
                IsValid = true,
                Message = "Validation passed."
            };
        }
    }

}
