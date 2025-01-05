using System;
using System.Management.Automation.Runspaces;
using WakeOnLanLibrary.Core.Interfaces; // Namespace for IRunspacePool
using WakeOnLanLibrary.Infrastructure.Runspaces;

namespace WakeOnLanLibrary.Core.Extensions
{
    public static class RunspacePoolExtensions
    {
        /// <summary>
        /// Extracts the internal RunspacePool from an IRunspacePool implementation.
        /// </summary>
        public static RunspacePool GetInternalRunspacePool(this IRunspacePool runspacePool)
        {
            if (runspacePool is RunspacePoolWrapper wrapper)
            {
                return wrapper.RunspacePool;
            }

            throw new InvalidOperationException("The provided IRunspacePool is not a valid RunspacePoolWrapper.");
        }
    }
}
