using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using WakeOnLanLibrary.Application.Models;

namespace WakeOnLanLibrary.Application.Interfaces
{
    public interface IWakeOnLanService
    {

        /// <summary>
        /// Sends multiple Wake-on-LAN requests and adds them to the monitoring process.
        /// </summary>
        /// <param name="proxyToTargets">A dictionary of proxies to their target computers.</param>
        /// <param name="port">The port used for Wake-on-LAN.</param>
        /// <param name="credential">The credentials for remote connections, if required.</param>
        /// <param name="maxPingAttempts">The maximum number of ping attempts.</param>
        /// <param name="timeoutInSeconds">The timeout duration in seconds.</param>
        /// <returns>A collection of results from the operation.</returns>
        Task<IEnumerable<WakeOnLanReturn>> WakeUpAndMonitorAsync(
            Dictionary<string, List<(string MacAddress, string ComputerName)>> proxyToTargets,
            int port = 9,
            PSCredential credential = null,
            int maxPingAttempts = 5,
            int timeoutInSeconds = 60);

        /// <summary>
        /// Updates the monitoring result for a specific computer.
        /// </summary>
        /// <param name="computerName">The name of the computer.</param>
        /// <param name="success">Indicates whether the operation was successful.</param>
        /// <param name="errorMessage">An optional error message.</param>
        void UpdateMonitoringResult(string computerName, bool success, string errorMessage = null);
    }
}
