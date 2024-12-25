using WakeOnLanLibrary.Models;
namespace WakeOnLanLibrary.interfaces
{

    public interface IComputerValidator
    {
        ValidationResult Validate<TComputer>(TComputer computer) where TComputer : Computer;
    }
}
