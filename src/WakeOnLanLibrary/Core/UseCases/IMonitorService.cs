using System;
using System.Threading;
using System.Threading.Tasks;

namespace WakeOnLanLibrary.Core.UseCases
{
    public interface IMonitorService
    {


        /// <summary>
        /// Starts monitoring all computers in the monitor cache asynchronously with throttling.
        /// </summary>
        /// <param name="maxPingAttempts">The maximum number of ping attempts per computer.</param>
        /// <param name="timeoutInSeconds">The timeout duration for monitoring a computer.</param>
        /// <param name="cancellationToken">Cancellation token to stop monitoring.</param>
        /// <returns>A Task representing the asynchronous monitoring process.</returns>

        Task StartMonitoringAsync(
            int maxPingAttempts = 5,
            int timeoutInSeconds = 60,
            CancellationToken cancellationToken = default);

        event Action<string, bool, string> MonitoringCompleted;

    }
}

