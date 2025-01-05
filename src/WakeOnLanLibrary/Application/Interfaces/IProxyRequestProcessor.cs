using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
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
            RunspacePool runspacePool,
            int maxPingAttempts,
            int timeoutInSeconds);
    }
}
