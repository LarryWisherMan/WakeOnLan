using System;
using System.Collections.Concurrent;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using WakeOnLanLibrary.Interfaces;

namespace WakeOnLanLibrary.Services
{
    public class RunspaceManager : IRunspaceManager
    {
        private readonly ConcurrentDictionary<string, Runspace> _runspaces = new();

        public Runspace GetOrCreateRunspace(string computerName, PSCredential credentials = null)
        {
            if (string.IsNullOrWhiteSpace(computerName))
                throw new ArgumentNullException(nameof(computerName), "Computer name cannot be null or empty.");

            return _runspaces.GetOrAdd(computerName, _ =>
            {
                var connectionInfo = new WSManConnectionInfo
                {
                    ComputerName = computerName,
                    AuthenticationMechanism = AuthenticationMechanism.Default,
                    Credential = credentials
                };

                var runspace = RunspaceFactory.CreateRunspace(connectionInfo);
                runspace.Open();
                return runspace;
            });
        }

        public void CloseRunspace(string computerName)
        {
            if (_runspaces.TryRemove(computerName, out var runspace))
            {
                if (runspace.RunspaceStateInfo.State == RunspaceState.Opened)
                {
                    runspace.Close();
                }
                runspace.Dispose();
            }
        }

        public void CloseAllRunspaces()
        {
            foreach (var key in _runspaces.Keys)
            {
                CloseRunspace(key);
            }
        }
    }
}
