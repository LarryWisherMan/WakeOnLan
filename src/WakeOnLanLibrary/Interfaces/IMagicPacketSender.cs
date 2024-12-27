using System.Threading.Tasks;
using WakeOnLanLibrary.Models;

namespace WakeOnLanLibrary.Interfaces
{
    public interface IMagicPacketSender
    {
        /// <summary>
        /// Sends a Wake-on-LAN request using the specified WOL request details.
        /// </summary>
        /// <param name="wolRequest">The Wake-on-LAN request object containing all necessary details.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendPacketAsync(WakeOnLanRequest wolRequest);
    }


}
