using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using WakeOnLanLibrary.Core.Interfaces;

namespace WakeOnLanLibrary.Application.Interfaces
{
    public interface IProxyRequestProcessor
    {
        Task ProcessProxyRequestsAsync(
            string proxyComputerName,
            List<(string MacAddress, string ComputerName)> targets,
            int port,
            PSCredential credential,
            IRunspacePool runspacePool,
            int maxPingAttempts,
            int timeoutInSeconds);
    }
}
