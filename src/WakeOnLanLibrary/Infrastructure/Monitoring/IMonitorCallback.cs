namespace WakeOnLanLibrary.Infrastructure.Monitoring
{
    public interface IMonitorCallback
    {
        /// <summary>
        /// Called when monitoring for a specific computer is complete.
        /// </summary>
        /// <param name="computerName">The name of the computer being monitored.</param>
        /// <param name="success">Indicates whether the monitoring was successful (e.g., the computer responded to pings).</param>
        /// <param name="errorMessage">An optional error message if monitoring failed.</param>
        void OnMonitoringComplete(string computerName, bool success, string errorMessage = null);
    }
}
