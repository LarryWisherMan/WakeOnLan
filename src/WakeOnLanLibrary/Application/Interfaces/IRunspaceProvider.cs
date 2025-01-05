using System.Management.Automation.Runspaces;
using WakeOnLanLibrary.Core.Interfaces;

namespace WakeOnLanLibrary.Application.Interfaces
{
    public interface IRunspaceProvider
    {
        /// <summary>
        /// Creates a new Runspace connected to a remote computer.
        /// </summary>
        Runspace CreateRunspace(WSManConnectionInfo connectionInfo);

        /// <summary>
        /// Creates a Runspace Pool for managing multiple concurrent remote connections.
        /// </summary>
        IRunspacePool CreateRunspacePool(int minRunspaces, int maxRunspaces, WSManConnectionInfo connectionInfo = null);
    }
}
