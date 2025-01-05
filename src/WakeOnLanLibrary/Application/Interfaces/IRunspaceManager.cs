using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using WakeOnLanLibrary.Core.Interfaces;

namespace WakeOnLanLibrary.Application.Interfaces
{
    public interface IRunspaceManager : IDisposable
    {
        /// <summary>
        /// Gets or creates a dedicated runspace for a specific computer.
        /// </summary>
        /// <param name="computerName">The name of the computer.</param>
        /// <param name="credentials">Optional credentials for connecting to the computer.</param>
        /// <returns>A <see cref="Runspace"/> instance.</returns>
        Runspace GetOrCreateRunspace(string computerName, PSCredential credentials = null);

        /// <summary>
        /// Gets or creates a shared runspace pool for a specific proxy computer.
        /// </summary>
        /// <param name="computerName">The name of the proxy computer.</param>
        /// <param name="credentials">Optional credentials for connecting to the computer.</param>
        /// <param name="minRunspaces">The minimum number of runspaces in the pool.</param>
        /// <param name="maxRunspaces">The maximum number of runspaces in the pool.</param>
        /// <returns>A <see cref="RunspacePool"/> instance.</returns>
        IRunspacePool GetOrCreateRunspacePool(string computerName, PSCredential credentials, int minRunspaces, int maxRunspaces);

        /// <summary>
        /// Closes and disposes a specific runspace for a computer.
        /// </summary>
        /// <param name="computerName">The name of the computer.</param>
        void CloseRunspace(string computerName);

        /// <summary>
        /// Closes all managed runspaces and runspace pools.
        /// </summary>
        void CloseAll();
    }
}
