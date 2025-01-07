using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using WakeOnLanLibrary.Application.Models;

namespace WakeOnLanLibrary.Application.Interfaces
{
    /// <summary>
    /// Interface for managing Wake-on-LAN (WOL) operations and monitoring.
    /// </summary>
    public interface IWakeOnLanService
    {
        /// <summary>
        /// Sends multiple Wake-on-LAN requests and initiates monitoring for target systems.
        /// </summary>
        /// <param name="proxyToTargets">A dictionary mapping proxy computers to their respective target systems.</param>
        /// <param name="port">The port used for Wake-on-LAN packets. Defaults to the configuration value if null.</param>
        /// <param name="credential">Credentials for accessing proxy computers, if required.</param>
        /// <param name="maxPingAttempts">The maximum number of ping attempts during monitoring. Defaults to the configuration value if null.</param>
        /// <param name="timeoutInSeconds">The timeout duration for monitoring operations, in seconds. Defaults to the configuration value if null.</param>
        /// <returns>A task that returns a collection of results from the Wake-on-LAN operation.</returns>
        Task<IEnumerable<WakeOnLanReturn>> WakeUpAndMonitorAsync(
            Dictionary<string, List<(string MacAddress, string ComputerName)>> proxyToTargets,
            int? port = null,
            PSCredential credential = null,
            int? maxPingAttempts = null,
            int? timeoutInSeconds = null);
    }
}


