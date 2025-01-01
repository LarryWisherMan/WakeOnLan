using System.Collections.Generic;
using System.Threading.Tasks;
using WakeOnLanLibrary.Application.Models;

namespace WakeOnLanLibrary.Core.Interfaces
{
    public interface IMagicPacketSender
    {
        /// <summary>
        /// Sends a single magic packet request.
        /// </summary>
        Task SendPacketAsync(WakeOnLanRequest request);

        /// <summary>
        /// Sends multiple magic packet requests.
        /// </summary>
        Task SendPacketAsync(IEnumerable<WakeOnLanRequest> requests);
    }
}
