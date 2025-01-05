using System.Threading.Tasks;
using WakeOnLanLibrary.Core.Interfaces;

namespace WakeOnLanLibrary.Application.Interfaces
{
    public interface IRemotePowerShellExecutor
    {
        /// <summary>
        /// Executes a PowerShell script asynchronously using a RunspacePool.
        /// </summary>
        Task ExecuteAsync(IRunspacePool runspacePool, string script);
    }
}

