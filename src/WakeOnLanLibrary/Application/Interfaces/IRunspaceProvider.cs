using System.Management.Automation.Runspaces;
using WakeOnLanLibrary.Core.Interfaces;

namespace WakeOnLanLibrary.Application.Interfaces
{
    public interface IRunspaceProvider
    {
        /// <summary>
        /// Creates a new Runspace connected to a remote computer.
        /// </summary>
        /// <param name="connectionInfo">The connection info for the remote computer.</param>
        /// <returns>An instance of <see cref="IRunspace"/> that wraps the created runspace.</returns>
        IRunspace CreateRunspace(WSManConnectionInfo connectionInfo);

        /// <summary>
        /// Creates a Runspace Pool for managing multiple concurrent remote connections.
        /// </summary>
        /// <param name="minRunspaces">The minimum number of runspaces in the pool.</param>
        /// <param name="maxRunspaces">The maximum number of runspaces in the pool.</param>
        /// <param name="connectionInfo">Optional connection info for the pool.</param>
        /// <returns>An instance of <see cref="IRunspacePool"/> for managing multiple remote connections.</returns>
        IRunspacePool CreateRunspacePool(int minRunspaces, int maxRunspaces, WSManConnectionInfo connectionInfo = null);
    }
}
