using System;
using System.Collections.Generic;

namespace WakeOnLanLibrary.Infrastructure.Caching
{
    public interface ICache<TKey, TValue>
    {
        /// <summary>
        /// Adds a new value or updates an existing value in the cache.
        /// </summary>
        /// <param name="key">The key to identify the value.</param>
        /// <param name="value">The value to add or update.</param>
        void AddOrUpdate(TKey key, TValue value);

        /// <summary>
        /// Adds a new value or updates an existing value in the cache using factory methods.
        /// </summary>
        /// <param name="key">The key to identify the value.</param>
        /// <param name="addValueFactory">The factory to create a new value if the key does not exist.</param>
        /// <param name="updateValueFactory">The factory to update the existing value if the key exists.</param>
        void AddOrUpdate(TKey key, Func<TValue> addValueFactory, Action<TValue> updateValueFactory);

        /// <summary>
        /// Retrieves a value by its key.
        /// </summary>
        /// <param name="key">The key to identify the value.</param>
        /// <returns>The value associated with the key.</returns>
        TValue Get(TKey key);

        /// <summary>
        /// Retrieves all values in the cache.
        /// </summary>
        /// <returns>A collection of all values in the cache.</returns>
        ICollection<TValue> GetAll();

        /// <summary>
        /// Removes a value by its key.
        /// </summary>
        /// <param name="key">The key to identify the value.</param>
        void Remove(TKey key);

        /// <summary>
        /// Clears all values from the cache.
        /// </summary>
        void Clear();

        /// <summary>
        /// Attempts to get a value by its key.
        /// </summary>
        /// <param name="key">The key to identify the value.</param>
        /// <param name="value">The value associated with the key if found.</param>
        /// <returns>True if the value is found; otherwise, false.</returns>
        bool TryGetValue(TKey key, out TValue value);

        /// <summary>
        /// Retrieves all keys in the cache.
        /// </summary>
        /// <returns>A collection of all keys in the cache.</returns>
        IEnumerable<TKey> GetAllKeys();

        /// <summary>
        /// Gets the enumerable of all keys.
        /// </summary>
        IEnumerable<TKey> Keys { get; }
    }
}
