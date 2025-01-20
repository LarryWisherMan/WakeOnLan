using System;

using System.Management.Automation.Runspaces;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Core.Interfaces;
using WakeOnLanLibrary.Infrastructure.Adapters;
using WakeOnLanLibrary.Infrastructure.Runspaces;

namespace WakeOnLanLibrary.Infrastructure.Factories
{
    public class RunspaceProvider : IRunspaceProvider
    {
        /// <summary>
        /// Creates a new Runspace connected to a remote computer.
        /// </summary>
        /// <param name="connectionInfo">The connection info for the remote computer.</param>
        /// <returns>An instance of <see cref="IRunspace"/> that wraps the created runspace.</returns>
        public IRunspace CreateRunspace(WSManConnectionInfo connectionInfo)
        {
            if (connectionInfo == null)
                throw new ArgumentNullException(nameof(connectionInfo), "Connection info cannot be null.");

            try
            {
                // Create the Runspace using the PowerShell SDK
                var runspace = RunspaceFactory.CreateRunspace(connectionInfo);

                // Open the Runspace to establish the connection
                runspace.Open();

                // Return a wrapper that implements IRunspace
                return new RunspaceWrapper(runspace);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create and open a Runspace.", ex);
            }
        }

        /// <summary>
        /// Creates a Runspace Pool for managing multiple concurrent remote connections.
        /// </summary>
        public IRunspacePool CreateRunspacePool(int minRunspaces, int maxRunspaces, WSManConnectionInfo connectionInfo = null)
        {
            if (minRunspaces <= 0 || maxRunspaces <= 0 || minRunspaces > maxRunspaces)
                throw new ArgumentException("Invalid Runspace Pool configuration: ensure minRunspaces > 0, maxRunspaces > 0, and minRunspaces <= maxRunspaces.");

            try
            {
                RunspacePool runspacePool;

                if (connectionInfo == null)
                {
                    runspacePool = RunspaceFactory.CreateRunspacePool(minRunspaces, maxRunspaces);
                }
                else
                {
                    runspacePool = RunspaceFactory.CreateRunspacePool(minRunspaces, maxRunspaces, connectionInfo);
                }

                runspacePool.Open();
                return new RunspacePoolWrapper(runspacePool);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create and open a Runspace Pool.", ex);
            }
        }
    }
}
