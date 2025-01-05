using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Infrastructure.Caching;

namespace WakeOnLanLibrary.Application.Interfaces
{
    public interface IMonitorCache : ICache<string, MonitorEntry>
    {
        // Any additional members specific to MonitorCache can be added here.
    }
}
