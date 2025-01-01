using System;

namespace WakeOnLanLibrary.Core.Entities
{
    public class MonitorEntry
    {
        // Identification Properties
        /// <summary>
        /// The name or IP address of the target computer being monitored.
        /// </summary>
        public string ComputerName { get; set; }

        /// <summary>
        /// The name or IP address of the proxy computer used for sending the WOL request.
        /// </summary>
        public string ProxyComputerName { get; set; }

        // Timing Properties
        /// <summary>
        /// The timestamp when the WOL request was sent.
        /// </summary>
        public DateTime WolSentTime { get; set; }

        /// <summary>
        /// The timestamp when the monitoring is expected to end.
        /// </summary>
        public DateTime MonitoringEndTime { get; set; }

        // Status Properties
        /// <summary>
        /// Indicates whether the WOL request was successfully sent.
        /// </summary>
        public bool WolSuccess { get; set; }

        /// <summary>
        /// Indicates whether the monitoring task is complete.
        /// </summary>
        public bool IsMonitoringComplete { get; set; }

        // Monitoring Statistics
        /// <summary>
        /// The number of ping attempts made during monitoring.
        /// </summary>
        public int PingCount { get; set; }
    }
}
