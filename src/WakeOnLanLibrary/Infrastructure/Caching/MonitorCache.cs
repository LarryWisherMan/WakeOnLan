using System;
using System.Collections.Generic;
using WakeOnLanLibrary.Core.Entities;

namespace WakeOnLanLibrary.Infrastructure.Caching
{

    public class MonitorCache : ICache<string, MonitorEntry>
    {
        private readonly ICache<string, MonitorEntry> _cache;

        public MonitorCache(ICache<string, MonitorEntry> cache)
        {
            _cache = cache;
        }

        public void AddOrUpdate(string key, MonitorEntry value) => _cache.AddOrUpdate(key, value);

        // New AddOrUpdate overload
        public void AddOrUpdate(string key, Func<MonitorEntry> addValueFactory, Action<MonitorEntry> updateValueFactory)
        {
            _cache.AddOrUpdate(
                key,
                addValueFactory,
                existingEntry =>
                {
                    updateValueFactory(existingEntry); // Use the factory to update the entry.
                });
        }

        public MonitorEntry Get(string key) => _cache.Get(key);
        public ICollection<MonitorEntry> GetAll() => _cache.GetAll();
        public void Remove(string key) => _cache.Remove(key);
        public void Clear() => _cache.Clear();

        public bool TryGetValue(string key, out MonitorEntry value)
        {
            return _cache.TryGetValue(key, out value);
        }

        public IEnumerable<string> Keys => _cache.Keys;
        public IEnumerable<string> GetAllKeys() => _cache.Keys;
    }

}
