using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;

namespace WakeOnLanLibrary.Interfaces
{
    public interface IRemotePowerShellExecutor
    {
        /// <summary>
        /// Establishes a connection to a remote computer and opens a PowerShell runspace.
        /// </summary>
        /// <param name="computerName">The name or IP address of the remote computer.</param>
        /// <param name="credentials">Optional credentials for authentication.</param>
        /// <returns>An open PowerShell Runspace connected to the remote computer.</returns>
        Runspace Connect(string computerName, PSCredential credentials = null);

        /// <summary>
        /// Executes a PowerShell script or command on an existing runspace.
        /// </summary>
        /// <param name="runspace">An open PowerShell Runspace to execute the command.</param>
        /// <param name="script">The PowerShell script or command to execute.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task ExecuteAsync(Runspace runspace, string script);

        /// <summary>
        /// Closes and disposes of a PowerShell runspace.
        /// </summary>
        /// <param name="runspace">The runspace to close.</param>
        void Disconnect(Runspace runspace);
    }

}
