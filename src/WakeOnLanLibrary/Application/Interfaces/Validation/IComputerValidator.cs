using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Core.Validation;

namespace WakeOnLanLibrary.Application.Interfaces.Validation
{
    /// <summary>
    /// Defines a validator for computer objects.
    /// </summary>
    public interface IComputerValidator
    {
        /// <summary>
        /// Validates the specified computer object.
        /// </summary>
        /// <typeparam name="TComputer">The type of computer to validate.</typeparam>
        /// <param name="computer">The computer object to validate.</param>
        /// <returns>A <see cref="ValidationResult"/> indicating whether the validation was successful and any associated messages.</returns>
        ValidationResult Validate<TComputer>(TComputer computer) where TComputer : Computer;
    }
}
