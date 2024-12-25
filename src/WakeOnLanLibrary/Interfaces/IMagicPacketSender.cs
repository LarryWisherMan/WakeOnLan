
using WakeOnLanLibrary.Models;

namespace WakeOnLanLibrary.Interfaces
{
    public interface IMagicPacketSender
    {
        /// <summary>
        /// Sends the provided magic packet.
        /// </summary>
        /// <param name="packet">The magic packet to send.</param>
        void SendPacket(MagicPacket packet);
    }
}
