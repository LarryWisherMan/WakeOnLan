using System;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Core.Entities;

namespace WakeOnLanLibrary.Infrastructure.Factories
{
    public class ComputerFactory : IComputerFactory
    {
        public TargetComputer CreateTargetComputer(string name, string macAddress)
        {
            if (string.IsNullOrWhiteSpace(macAddress))
            {
                throw new ArgumentException("MAC address cannot be null or empty.", nameof(macAddress));
            }

            return new TargetComputer
            {
                Name = name,
                MacAddress = macAddress
            };
        }

        public ProxyComputer CreateProxyComputer(string name, int port)
        {
            if (port <= 0)
            {
                throw new ArgumentException("Port must be greater than zero.", nameof(port));
            }

            return new ProxyComputer
            {
                Name = name,
                RelayPort = port
            };
        }
    }
}
