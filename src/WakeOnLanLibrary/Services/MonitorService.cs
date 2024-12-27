using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WakeOnLanLibrary.Helpers;
using WakeOnLanLibrary.Interfaces;
using WakeOnLanLibrary.Models;

namespace WakeOnLanLibrary.Services
{
    public class MonitorService : IMonitorService
    {
        public MonitorCache MonitorCache { get; }

        private readonly SemaphoreSlim _throttle;

        public MonitorService(MonitorCache monitorCache, int maxConcurrentTasks = 5)
        {
            MonitorCache = monitorCache ?? throw new ArgumentNullException(nameof(monitorCache));
            _throttle = new SemaphoreSlim(maxConcurrentTasks);
        }

        /// <summary>
        /// Starts monitoring all computers in the monitor cache asynchronously with throttling.
        /// </summary>
        public async Task StartMonitoringAsync(
            int intervalInSeconds = 10,
            int maxPingAttempts = 5,
            int timeoutInSeconds = 60,
            CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var entries = MonitorCache.GetAllEntries();

                var monitoringTasks = entries
                    .Where(entry => !entry.IsMonitoringComplete)
                    .Select(entry => MonitorWithThrottleAsync(entry, maxPingAttempts, timeoutInSeconds, cancellationToken))
                    .ToList();

                await Task.WhenAll(monitoringTasks);

                await Task.Delay(intervalInSeconds * 1000, cancellationToken); // Delay before the next cycle
            }
        }

        /// <summary>
        /// Monitors a single computer with throttling.
        /// </summary>
        private async Task MonitorWithThrottleAsync(
            MonitorEntry entry,
            int maxPingAttempts,
            int timeoutInSeconds,
            CancellationToken cancellationToken)
        {
            await _throttle.WaitAsync(cancellationToken); // Throttle the number of concurrent tasks
            try
            {
                var isAwake = await MonitorComputerAsync(entry, maxPingAttempts, timeoutInSeconds);

                lock (MonitorCache)
                {
                    entry.WolSuccess = isAwake;
                    entry.IsMonitoringComplete = isAwake || (DateTime.UtcNow - entry.WolSentTime).TotalSeconds > timeoutInSeconds;

                    if (entry.IsMonitoringComplete)
                    {
                        MonitorCache.Remove(entry.ComputerName); // Remove from cache if monitoring is complete
                    }
                }
            }
            finally
            {
                _throttle.Release(); // Release the semaphore for the next task
            }
        }

        /// <summary>
        /// Monitors a single computer by pinging it.
        /// </summary>
        public async Task<bool> MonitorComputerAsync(MonitorEntry entry, int maxPingAttempts, int timeoutInSeconds)
        {
            for (var i = 0; i < maxPingAttempts; i++)
            {
                if (await NetworkHelper.IsComputerOnlineAsync(entry.ComputerName))
                {
                    return true; // Successfully pinged
                }

                await Task.Delay(timeoutInSeconds * 1000 / maxPingAttempts); // Delay before retrying
            }

            return false; // All ping attempts failed
        }
    }
}



