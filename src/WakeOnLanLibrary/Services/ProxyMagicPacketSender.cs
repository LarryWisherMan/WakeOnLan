using System;
using System.Collections.Generic;
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

            if (string.IsNullOrWhiteSpace(wolRequest.TargetMacAddress))
                throw new ArgumentException("Target MAC address cannot be null or empty.", nameof(wolRequest.TargetMacAddress));

            // Build and execute the script for a single request
            var script = BuildPowerShellScript(new List<WakeOnLanRequest> { wolRequest });
            await _executor.ExecuteAsync(wolRequest.ProxyRunspace, script);
        }

        public async Task SendPacketAsync(IEnumerable<WakeOnLanRequest> wolRequests)
        {
            if (wolRequests == null)
                throw new ArgumentNullException(nameof(wolRequests), "WOL requests cannot be null.");

            foreach (var wolRequest in wolRequests)
            {
                if (string.IsNullOrWhiteSpace(wolRequest.TargetMacAddress))
                    throw new ArgumentException("Target MAC address cannot be null or empty.", nameof(wolRequest.TargetMacAddress));
            }

            // Group by runspace and process requests in batches
            var groupedRequests = GroupRequestsByProxyRunspace(wolRequests);
            foreach (var group in groupedRequests)
            {
                var script = BuildPowerShellScript(group.Value);
                await _executor.ExecuteAsync(group.Key, script);
            }
        }

        private Dictionary<Runspace, List<WakeOnLanRequest>> GroupRequestsByProxyRunspace(IEnumerable<WakeOnLanRequest> wolRequests)
        {
            var groupedRequests = new Dictionary<Runspace, List<WakeOnLanRequest>>();

            foreach (var request in wolRequests)
            {
                if (!groupedRequests.ContainsKey(request.ProxyRunspace))
                {
                    groupedRequests[request.ProxyRunspace] = new List<WakeOnLanRequest>();
                }

                groupedRequests[request.ProxyRunspace].Add(request);
            }

            return groupedRequests;
        }

        private string BuildPowerShellScript(List<WakeOnLanRequest> requests)
        {
            var script = @"
            $UdpClient = New-Object System.Net.Sockets.UdpClient
            $Broadcast = [System.Net.IPAddress]::Broadcast
        ";

            foreach (var request in requests)
            {
                var packetBytes = MagicPacketGenerator.GeneratePacket(request.TargetMacAddress);
                script += $@"
                $Packet = [byte[]]@({string.Join(",", packetBytes)})
                $UdpClient.Connect($Broadcast, {request.Port})
                $UdpClient.Send($Packet, $Packet.Length) | Out-Null
            ";
            }

            script += "$UdpClient.Close()";
            return script;
        }
    }
}

