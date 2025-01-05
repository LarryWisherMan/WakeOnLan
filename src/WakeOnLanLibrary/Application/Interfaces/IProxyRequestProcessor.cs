using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;

namespace WakeOnLanLibrary.Application.Interfaces
{
    public interface IProxyRequestProcessor
    {
        Task ProcessProxyRequestsAsync(
            string proxyComputerName,
            List<(string MacAddress, string ComputerName)> targets,
            int port,
            PSCredential credential,
            int minRunspaces,
            int maxRunspaces,
            int maxPingAttempts,
            int timeoutInSeconds);
    }

}
