using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using WakeOnLanLibrary.Application.Interfaces.Execution;

namespace WakeOnLanLibrary.Infrastructure.Execution
{
    public class PowerShellExecutor : IPowerShellExecutor, IDisposable
    {
        private readonly PowerShell _powerShell;

        public PowerShellExecutor()
        {
            _powerShell = PowerShell.Create();
        }

        public void AddScript(string script)
        {
            if (string.IsNullOrWhiteSpace(script))
                throw new ArgumentNullException(nameof(script), "Script cannot be null or empty.");

            _powerShell.AddScript(script);
        }

        public async Task<ICollection<PSObject>> InvokeAsync()
        {
            try
            {
                return await Task.Run(() => _powerShell.Invoke());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while executing the PowerShell script.", ex);
            }
        }

        public bool HadErrors => _powerShell.HadErrors;

        public IEnumerable<ErrorRecord> Errors => _powerShell.Streams.Error.ReadAll();

        public void Dispose()
        {
            _powerShell?.Dispose();
        }
    }
}