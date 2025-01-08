using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Core.Validators;

namespace WakeOnLanLibrary.Core.Interfaces.Validation
{
    /// <summary>
    /// Defines a factory for retrieving validators for specific computer types.
    /// </summary>
    public interface IValidatorFactory
    {
        /// <summary>
        /// Retrieves a composite validator for the specified type of computer.
        /// </summary>
        /// <typeparam name="TComputer">The type of computer to validate.</typeparam>
        /// <returns>A <see cref="CompositeValidator{TComputer}"/> for validating the specified type of computer.</returns>
        CompositeValidator<TComputer> GetValidator<TComputer>() where TComputer : Computer;
    }
}
