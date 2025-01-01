using System;

namespace WakeOnLanLibrary.Core.Entities
{
    public abstract class Computer
    {
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public string Subnet { get; set; }
        public bool OnlineStatus { get; set; }
        public DateTime LastActivity { get; set; }

    }
}
