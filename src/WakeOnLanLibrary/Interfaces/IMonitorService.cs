using System.Threading;
using System.Threading.Tasks;
using WakeOnLanLibrary.Models;
using WakeOnLanLibrary.Services;

namespace WakeOnLanLibrary.Interfaces
{
    public interface IMonitorService
    {
        /// <summary>
        /// Gets the monitor cache used for tracking monitored computers.
        /// </summary>
        MonitorCache MonitorCache { get; }

        /// <summary>
        /// Starts monitoring all computers in the monitor cache asynchronously with throttling.
        /// </summary>
        /// <param name="intervalInSeconds">The interval between monitoring cycles.</param>
        /// <param name="maxPingAttempts">The maximum number of ping attempts per computer.</param>
        /// <param name="timeoutInSeconds">The timeout duration for monitoring a computer.</param>
        /// <param name="cancellationToken">Cancellation token to stop monitoring.</param>
        /// <returns>A Task representing the asynchronous monitoring process.</returns>
        Task StartMonitoringAsync(
            int intervalInSeconds = 10,
            int maxPingAttempts = 5,
            int timeoutInSeconds = 60,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Monitors a single computer asynchronously.
        /// </summary>
        /// <param name="entry">The monitor entry for the computer.</param>
        /// <param name="maxPingAttempts">The maximum number of ping attempts.</param>
        /// <param name="timeoutInSeconds">The timeout duration for monitoring the computer.</param>
        /// <returns>A Task returning true if the computer is awake; otherwise, false.</returns>
        Task<bool> MonitorComputerAsync(MonitorEntry entry, int maxPingAttempts, int timeoutInSeconds);
    }
}

