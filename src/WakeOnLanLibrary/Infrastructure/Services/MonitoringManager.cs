using System;
using System.Threading;
using System.Threading.Tasks;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Core.UseCases;

namespace WakeOnLanLibrary.Infrastructure.Services
{
    public class MonitoringManager : IMonitoringManager
    {
        private readonly IMonitorService _monitorService;
        public event Action<string, bool, string> MonitoringCompleted;

        public MonitoringManager(IMonitorService monitorService)
        {
            _monitorService = monitorService ?? throw new ArgumentNullException(nameof(monitorService));

            // Subscribe to the MonitoringCompleted event from the MonitorService
            _monitorService.MonitoringCompleted += OnMonitoringCompleted;
        }

        public async Task StartMonitoringAsync(
            int maxPingAttempts,
            int timeoutInSeconds,
            CancellationToken cancellationToken = default)
        {
            // Delegate the monitoring process to IMonitorService
            await _monitorService.StartMonitoringAsync(maxPingAttempts, timeoutInSeconds, cancellationToken);
        }

        public void UpdateMonitoringResult(string computerName, bool success, string errorMessage = null)
        {
            // Propagate updates to MonitoringCompleted subscribers
            MonitoringCompleted?.Invoke(computerName, success, errorMessage);
        }

        private void OnMonitoringCompleted(string computerName, bool success, string errorMessage)
        {
            // Handle the MonitoringCompleted event from MonitorService
            MonitoringCompleted?.Invoke(computerName, success, errorMessage);

        }
    }

}
