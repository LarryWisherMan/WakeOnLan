using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Core.Extensions;
using WakeOnLanLibrary.Core.Interfaces;

namespace WakeOnLanLibrary.Infrastructure.Services
{
    public class RemotePowerShellExecutor : IRemotePowerShellExecutor
    {
        /// <summary>
        /// Executes a PowerShell script asynchronously using a RunspacePool.
        /// </summary>
        public async Task ExecuteAsync(IRunspacePool runspacePool, string script)
        {
            if (runspacePool == null)
                throw new ArgumentNullException(nameof(runspacePool), "RunspacePool cannot be null.");

            if (string.IsNullOrWhiteSpace(script))
                throw new ArgumentNullException(nameof(script), "Script cannot be null or empty.");

            // Validate that the RunspacePool is open
            if (runspacePool.RunspacePoolStateInfo.State != RunspacePoolState.Opened)
                throw new InvalidOperationException("RunspacePool is not in an open state.");

            // Create a new PowerShell instance
            using var powerShell = PowerShell.Create();
            powerShell.RunspacePool = runspacePool.GetInternalRunspacePool();
            powerShell.AddScript(script);

            try
            {
                // Execute the script asynchronously
                var results = await Task.Run(() => powerShell.Invoke());

                // Check for errors
                if (powerShell.HadErrors)
                {
                    var errorDetails = string.Join(Environment.NewLine, powerShell.Streams.Error.ReadAll());
                    throw new InvalidOperationException($"Errors occurred during script execution: {errorDetails}");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while executing the PowerShell script.", ex);
            }
        }
        /// <summary>
        /// Executes a PowerShell script asynchronously using a RunspacePool and returns the results.
        /// </summary>
        public async Task<IEnumerable<PSObject>> ExecuteWithResultsAsync(IRunspacePool runspacePool, string script)
        {
            if (runspacePool == null)
                throw new ArgumentNullException(nameof(runspacePool), "RunspacePool cannot be null.");

            if (string.IsNullOrWhiteSpace(script))
                throw new ArgumentNullException(nameof(script), "Script cannot be null or empty.");

            // Validate that the RunspacePool is open
            if (runspacePool.RunspacePoolStateInfo.State != RunspacePoolState.Opened)
                throw new InvalidOperationException("RunspacePool is not in an open state.");

            // Create a new PowerShell instance
            using var powerShell = PowerShell.Create();
            powerShell.RunspacePool = runspacePool.GetInternalRunspacePool();
            powerShell.AddScript(script);

            try
            {
                // Execute the script asynchronously
                var results = await Task.Run(() => powerShell.Invoke());

                // Check for errors
                if (powerShell.HadErrors)
                {
                    var errorDetails = string.Join(Environment.NewLine, powerShell.Streams.Error.ReadAll());
                    throw new InvalidOperationException($"Errors occurred during script execution: {errorDetails}");
                }

                return results; // Return the script results
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while executing the PowerShell script.", ex);
            }
        }

    }
}
