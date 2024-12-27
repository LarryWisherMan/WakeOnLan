using System;

namespace WakeOnLanLibrary.Models
{
    public class WakeOnLanReturn
    {
        /// <summary>
        /// The name of the target computer.
        /// </summary>
        public string TargetComputerName { get; set; }

        /// <summary>
        /// The MAC address of the target computer.
        /// </summary>
        public string TargetMacAddress { get; set; }

        /// <summary>
        /// The name of the proxy computer used for the operation.
        /// </summary>
        public string ProxyComputerName { get; set; }

        /// <summary>
        /// The port used for the WOL request.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Timestamp when the operation was executed.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Whether the operation was successful.
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Error message if the operation failed.
        /// </summary>
        public string ErrorMessage { get; set; }
    }
}
