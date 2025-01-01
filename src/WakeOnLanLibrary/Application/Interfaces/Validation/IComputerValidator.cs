using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Core.Entities;
namespace WakeOnLanLibrary.Application.Interfaces.Validation
{

    public interface IComputerValidator
    {
        ValidationResult Validate<TComputer>(TComputer computer) where TComputer : Computer;
    }
}
