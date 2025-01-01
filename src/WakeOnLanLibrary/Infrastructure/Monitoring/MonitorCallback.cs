using System;
using WakeOnLanLibrary.Application.Interfaces;

namespace WakeOnLanLibrary.Infrastructure.Monitoring
{
    public class MonitorCallback : IMonitorCallback
    {
        private readonly IWakeOnLanService _wakeOnLanService;

        public MonitorCallback(IWakeOnLanService wakeOnLanService)
        {
            _wakeOnLanService = wakeOnLanService ?? throw new ArgumentNullException(nameof(wakeOnLanService));
        }

        public void OnMonitoringComplete(string computerName, bool success, string errorMessage = null)
        {
            _wakeOnLanService.UpdateMonitoringResult(computerName, success, errorMessage);
        }
    }
}
