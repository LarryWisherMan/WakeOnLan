using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WakeOnLanLibrary.Models;

namespace WakeOnLanLibrary.Services
{
    public class MonitorCache
    {
        private readonly ConcurrentDictionary<string, MonitorEntry> _cache = new();

        /// <summary>
        /// Adds a monitor entry to the cache.
        /// </summary>
        public void Add(MonitorEntry monitorEntry)
        {
            if (monitorEntry == null)
                throw new ArgumentNullException(nameof(monitorEntry), "Monitor entry cannot be null.");

            if (string.IsNullOrWhiteSpace(monitorEntry.ComputerName))
                throw new ArgumentException("Computer name cannot be null or empty.", nameof(monitorEntry));

            if (string.IsNullOrWhiteSpace(monitorEntry.ProxyComputerName))
                throw new ArgumentException("Proxy computer name cannot be null or empty.", nameof(monitorEntry));

            _cache[monitorEntry.ComputerName] = monitorEntry;
        }

        /// <summary>
        /// Retrieves all monitor entries.
        /// </summary>
        public ICollection<MonitorEntry> GetAllEntries()
        {
            return _cache.Values;
        }

        /// <summary>
        /// Removes a computer from the monitor cache.
        /// </summary>
        public void Remove(string computerName)
        {
            _cache.TryRemove(computerName, out _);
        }

        /// <summary>
        /// Clears all entries from the monitor cache.
        /// </summary>
        public void Clear()
        {
            _cache.Clear();
        }
    }
}
