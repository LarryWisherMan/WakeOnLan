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
        private readonly PowerShell _powerShell;
        private readonly ConcurrentBag<PowerShellError> _errorCollection;

        public PowerShellExecutor()
        {
            _powerShell = PowerShell.Create();
            _errorCollection = new ConcurrentBag<PowerShellError>();

            // Subscribe to the error stream
            _powerShell.Streams.Error.DataAdded += (sender, e) =>
            {
                var errorRecord = ((PSDataCollection<ErrorRecord>)sender)[e.Index];
                _errorCollection.Add(ConvertToPowerShellError(errorRecord));
            };
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
                var results = await Task.Run(() => _powerShell.Invoke());

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
