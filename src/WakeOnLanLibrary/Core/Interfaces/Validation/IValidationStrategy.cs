using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Core.Entities;

namespace WakeOnLanLibrary.Core.Interfaces.Validation
{
    /// <summary>
    /// Defines a validation strategy for a specific type of computer.
    /// </summary>
    /// <typeparam name="TComputer">The type of computer to validate.</typeparam>
    public interface IValidationStrategy<in TComputer> where TComputer : Computer
    {
        /// <summary>
        /// Validates the given computer and returns the result.
        /// </summary>
        /// <param name="computer">The computer to validate.</param>
        /// <returns>A <see cref="ValidationResult"/> representing the outcome of the validation.</returns>
        ValidationResult Validate(TComputer computer);
    }
}
