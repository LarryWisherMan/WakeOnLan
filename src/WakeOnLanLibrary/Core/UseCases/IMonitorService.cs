using System;
using System.Threading;
using System.Threading.Tasks;

namespace WakeOnLanLibrary.Core.UseCases
{
    /// <summary>
    /// Interface for a service that monitors the online status of computers.
    /// </summary>
    public interface IMonitorService
    {
        /// <summary>
        /// Event triggered when the monitoring of an entry is completed.
        /// </summary>
        event Action<string, bool, string> MonitoringCompleted;

        /// <summary>
        /// Starts the monitoring process for all entries in the cache.
        /// </summary>
        /// <param name="maxPingAttempts">Maximum number of ping attempts per entry.</param>
        /// <param name="timeoutInSeconds">Timeout for monitoring an entry, in seconds.</param>
        /// <param name="cancellationToken">Cancellation token to stop monitoring.</param>
        /// <returns>A task that represents the asynchronous monitoring operation.</returns>
        Task StartMonitoringAsync(
            int maxPingAttempts = 5,
            int timeoutInSeconds = 60,
            CancellationToken cancellationToken = default);
    }
}


