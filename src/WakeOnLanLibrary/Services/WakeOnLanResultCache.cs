using System.Collections.Concurrent;
using System.Collections.Generic;
using WakeOnLanLibrary.Models;

namespace WakeOnLanLibrary.Services
{
    public class WakeOnLanResultCache
    {
        private readonly ConcurrentDictionary<string, WakeOnLanReturn> _results = new();

        /// <summary>
        /// Adds or updates the result of a WOL operation in the cache.
        /// </summary>
        public void AddOrUpdate(string key, WakeOnLanReturn result)
        {
            _results[key] = result;
        }

        /// <summary>
        /// Retrieves the result of a WOL operation by key.
        /// </summary>
        public WakeOnLanReturn Get(string key)
        {
            _results.TryGetValue(key, out var result);
            return result;
        }

        /// <summary>
        /// Retrieves all WOL results.
        /// </summary>
        public IEnumerable<WakeOnLanReturn> GetAll()
        {
            return _results.Values;
        }

        /// <summary>
        /// Clears all results from the cache.
        /// </summary>
        public void Clear()
        {
            _results.Clear();
        }
    }
}
