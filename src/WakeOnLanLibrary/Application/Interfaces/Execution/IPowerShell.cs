using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace WakeOnLanLibrary.Application.Interfaces.Execution
{
    public interface IPowerShell : IDisposable
    {
        void AddScript(string script);
        ICollection<PSObject> Invoke();
        bool HadErrors { get; }
        IEnumerable<ErrorRecord> Errors { get; }
    }
}
