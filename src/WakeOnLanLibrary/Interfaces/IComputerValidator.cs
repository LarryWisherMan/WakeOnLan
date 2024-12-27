using WakeOnLanLibrary.Models;
namespace WakeOnLanLibrary.Interfaces
{

    public interface IComputerValidator
    {
        ValidationResult Validate<TComputer>(TComputer computer) where TComputer : Computer;
    }
}
