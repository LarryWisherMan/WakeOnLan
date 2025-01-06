using System;
using System.Threading;
using System.Threading.Tasks;

namespace WakeOnLanLibrary.Application.Interfaces
{
    public interface IMonitoringManager
    {
        Task StartMonitoringAsync(
            int maxPingAttempts,
            int timeoutInSeconds,
            CancellationToken cancellationToken = default);

        void UpdateMonitoringResult(string computerName, bool success, string errorMessage = null);

        event Action<string, bool, string> MonitoringCompleted;
    }
}
