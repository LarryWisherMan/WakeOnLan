using System;
using System.Collections.Generic;
using System.Linq;
using WakeOnLanLibrary.Application.Models;

namespace WakeOnLanLibrary.Infrastructure.Caching
{
    public class WakeOnLanResultCache : ICache<string, WakeOnLanReturn>
    {
        private readonly ICache<string, WakeOnLanReturn> _cache;


        public WakeOnLanResultCache(ICache<string, WakeOnLanReturn> cache)
        {
            _cache = cache;
        }

        public void AddOrUpdate(string key, WakeOnLanReturn value) => _cache.AddOrUpdate(key, value);

        public void AddOrUpdate(string key, Func<WakeOnLanReturn> addValueFactory, Action<WakeOnLanReturn> updateValueFactory)
        {
            _cache.AddOrUpdate(
                key,
                addValueFactory,
                existingEntry =>
                {
                    updateValueFactory(existingEntry);
                });
        }


        public WakeOnLanReturn Get(string key) => _cache.Get(key);
        public ICollection<WakeOnLanReturn> GetAll() => _cache.GetAll();
        public void Remove(string key) => _cache.Remove(key);
        public void Clear() => _cache.Clear();

        public IEnumerable<WakeOnLanReturn> GetAllResults(bool sortDescending = false)
        {
            var results = GetAll();
            return sortDescending
                ? results.OrderByDescending(result => result.Timestamp)
                : results.OrderBy(result => result.Timestamp);
        }

        public bool TryGetValue(string key, out WakeOnLanReturn value)
        {
            return _cache.TryGetValue(key, out value);
        }

        public IEnumerable<string> Keys => _cache.Keys;
        public IEnumerable<string> GetAllKeys() => _cache.Keys;
    }
}

