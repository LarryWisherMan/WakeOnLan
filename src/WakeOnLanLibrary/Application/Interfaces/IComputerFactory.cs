using WakeOnLanLibrary.Core.Entities;

namespace WakeOnLanLibrary.Application.Interfaces
{
    public interface IComputerFactory
    {
        TargetComputer CreateTargetComputer(string name, string macAddress);
        ProxyComputer CreateProxyComputer(string name, int port);
    }
}
