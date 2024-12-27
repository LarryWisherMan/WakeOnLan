using System;
using System.Linq;

namespace WakeOnLanLibrary.Services
{
    public static class MagicPacketGenerator
    {
        public static byte[] GeneratePacket(string macAddress)
        {
            if (string.IsNullOrWhiteSpace(macAddress))
            {
                throw new ArgumentException("MAC address cannot be null or empty.", nameof(macAddress));
            }

            // Convert the MAC address to a byte array
            var macBytes = macAddress.Split(new[] { ':', '-' })
                                      .Select(hex => Convert.ToByte(hex, 16))
                                      .ToArray();

            // Create the magic packet: 6 bytes of 0xFF followed by 16 repetitions of the MAC address
            var packet = new byte[6 + (16 * macBytes.Length)];
            for (int i = 0; i < 6; i++)
            {
                packet[i] = 0xFF; // Synchronization stream
            }

            for (int i = 6; i < packet.Length; i += macBytes.Length)
            {
                Array.Copy(macBytes, 0, packet, i, macBytes.Length);
            }

            return packet;
        }
    }
}
