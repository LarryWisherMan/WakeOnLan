﻿using System;
using WakeOnLanLibrary.Core.Interfaces;

namespace WakeOnLanLibrary.Application.Models
{
    public class WakeOnLanRequest
    {
        /// <summary>
        /// The target computer's name.
        /// </summary>
        public string TargetComputerName { get; set; }

        /// <summary>
        /// The MAC address of the target computer.
        /// </summary>
        public string TargetMacAddress { get; set; }

        /// <summary>
        /// The port to use for the WOL request (default: 9).
        /// </summary>
        public int Port { get; set; } = 9;

        /// <summary>
        /// The RunspacePool associated with the proxy computer.
        /// </summary>
        public IRunspacePool ProxyRunspacePool { get; set; }

        /// <summary>
        /// Timestamp indicating when the request was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
