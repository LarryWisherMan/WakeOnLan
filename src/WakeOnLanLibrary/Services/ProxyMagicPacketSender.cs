using System;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using WakeOnLanLibrary.Interfaces;
using WakeOnLanLibrary.Models;

namespace WakeOnLanLibrary.Services
{
    public class ProxyMagicPacketSender : IMagicPacketSender
    {
        private readonly IRemotePowerShellExecutor _executor;

        public ProxyMagicPacketSender(IRemotePowerShellExecutor executor)
        {
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        }

        public async Task SendPacketAsync(WakeOnLanRequest wolRequest)
        {
            if (wolRequest == null)
                throw new ArgumentNullException(nameof(wolRequest), "WOL request cannot be null.");

            if (wolRequest.ProxyRunspace == null || wolRequest.ProxyRunspace.RunspaceStateInfo.State != RunspaceState.Opened)
                throw new InvalidOperationException("The proxy runspace is not available or is in a closed state.");

            if (string.IsNullOrWhiteSpace(wolRequest.TargetMacAddress))
                throw new ArgumentException("Target MAC address cannot be null or empty.", nameof(wolRequest.TargetMacAddress));

            // Build the PowerShell script
            var script = BuildPowerShellScript(wolRequest);

            try
            {
                // Execute the script using the provided runspace
                await _executor.ExecuteAsync(wolRequest.ProxyRunspace, script);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send the WOL packet using the proxy runspace.", ex);
            }
        }

        private string BuildPowerShellScript(WakeOnLanRequest wolRequest)
        {
            return $@"
                $UdpClient = New-Object System.Net.Sockets.UdpClient
                $Broadcast = [System.Net.IPAddress]::Broadcast
                $Packet = [byte[]]@({string.Join(",", MagicPacketGenerator.GeneratePacket(wolRequest.TargetMacAddress))})
                $UdpClient.Connect($Broadcast, {wolRequest.Port})
                $UdpClient.Send($Packet, $Packet.Length) | Out-Null
                $UdpClient.Close()
            ";
        }
    }
}
