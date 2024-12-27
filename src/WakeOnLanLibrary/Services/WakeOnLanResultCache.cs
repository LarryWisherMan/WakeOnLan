using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (result == null) throw new ArgumentNullException(nameof(result));

            _results.AddOrUpdate(key, result, (_, _) => result);
        }

        /// <summary>
        /// Retrieves the result of a WOL operation by key.
        /// </summary>
        public WakeOnLanReturn Get(string key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            _results.TryGetValue(key, out var result);
            return result;
        }

        /// <summary>
        /// Retrieves all WOL results.
        /// </summary>
        public IEnumerable<WakeOnLanReturn> GetAll(bool sortDescending = false)
        {
            var results = _results.Values;
            return sortDescending
                ? results.OrderByDescending(result => result.Timestamp)
                : results.OrderBy(result => result.Timestamp);
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

