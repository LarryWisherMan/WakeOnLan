using System;
using System.Collections.Concurrent;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Core.Interfaces;

namespace WakeOnLanLibrary.Infrastructure.Services
{
    public class RunspaceManager : IRunspaceManager, IDisposable
    {
        private readonly IRunspaceProvider _runspaceProvider;
        private readonly ConcurrentDictionary<string, IRunspacePool> _runspacePools = new();
        private readonly ConcurrentDictionary<string, Runspace> _runspaces = new();
        private bool _disposed;

        public RunspaceManager(IRunspaceProvider runspaceProvider)
        {
            _runspaceProvider = runspaceProvider ?? throw new ArgumentNullException(nameof(runspaceProvider));
        }

        /// <summary>
        /// Gets or creates a shared runspace pool for a specific proxy computer.
        /// </summary>
        public IRunspacePool GetOrCreateRunspacePool(string computerName, PSCredential credentials, int minRunspaces, int maxRunspaces)
        {
            if (string.IsNullOrWhiteSpace(computerName))
                throw new ArgumentNullException(nameof(computerName), "Computer name cannot be null or empty.");

            return _runspacePools.GetOrAdd(computerName, _ =>
            {
                try
                {
                    var connectionInfo = new WSManConnectionInfo
                    {
                        ComputerName = computerName,
                        Credential = credentials,
                        AuthenticationMechanism = AuthenticationMechanism.Default
                    };

                    var pool = _runspaceProvider.CreateRunspacePool(minRunspaces, maxRunspaces, connectionInfo);

                    if (pool.RunspacePoolStateInfo.State == RunspacePoolState.BeforeOpen)
                        pool.Open();
                    else if (pool.RunspacePoolStateInfo.State != RunspacePoolState.Opened)
                        throw new InvalidOperationException($"Cannot use runspace pool. Current state: {pool.RunspacePoolStateInfo.State}");

                    return pool;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to create and open a Runspace Pool for {computerName}: {ex.Message}");
                    throw;
                }
            });
        }


        /// <summary>
        /// Gets or creates a dedicated runspace for a specific computer.
        /// </summary>
        public Runspace GetOrCreateRunspace(string computerName, PSCredential credentials = null)
        {
            if (string.IsNullOrWhiteSpace(computerName))
                throw new ArgumentNullException(nameof(computerName), "Computer name cannot be null or empty.");

            return _runspaces.GetOrAdd(computerName, _ =>
            {
                try
                {
                    var connectionInfo = new WSManConnectionInfo
                    {
                        ComputerName = computerName,
                        Credential = credentials,
                        AuthenticationMechanism = AuthenticationMechanism.Default
                    };

                    var runspace = _runspaceProvider.CreateRunspace(connectionInfo);
                    runspace.Open();
                    return runspace;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating runspace for {computerName}: {ex.Message}");
                    throw;
                }
            });
        }

        /// <summary>
        /// Closes and disposes a specific runspace pool.
        /// </summary>
        public void CloseRunspacePool(string computerName)
        {
            if (_runspacePools.TryRemove(computerName, out var pool))
            {
                try
                {
                    if (pool.RunspacePoolStateInfo.State == RunspacePoolState.Opened)
                        pool.Close();

                    pool.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error closing runspace pool for {computerName}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Closes and disposes a specific runspace.
        /// </summary>
        public void CloseRunspace(string computerName)
        {
            if (_runspaces.TryRemove(computerName, out var runspace))
            {
                try
                {
                    if (runspace.RunspaceStateInfo.State == RunspaceState.Opened)
                        runspace.Close();

                    runspace.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error closing runspace for {computerName}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Closes all managed runspaces and runspace pools.
        /// </summary>
        public void CloseAll()
        {
            foreach (var key in _runspaces.Keys)
            {
                CloseRunspace(key);
            }

            foreach (var key in _runspacePools.Keys)
            {
                CloseRunspacePool(key);
            }
        }

        /// <summary>
        /// Disposes of all resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            CloseAll();
            _disposed = true;
        }
    }
}
