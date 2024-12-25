using System;
using System.Management.Automation.Runspaces;
using WakeOnLanLibrary.Interfaces;
using WakeOnLanLibrary.Models;

namespace WakeOnLanLibrary.Services
{
    public class ProxyMagicPacketSender : IMagicPacketSender
    {
        private readonly string _proxyIp;

        public ProxyMagicPacketSender(string proxyIp)
        {
            _proxyIp = proxyIp ?? throw new ArgumentNullException(nameof(proxyIp), "Proxy IP cannot be null.");
        }

        public void SendPacket(MagicPacket packet)
        {
            if (packet == null)
            {
                throw new ArgumentNullException(nameof(packet), "MagicPacket cannot be null.");
            }

            // Build the PowerShell script
            string script = $@"
                $UdpClient = New-Object System.Net.Sockets.UdpClient
                $Broadcast = [System.Net.IPAddress]::Broadcast
                $Packet = [byte[]]@({string.Join(",", packet.PacketBytes)}) # Convert byte array to PowerShell array
                $Ports = @({packet.Port}) # Ports array for flexibility
                $Ports | ForEach-Object {{
                    $UdpClient.Connect($Broadcast, $_)
                    $UdpClient.Send($Packet, $Packet.Length) | Out-Null
                }}
                $UdpClient.Close()
            ";

            // Execute the script on the proxy computer
            ExecuteRemoteCommand(_proxyIp, script);
        }

        private void ExecuteRemoteCommand(string proxyIp, string script)
        {
            var connectionInfo = new WSManConnectionInfo
            {
                ComputerName = proxyIp,
                AuthenticationMechanism = AuthenticationMechanism.Default,

            };

            var runspace = RunspaceFactory.CreateRunspace(connectionInfo);
            runspace.Open();

            var pipeline = runspace.CreatePipeline();
            pipeline.Commands.AddScript(script);

            try
            {
                pipeline.Invoke();
                if (pipeline.HadErrors)
                {
                    throw new InvalidOperationException("Error occurred during remote execution.");
                }
            }
            finally
            {
                runspace.Close();
            }
        }
    }
}
