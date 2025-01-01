namespace WakeOnLanLibrary.Infrastructure.Helpers
{
    using System;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Threading.Tasks;
    using WakeOnLanLibrary.Application.Interfaces.Helpers;

    public class NetworkHelper : INetworkHelper
    {
        public async Task<bool> IsComputerOnlineAsync(string computerName, int timeoutInMilliseconds = 5000)
        {
            if (string.IsNullOrWhiteSpace(computerName))
                throw new ArgumentNullException(nameof(computerName));

            try
            {
                var ping = new Ping();
                var reply = await ping.SendPingAsync(computerName, timeoutInMilliseconds);
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false; // Return false on any exception to indicate the computer is offline
            }
        }

        public string ResolveHostName(string computerNameOrIpAddress)
        {
            if (string.IsNullOrWhiteSpace(computerNameOrIpAddress))
                throw new ArgumentNullException(nameof(computerNameOrIpAddress));

            try
            {
                var hostEntry = Dns.GetHostEntry(computerNameOrIpAddress);
                return hostEntry.HostName;
            }
            catch
            {
                return computerNameOrIpAddress; // Return the input if resolution fails
            }
        }
    }

}
