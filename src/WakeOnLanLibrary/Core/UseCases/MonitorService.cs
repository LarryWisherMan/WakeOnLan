using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Infrastructure.Monitoring;

namespace WakeOnLanLibrary.Core.UseCases
{
    /// <summary>
    /// Handles monitoring of multiple computer entries, checking their online status, 
    /// and managing cache updates and completion events.
    /// </summary>
    public class MonitorService : IMonitorService
    {
        private readonly IMonitorCache _monitorCache; // Cache for storing and retrieving monitor entries
        private readonly IMonitorTask _monitorTask;   // Task responsible for checking computer status
        private readonly SemaphoreSlim _throttle;    // Controls the number of concurrent monitoring tasks
        private readonly int _intervalInSeconds;     // Interval between monitoring cycles
        private readonly Func<Task> _delayAction;    // Injected delay action for testability
        private readonly Func<DateTime> _timeProvider; // Injected time provider for testability

        /// <summary>
        /// Event triggered when monitoring of an entry is completed.
        /// </summary>
        public event Action<string, bool, string> MonitoringCompleted;

        /// <summary>
        /// Initializes a new instance of the <see cref="MonitorService"/> class.
        /// </summary>
        /// <param name="monitorCache">Cache for storing monitor entries.</param>
        /// <param name="monitorTask">Task responsible for executing monitoring logic.</param>
        /// <param name="delayAction">Optional delay function for testability. Defaults to Task.Delay.</param>
        /// <param name="timeProvider">Optional time provider for testability. Defaults to DateTime.UtcNow.</param>
        /// <param name="maxConcurrentTasks">Maximum number of concurrent monitoring tasks.</param>
        /// <param name="intervalInSeconds">Interval between monitoring cycles, in seconds.</param>
        public MonitorService(
            IMonitorCache monitorCache,
            IMonitorTask monitorTask,
            Func<Task> delayAction = null,
            Func<DateTime> timeProvider = null,
            int maxConcurrentTasks = 5,
            int intervalInSeconds = 10)
        {
            _monitorCache = monitorCache ?? throw new ArgumentNullException(nameof(monitorCache));
            _monitorTask = monitorTask ?? throw new ArgumentNullException(nameof(monitorTask));
            _throttle = new SemaphoreSlim(maxConcurrentTasks);
            _intervalInSeconds = intervalInSeconds;
            _delayAction = delayAction ?? (() => Task.Delay(intervalInSeconds * 1000));
            _timeProvider = timeProvider ?? (() => DateTime.UtcNow);
        }

        /// <summary>
        /// Starts monitoring all entries in the cache.
        /// Periodically checks each entry's status until canceled.
        /// </summary>
        /// <param name="maxPingAttempts">Maximum number of ping attempts per entry.</param>
        /// <param name="timeoutInSeconds">Timeout for monitoring an entry, in seconds.</param>
        /// <param name="cancellationToken">Cancellation token to stop monitoring.</param>
        public async Task StartMonitoringAsync(
     int maxPingAttempts = 5,
     int timeoutInSeconds = 60,
     CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Get entries that are not yet complete
                var entries = GetIncompleteEntries().ToList();

                // Exit the loop if no entries to process
                if (!entries.Any())
                {
                    break;
                }

                // Create monitoring tasks for each entry
                var tasks = entries.Select(entry => MonitorEntryAsync(entry, maxPingAttempts, timeoutInSeconds, cancellationToken)).ToList();

                // Wait for all monitoring tasks to complete
                await Task.WhenAll(tasks);

                // Wait for the configured interval before starting the next cycle
                await _delayAction.Invoke();
            }
        }

        /// <summary>
        /// Retrieves all entries that have not completed monitoring.
        /// </summary>
        /// <returns>Collection of incomplete monitor entries.</returns>
        private IEnumerable<MonitorEntry> GetIncompleteEntries()
        {
            return _monitorCache.GetAll().Where(entry => !entry.IsMonitoringComplete);
        }

        /// <summary>
        /// Monitors a single entry by checking its status and updating the cache.
        /// </summary>
        /// <param name="entry">The monitor entry to check.</param>
        /// <param name="maxPingAttempts">Maximum number of ping attempts.</param>
        /// <param name="timeoutInSeconds">Timeout for the operation, in seconds.</param>
        /// <param name="cancellationToken">Cancellation token for the operation.</param>
        private async Task MonitorEntryAsync(
            MonitorEntry entry,
            int maxPingAttempts,
            int timeoutInSeconds,
            CancellationToken cancellationToken)
        {
            // Wait for a throttle slot to become available
            await _throttle.WaitAsync(cancellationToken);

            try
            {
                // Check if the computer is online
                var isAwake = await _monitorTask.ExecuteAsync(entry, maxPingAttempts, timeoutInSeconds);

                // Update the cache with the result
                UpdateCache(entry, isAwake);

                // Trigger the event if monitoring is complete
                if (entry.IsMonitoringComplete)
                {
                    InvokeMonitoringCompleted(entry, isAwake);
                }
            }
            finally
            {
                // Release the throttle slot
                _throttle.Release();
            }
        }

        /// <summary>
        /// Updates the cache with the results of the monitoring operation.
        /// </summary>
        /// <param name="entry">The entry to update.</param>
        /// <param name="isAwake">Whether the computer is awake.</param>
        private void UpdateCache(MonitorEntry entry, bool isAwake)
        {
            entry.WolSuccess = isAwake;
            entry.IsMonitoringComplete = isAwake || _timeProvider.Invoke() >= entry.MonitoringEndTime;

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
        }

        /// <summary>
        /// Invokes the <see cref="MonitoringCompleted"/> event.
        /// </summary>
        /// <param name="entry">The entry for which monitoring is complete.</param>
        /// <param name="isAwake">Whether the computer is awake.</param>
        private void InvokeMonitoringCompleted(MonitorEntry entry, bool isAwake)
        {
            string errorMessage = isAwake ? null : "Ping response not received within the timeout period.";
            MonitoringCompleted?.Invoke(entry.ComputerName, isAwake, errorMessage);
        }
    }
}

