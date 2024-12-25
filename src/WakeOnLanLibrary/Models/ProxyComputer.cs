using System;

namespace WakeOnLanLibrary.Models
{
    public class ProxyComputer : Computer
    {
        public bool RelayCapability { get; set; } = true;
        public int RelayPort { get; set; } = 9;
        public string Subnet { get; set; }
        public bool OnlineStatus { get; set; }
        public string Role { get; set; } = "WakeOnLanRelay";
        public DateTime LastActivity { get; set; }
    }
}
