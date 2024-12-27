using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace WakeOnLanLibrary.Interfaces
{
    public interface IRunspaceManager
    {
        Runspace GetOrCreateRunspace(string computerName, PSCredential credentials = null);
        void CloseRunspace(string computerName);
        void CloseAllRunspaces();
    }

}
