using System.Threading;
using System.Threading.Tasks;
using WakeOnLanLibrary.Core.Entities;

namespace WakeOnLanLibrary.Infrastructure.Monitoring
{
    public interface IMonitorTask
    {
        Task<bool> ExecuteAsync(
            MonitorEntry entry,
            int maxPingAttempts,
            int timeoutInSeconds,
            CancellationToken cancellationToken = default);
    }

}
