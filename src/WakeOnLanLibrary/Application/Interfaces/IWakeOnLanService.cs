﻿using System.Collections.Generic;
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
        /// <param name="port">The port used for Wake-on-LAN. Uses default if null.</param>
        /// <param name="credential">The credentials for remote connections, if required.</param>
        /// <param name="maxPingAttempts">The maximum number of ping attempts. Uses default if null.</param>
        /// <param name="timeoutInSeconds">The timeout duration in seconds. Uses default if null.</param>
        /// <returns>A collection of results from the operation.</returns>
        Task<IEnumerable<WakeOnLanReturn>> WakeUpAndMonitorAsync(
            Dictionary<string, List<(string MacAddress, string ComputerName)>> proxyToTargets,
            int? port = null,
            PSCredential credential = null,
            int? maxPingAttempts = null,
            int? timeoutInSeconds = null);
    }
}

