using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;

namespace WakeOnLanLibrary.Application.Interfaces
{
    /// <summary>
    /// Interface for processing proxy requests for Wake-on-LAN (WOL) operations.
    /// </summary>
    public interface IProxyRequestProcessor
    {
        /// <summary>
        /// Processes Wake-on-LAN requests for a proxy computer, sending packets to target systems.
        /// </summary>
        /// <param name="proxyComputerName">The name of the proxy computer managing the WOL requests.</param>
        /// <param name="targets">A list of target computers and their MAC addresses.</param>
        /// <param name="port">The port used for WOL packets.</param>
        /// <param name="credential">Credentials for accessing the proxy computer.</param>
        /// <param name="minRunspaces">The minimum number of runspaces in the runspace pool.</param>
        /// <param name="maxRunspaces">The maximum number of runspaces in the runspace pool.</param>
        /// <param name="maxPingAttempts">The maximum number of ping attempts during monitoring.</param>
        /// <param name="timeoutInSeconds">The timeout duration for monitoring, in seconds.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
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
