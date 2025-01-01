using System;

using System.Management.Automation.Runspaces;
using WakeOnLanLibrary.Application.Interfaces;

namespace WakeOnLanLibrary.Infrastructure.Factories
{
    public class RunspaceProvider : IRunspaceProvider
    {
        /// <summary>
        /// Creates a new Runspace connected to a remote computer.
        /// </summary>
        public Runspace CreateRunspace(WSManConnectionInfo connectionInfo)
        {
            if (connectionInfo == null)
                throw new ArgumentNullException(nameof(connectionInfo), "Connection info cannot be null.");

            try
            {
                var runspace = RunspaceFactory.CreateRunspace(connectionInfo);
                runspace.Open();
                return runspace;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create and open a Runspace.", ex);
            }
        }

        /// <summary>
        /// Creates a Runspace Pool for managing multiple concurrent remote connections.
        /// </summary>
        public RunspacePool CreateRunspacePool(int minRunspaces, int maxRunspaces, WSManConnectionInfo connectionInfo = null)
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
                return runspacePool;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create and open a Runspace Pool.", ex);
            }
        }
    }
}
