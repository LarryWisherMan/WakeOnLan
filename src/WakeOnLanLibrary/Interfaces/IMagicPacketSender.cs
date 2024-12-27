using System.Collections.Generic;
using System.Threading.Tasks;
using WakeOnLanLibrary.Models;

namespace WakeOnLanLibrary.Interfaces
{
    public interface IMagicPacketSender
    {
        /// <summary>
        /// Sends a Wake-on-LAN packet for a single request.
        /// </summary>
        /// <param name="wolRequest">The Wake-on-LAN request to process.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendPacketAsync(WakeOnLanRequest wolRequest);

        /// <summary>
        /// Sends Wake-on-LAN packets for multiple requests in a batch.
        /// </summary>
        /// <param name="wolRequests">The collection of Wake-on-LAN requests to process.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendPacketAsync(IEnumerable<WakeOnLanRequest> wolRequests);
    }
}
