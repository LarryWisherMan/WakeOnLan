using WakeOnLanLibrary.Application.Interfaces.Validation;
using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Core.Validators;


namespace WakeOnLanLibrary.Infrastructure.Services
{

    public class ComputerValidator : IComputerValidator
    {
        private readonly CompositeValidator<Computer> _generalValidator;
        private readonly CompositeValidator<TargetComputer> _targetValidator;
        private readonly CompositeValidator<ProxyComputer> _proxyValidator;

        public ComputerValidator(
            CompositeValidator<Computer> generalValidator,
            CompositeValidator<TargetComputer> targetValidator,
            CompositeValidator<ProxyComputer> proxyValidator)
        {
            _generalValidator = generalValidator;
            _targetValidator = targetValidator;
            _proxyValidator = proxyValidator;
        }

        public ValidationResult Validate<TComputer>(TComputer computer) where TComputer : Computer
        {
            // Perform general validation first
            var generalValidation = _generalValidator.Validate(computer);
            if (!generalValidation.IsValid)
            {
                return generalValidation;
            }

            // Perform specific validation
            return computer switch
            {
                TargetComputer target => _targetValidator.Validate(target),
                ProxyComputer proxy => _proxyValidator.Validate(proxy),
                _ => new ValidationResult { IsValid = true, Message = "Validation passed." }
            };
        }
    }
}

//public class ComputerValidator : IComputerValidator
//{
//    public ValidationResult Validate<TComputer>(TComputer computer) where TComputer : Computer
//    {
//        // Common validation for all computers
//        if (string.IsNullOrWhiteSpace(computer.Name) && string.IsNullOrWhiteSpace(computer.IpAddress))
//        {
//            return new ValidationResult
//            {
//                IsValid = false,
//                Message = "Computer name or IP address must be provided."
//            };
//        }

//        if (!NameIpValidator.IsValidComputerName(computer.Name) &&
//            !NameIpValidator.IsValidIpAddress(computer.IpAddress))
//        {
//            return new ValidationResult
//            {
//                IsValid = false,
//                Message = "Invalid computer name or IP address format."
//            };
//        }

//        // Specific validation for subclasses
//        if (computer is TargetComputer target)
//        {


//            if (NetworkHelper.IsComputerOnline(target.Name))
//            {
//                return new ValidationResult
//                {
//                    IsValid = false,
//                    Message = "Target Computer is already Online"
//                };
//            }


//            if (!MacAddressHelper.IsValidMacAddress(target.MacAddress))
//            {
//                return new ValidationResult
//                {
//                    IsValid = false,
//                    Message = "Invalid MAC address format."
//                };
//            }
//        }
//        else if (computer is ProxyComputer proxy)
//        {
//            if (!NetworkHelper.IsComputerOnline(proxy.Name))
//            {
//                return new ValidationResult
//                {
//                    IsValid = false,
//                    Message = "Proxy computer is not reachable."
//                };
//            }
//        }

//        return new ValidationResult
//        {
//            IsValid = true,
//            Message = "Validation passed."
//        };
//    }
//}


