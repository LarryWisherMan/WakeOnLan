using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using WakeOnLanLibrary.Interfaces;

namespace WakeOnLanLibrary.Services
{
    public class RemotePowerShellExecutor : IRemotePowerShellExecutor
    {
        public Runspace Connect(string computerName, PSCredential credentials = null)
        {
            if (string.IsNullOrWhiteSpace(computerName))
                throw new ArgumentNullException(nameof(computerName), "Computer name cannot be null or empty.");

            var connectionInfo = new WSManConnectionInfo
            {
                ComputerName = computerName,
                AuthenticationMechanism = AuthenticationMechanism.Default,
                Credential = credentials
            };

            var runspace = RunspaceFactory.CreateRunspace(connectionInfo);
            runspace.Open();

            return runspace;
        }

        public async Task ExecuteAsync(Runspace runspace, string script)
        {
            if (runspace == null || runspace.RunspaceStateInfo.State != RunspaceState.Opened)
                throw new InvalidOperationException("Runspace is not in an open state.");

            if (string.IsNullOrWhiteSpace(script))
                throw new ArgumentNullException(nameof(script), "Script cannot be null or empty.");

            var pipeline = runspace.CreatePipeline();
            pipeline.Commands.AddScript(script);

            var results = await Task.Run(() => pipeline.Invoke());

            if (pipeline.HadErrors)
            {
                var error = string.Join(Environment.NewLine, pipeline.Error.ReadToEnd());
                throw new InvalidOperationException($"Errors occurred during script execution: {error}");
            }
        }

        public void Disconnect(Runspace runspace)
        {
            if (runspace == null || runspace.RunspaceStateInfo.State != RunspaceState.Opened)
                return;

            runspace.Close();
            runspace.Dispose();
        }
    }

}
