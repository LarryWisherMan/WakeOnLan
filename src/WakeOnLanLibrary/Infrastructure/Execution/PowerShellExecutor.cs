using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using WakeOnLanLibrary.Application.Common;
using WakeOnLanLibrary.Application.Interfaces.Execution;

namespace WakeOnLanLibrary.Infrastructure.Execution
{
    public class PowerShellExecutor : IPowerShellExecutor, IDisposable
    {
        private readonly IPowerShell _powerShell;
        private readonly ConcurrentBag<PowerShellError> _errorCollection;

        public PowerShellExecutor(IPowerShell powerShell)
        {
            _powerShell = powerShell ?? throw new ArgumentNullException(nameof(powerShell));
            _errorCollection = new ConcurrentBag<PowerShellError>();
        }

        public void AddScript(string script)
        {
            _powerShell.AddScript(script);
        }

        public async Task<ICollection<PSObject>> InvokeAsync()
        {
            try
            {
                var results = await Task.Run(() => _powerShell.Invoke());

                foreach (var error in _powerShell.Errors)
                {
                    _errorCollection.Add(ConvertToPowerShellError(error));
                }

                return results;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while executing the PowerShell script.", ex);
            }
        }

        public bool HadErrors => _errorCollection.Count > 0;

        public IEnumerable<PowerShellError> Errors => _errorCollection;

        public void Dispose()
        {
            _powerShell?.Dispose();
        }

        private PowerShellError ConvertToPowerShellError(ErrorRecord errorRecord)
        {
            return new PowerShellError(
                message: errorRecord.Exception?.Message ?? "Unknown error",
                errorId: errorRecord.FullyQualifiedErrorId,
                targetObject: errorRecord.TargetObject?.ToString() ?? "Unknown target"
            );
        }
    }
}
