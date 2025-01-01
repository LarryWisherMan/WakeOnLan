using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace WakeOnLanLibrary.Infrastructure.Caching
{
    public class Cache<TKey, TValue> : ICache<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, TValue> _cache = new();

        public void AddOrUpdate(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));

            _cache.AddOrUpdate(key, value, (_, _) => value);
        }

        public void AddOrUpdate(TKey key, Func<TValue> addValueFactory, Action<TValue> updateValueFactory)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (addValueFactory == null) throw new ArgumentNullException(nameof(addValueFactory));
            if (updateValueFactory == null) throw new ArgumentNullException(nameof(updateValueFactory));

            _cache.AddOrUpdate(
                key,
                _ => addValueFactory(),
                (_, existingValue) =>
                {
                    updateValueFactory(existingValue);
                    return existingValue;
                });
        }

        public TValue Get(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            _cache.TryGetValue(key, out var value);
            return value;
        }

        public ICollection<TValue> GetAll()
        {
            return _cache.Values;
        }

        public void Remove(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            _cache.TryRemove(key, out _);
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _cache.TryGetValue(key, out value);
        }

        public IEnumerable<TKey> GetAllKeys()
        {
            return _cache.Keys;
        }

        public IEnumerable<TKey> Keys => _cache.Keys; // Implement the Keys property

    }

}
