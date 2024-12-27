using WakeOnLanLibrary.Models;

namespace WakeOnLanLibrary.Interfaces
{
    public interface IComputerFactory
    {
        TargetComputer CreateTargetComputer(string name, string macAddress);
        ProxyComputer CreateProxyComputer(string name, int port);
    }
}
