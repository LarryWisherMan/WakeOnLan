using System.Net.NetworkInformation;

namespace WakeOnLanLibrary.Helpers
{
    public static class NetworkHelper
    {
        public static bool IsComputerOnline(string ipAddress)
        {
            try
            {
                var ping = new Ping();
                var reply = ping.Send(ipAddress, 1000);
                return reply != null && reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }
    }
}
