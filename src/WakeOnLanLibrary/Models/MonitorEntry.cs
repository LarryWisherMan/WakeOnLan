using System;

namespace WakeOnLanLibrary.Models
{
    public class MonitorEntry
    {
        /// <summary>
        /// The name or IP address of the target computer being monitored.
        /// </summary>
        public string ComputerName { get; set; }

        /// <summary>
        /// The name or IP address of the proxy computer used for sending the WOL request.
        /// </summary>
        public string ProxyComputerName { get; set; }

        /// <summary>
        /// The timestamp when the WOL request was sent.
        /// </summary>
        public DateTime WolSentTime { get; set; }

        /// <summary>
        /// Indicates whether the WOL request was successfully sent.
        /// </summary>
        public bool WolSuccess { get; set; }

        /// <summary>
        /// Indicates whether the monitoring task is complete.
        /// </summary>
        public bool IsMonitoringComplete { get; set; }
    }
}
