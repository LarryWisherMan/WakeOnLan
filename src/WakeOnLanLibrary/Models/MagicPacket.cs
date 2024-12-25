using System;

namespace WakeOnLanLibrary.Models
{
    public class MagicPacket
    {
        public string ComputerName { get; set; } // Name of the target computer
        public string MacAddress { get; set; }  // MAC address of the target computer
        public byte[] PacketBytes { get; set; } // Raw magic packet data
        public int Port { get; set; } = 9;      // Destination port (default: 9)
        public string DestinationAddress { get; set; } // Broadcast address
        public DateTime CreatedAt { get; set; } = DateTime.Now; // Timestamp
    }
}
