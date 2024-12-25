using System.Net;

namespace WakeOnLanLibrary.Helpers
{
    public static class NameIpValidator
    {
        public static bool IsValidComputerName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.Length <= 63;
        }

        public static bool IsValidIpAddress(string ipAddress)
        {
            return IPAddress.TryParse(ipAddress, out _);
        }
    }
}
