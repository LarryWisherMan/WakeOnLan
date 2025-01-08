using System;
using WakeOnLanLibrary.Application.Interfaces.Validation;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Core.Interfaces.Validation;
using WakeOnLanLibrary.Core.Validation;

namespace WakeOnLanLibrary.Application.Validation
{
    /// <summary>
    /// Orchestrates the validation of different types of computer objects.
    /// </summary>
    public class ComputerValidator : IComputerValidator
    {
        private readonly IValidatorFactory _validatorFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputerValidator"/> class.
        /// </summary>
        /// <param name="validatorFactory">The factory for retrieving validators.</param>
        public ComputerValidator(IValidatorFactory validatorFactory)
        {
            _validatorFactory = validatorFactory ?? throw new ArgumentNullException(nameof(validatorFactory));
        }

        /// <summary>
        /// Validates the specified computer object using the appropriate validator.
        /// </summary>
        /// <typeparam name="TComputer">The type of computer to validate.</typeparam>
        /// <param name="computer">The computer object to validate.</param>
        /// <returns>
        /// A <see cref="ValidationResult"/> indicating whether the validation succeeded or failed.
        /// </returns>
        public ValidationResult Validate<TComputer>(TComputer computer) where TComputer : Computer
        {
            if (computer == null)
            {
                throw new ArgumentNullException(nameof(computer), "The computer object cannot be null.");
            }

            // Retrieve the appropriate composite validator for the computer type
            var validator = _validatorFactory.GetValidator<TComputer>();

            // Validate the computer object
            return validator.Validate(computer);
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


