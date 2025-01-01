using System;
using System.Threading;
using System.Threading.Tasks;
using WakeOnLanLibrary.Application.Interfaces.Helpers;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Infrastructure.Monitoring;

public class MonitorTask : IMonitorTask
{
    private readonly INetworkHelper _networkHelper;

    public MonitorTask(INetworkHelper networkHelper)
    {
        _networkHelper = networkHelper ?? throw new ArgumentNullException(nameof(networkHelper));
    }

    public async Task<bool> ExecuteAsync(
        MonitorEntry entry,
        int maxPingAttempts,
        int timeoutInSeconds,
        CancellationToken cancellationToken = default)
    {
        if (entry == null) throw new ArgumentNullException(nameof(entry));
        if (maxPingAttempts <= 0) throw new ArgumentOutOfRangeException(nameof(maxPingAttempts), "Must be greater than zero.");
        if (timeoutInSeconds <= 0) throw new ArgumentOutOfRangeException(nameof(timeoutInSeconds), "Must be greater than zero.");

        var interval = (timeoutInSeconds * 1000) / maxPingAttempts;

        for (var i = 0; i < maxPingAttempts; i++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                // Exit early if cancellation is requested
                return false;
            }

            entry.PingCount++; // Increment PingCount

            if (await _networkHelper.IsComputerOnlineAsync(entry.ComputerName))
            {
                return true; // Successfully pinged
            }

            // Delay between attempts, honoring the interval and cancellation token
            try
            {
                await Task.Delay(interval, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                return false; // Exit gracefully if task is canceled
            }
        }

        return false; // All ping attempts failed
    }
}

