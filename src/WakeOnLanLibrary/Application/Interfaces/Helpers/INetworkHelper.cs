using System.Threading.Tasks;

namespace WakeOnLanLibrary.Application.Interfaces.Helpers
{
    public interface INetworkHelper
    {
        Task<bool> IsComputerOnlineAsync(string computerName, int timeoutInMilliseconds = 5000);
        string ResolveHostName(string computerNameOrIpAddress);
        Task<bool> IsWsmanAvailableAsync(string computerName);
    }

}
