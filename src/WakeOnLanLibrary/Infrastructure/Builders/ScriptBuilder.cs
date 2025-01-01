using System.Collections.Generic;
using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Infrastructure.Services;

namespace WakeOnLanLibrary.Infrastructure.Builders
{
    public class ScriptBuilder : IScriptBuilder
    {
        public string BuildScript(IEnumerable<WakeOnLanRequest> requests)
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
