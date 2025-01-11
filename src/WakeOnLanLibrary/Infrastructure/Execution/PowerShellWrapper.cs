using System;
using System.Collections.Generic;
using System.Management.Automation;
using WakeOnLanLibrary.Application.Interfaces.Execution;

namespace WakeOnLanLibrary.Infrastructure.Execution
{
    public class PowerShellWrapper : IPowerShell
    {
        private readonly PowerShell _powerShell;

        public PowerShellWrapper(PowerShell powerShell)
        {
            _powerShell = powerShell;
        }

        public PowerShellWrapper()
        {
            try
            {
                _powerShell = PowerShell.Create();
                if (_powerShell == null)
                {
                    throw new InvalidOperationException("PowerShell.Create() returned null. Ensure PowerShell is installed and accessible.");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to create PowerShell instance.", ex);
            }
        }

        public void AddScript(string script)
        {
            if (string.IsNullOrWhiteSpace(script))
                throw new ArgumentNullException(nameof(script), "Script cannot be null or empty.");

            _powerShell.AddScript(script);
        }

        public ICollection<PSObject> Invoke()
        {
            return _powerShell.Invoke();
        }

        public bool HadErrors => _powerShell.HadErrors;

        public IEnumerable<ErrorRecord> Errors => _powerShell.Streams.Error.ReadAll();

        public void Dispose()
        {
            _powerShell?.Dispose();
        }
    }

}
