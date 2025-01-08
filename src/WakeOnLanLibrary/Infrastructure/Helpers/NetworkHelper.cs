namespace WakeOnLanLibrary.Infrastructure.Helpers
{
    using System;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
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

        public async Task<bool> IsWsmanAvailableAsync(string computerName)
        {
            if (string.IsNullOrWhiteSpace(computerName))
                throw new ArgumentNullException(nameof(computerName));

            return await Task.Run(() =>
            {
                try
                {
                    // Create a PowerShell runspace
                    using var runspace = RunspaceFactory.CreateRunspace();
                    runspace.Open();

                    // Use the PowerShell class to execute Test-WSMan
                    using var powerShell = PowerShell.Create();
                    powerShell.Runspace = runspace;

                    powerShell.AddCommand("Test-WSMan")
                              .AddParameter("ComputerName", computerName);

                    // Execute the command and check for results
                    var results = powerShell.Invoke();
                    return results.Count > 0; // If results are returned, WSMan is available
                }
                catch
                {
                    // If any exception occurs, WSMan is unavailable
                    return false;
                }
            });
        }
    }
}
