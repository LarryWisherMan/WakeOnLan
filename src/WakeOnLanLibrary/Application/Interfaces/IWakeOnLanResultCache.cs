using System.Collections.Generic;
using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Infrastructure.Caching;

namespace WakeOnLanLibrary.Application.Interfaces
{
    public interface IWakeOnLanResultCache : ICache<string, WakeOnLanReturn>
    {
        /// <summary>
        /// Retrieves all results from the cache, optionally sorted.
        /// </summary>
        /// <param name="sortDescending">Whether to sort the results in descending order.</param>
        /// <returns>A sorted or unsorted collection of WakeOnLanReturn objects.</returns>
        IEnumerable<WakeOnLanReturn> GetAllResults(bool sortDescending = false);
    }
}
