using System;
using WakeOnLanLibrary.Interfaces;
using WakeOnLanLibrary.Models;

namespace WakeOnLanLibrary.Services
{
    public class WakeOnLanService
    {
        private readonly IMagicPacketGenerator _packetGenerator;
        private readonly IMagicPacketSender _packetSender;

        public WakeOnLanService(IMagicPacketGenerator packetGenerator, IMagicPacketSender packetSender)
        {
            _packetGenerator = packetGenerator ?? throw new ArgumentNullException(nameof(packetGenerator));
            _packetSender = packetSender ?? throw new ArgumentNullException(nameof(packetSender));
        }

        public void WakeUp(string macAddress, string computerName, string destinationAddress, int port = 9)
        {
            var packetBytes = _packetGenerator.GeneratePacket(macAddress);
            var magicPacket = new MagicPacket
            {
                ComputerName = computerName,
                MacAddress = macAddress,
                PacketBytes = packetBytes,
                DestinationAddress = destinationAddress,
                Port = port,
                CreatedAt = DateTime.Now
            };

            _packetSender.SendPacket(magicPacket);
        }
    }
}
