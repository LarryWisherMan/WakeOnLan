using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Core.Entities;

namespace WakeOnLanLibrary.Application.Interfaces.Validation
{
    public interface IValidationStrategy<TComputer> where TComputer : Computer
    {
        ValidationResult Validate(TComputer computer);
    }

}
