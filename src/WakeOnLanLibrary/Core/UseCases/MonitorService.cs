using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Infrastructure.Caching;
using WakeOnLanLibrary.Infrastructure.Monitoring;

namespace WakeOnLanLibrary.Core.UseCases
{

    public class MonitorService : IMonitorService
    {
        private readonly MonitorCache _monitorCache;
        private readonly IMonitorTask _monitorTask;
        private readonly SemaphoreSlim _throttle;
        private readonly int _intervalInSeconds;

        public event Action<string, bool, string> MonitoringCompleted;

        public MonitorService(
            MonitorCache monitorCache,
            IMonitorTask monitorTask,
            int maxConcurrentTasks = 5,
            int intervalInSeconds = 10)
        {
            _monitorCache = monitorCache ?? throw new ArgumentNullException(nameof(monitorCache));
            _monitorTask = monitorTask ?? throw new ArgumentNullException(nameof(monitorTask));
            _throttle = new SemaphoreSlim(maxConcurrentTasks);
            _intervalInSeconds = intervalInSeconds;
        }

        public async Task StartMonitoringAsync(
            int maxPingAttempts = 5,
            int timeoutInSeconds = 60,
            CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var entries = _monitorCache.GetAll();

                var tasks = entries
                    .Where(entry => !entry.IsMonitoringComplete)
                    .Select(entry => MonitorEntryAsync(entry, maxPingAttempts, timeoutInSeconds, cancellationToken))
                    .ToList();

                await Task.WhenAll(tasks);
                await Task.Delay(_intervalInSeconds * 1000, cancellationToken);
            }
        }

        private async Task MonitorEntryAsync(
            MonitorEntry entry,
            int maxPingAttempts,
            int timeoutInSeconds,
            CancellationToken cancellationToken)
        {
            await _throttle.WaitAsync(cancellationToken);

            try
            {

                var isAwake = await _monitorTask.ExecuteAsync(entry, maxPingAttempts, timeoutInSeconds);

                // Update entry properties based on the monitoring results
                entry.WolSuccess = isAwake;
                entry.IsMonitoringComplete = isAwake || DateTime.UtcNow >= entry.MonitoringEndTime;

                // Update the cache atomically using the new AddOrUpdate overload
                _monitorCache.AddOrUpdate(
                    entry.ComputerName,
                    // Create a new entry if it does not exist
                    () => new MonitorEntry
                    {
                        ComputerName = entry.ComputerName,
                        ProxyComputerName = entry.ProxyComputerName,
                        WolSentTime = entry.WolSentTime,
                        MonitoringEndTime = entry.MonitoringEndTime,
                        WolSuccess = entry.WolSuccess,
                        IsMonitoringComplete = entry.IsMonitoringComplete,
                        PingCount = entry.PingCount
                    },
                    // Update the existing entry
                    existingEntry =>
                    {
                        existingEntry.WolSuccess = entry.WolSuccess;
                        existingEntry.IsMonitoringComplete = entry.IsMonitoringComplete;
                        existingEntry.PingCount = entry.PingCount;
                        existingEntry.MonitoringEndTime = entry.MonitoringEndTime;
                    });

                // If monitoring is complete, invoke the callback
                if (entry.IsMonitoringComplete)
                {
                    string errorMessage = isAwake ? null : "Ping response not received within the timeout period.";
                    MonitoringCompleted?.Invoke(entry.ComputerName, isAwake, errorMessage);
                }
            }
            finally
            {
                _throttle.Release();
            }
        }
    }

}



