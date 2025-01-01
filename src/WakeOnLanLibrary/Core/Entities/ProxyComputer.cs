using System.Management.Automation.Runspaces;

namespace WakeOnLanLibrary.Core.Entities
{
    public class ProxyComputer : Computer
    {
        public bool RelayCapability { get; set; } = true;
        public int RelayPort { get; set; } = 9;
        public string Role { get; set; } = "WakeOnLanRelay";

        /// <summary>
        /// Associated PowerShell runspace for executing remote commands.
        /// </summary>
        public Runspace Runspace { get; set; }

        /// <summary>
        /// Checks if the associated runspace is available and open.
        /// </summary>
        public bool IsRunspaceAvailable => Runspace?.RunspaceStateInfo.State == RunspaceState.Opened;

        /// <summary>
        /// Closes and disposes the associated runspace.
        /// </summary>
        public void CloseRunspace()
        {
            if (Runspace != null && Runspace.RunspaceStateInfo.State == RunspaceState.Opened)
            {
                Runspace.Close();
                Runspace.Dispose();
                Runspace = null;
            }
        }
    }
}


