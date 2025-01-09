using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;

namespace WakeOnLanLibrary.Application.Interfaces.Execution
{
    public interface IPowerShellExecutor
    {
        void AddScript(string script);
        Task<ICollection<PSObject>> InvokeAsync();
        bool HadErrors { get; }
        IEnumerable<ErrorRecord> Errors { get; }
    }
}

