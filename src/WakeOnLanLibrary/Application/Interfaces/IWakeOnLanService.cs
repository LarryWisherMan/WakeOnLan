using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Infrastructure.Caching;

namespace WakeOnLanLibrary.Application.Interfaces
{
    public interface IWakeOnLanService
    {
        MonitorCache MonitorCache { get; }
        WakeOnLanResultCache ResultCache { get; }

        Task<IEnumerable<WakeOnLanReturn>> WakeUpAndMonitorAsync(
            Dictionary<string, List<(string MacAddress, string ComputerName)>> proxyToTargets,
            int port = 9,
            PSCredential credential = null,
            int maxPingAttempts = 5,
            int timeoutInSeconds = 60);

        void UpdateMonitoringResult(string computerName, bool success, string errorMessage = null);
    }
}
