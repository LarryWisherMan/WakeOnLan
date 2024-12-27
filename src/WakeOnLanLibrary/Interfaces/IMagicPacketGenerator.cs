
namespace WakeOnLanLibrary.Interfaces
{
    public interface IMagicPacketGenerator
    {
        /// <summary>
        /// Generates a magic packet for the given MAC address.
        /// </summary>
        /// <param name="macAddress">The MAC address of the target device.</param>
        /// <returns>A byte array representing the magic packet.</returns>
        byte[] GeneratePacket(string macAddress);
    }
}
