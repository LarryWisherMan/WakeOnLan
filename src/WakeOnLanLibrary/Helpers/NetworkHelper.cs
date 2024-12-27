using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace WakeOnLanLibrary.Helpers
{
    public static class NetworkHelper
    {
        /// <summary>
        /// Checks if a computer is online by sending a ping.
        /// </summary>
        /// <param name="hostOrIpAddress">The hostname or IP address of the computer.</param>
        /// <returns>True if the computer is online; otherwise, false.</returns>
        public static bool IsComputerOnline(string hostOrIpAddress)
        {
            try
            {
                using var ping = new Ping();
                var reply = ping.Send(hostOrIpAddress, 1000); // 1-second timeout
                return reply != null && reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Asynchronously checks if a computer is online by sending a ping.
        /// </summary>
        /// <param name="hostOrIpAddress">The hostname or IP address of the computer.</param>
        /// <returns>A Task representing the asynchronous operation. True if the computer is online; otherwise, false.</returns>
        public static async Task<bool> IsComputerOnlineAsync(string hostOrIpAddress)
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(hostOrIpAddress, 1000); // 1-second timeout
                return reply != null && reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }
    }
}
