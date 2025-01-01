using System.Management.Automation.Runspaces;
using System.Threading.Tasks;

namespace WakeOnLanLibrary.Application.Interfaces
{
    public interface IRemotePowerShellExecutor
    {
        /// <summary>
        /// Executes a PowerShell script asynchronously using a RunspacePool.
        /// </summary>
        Task ExecuteAsync(RunspacePool runspacePool, string script);
    }
}

