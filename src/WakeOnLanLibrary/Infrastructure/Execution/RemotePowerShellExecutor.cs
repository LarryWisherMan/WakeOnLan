using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Application.Interfaces.Execution;
using WakeOnLanLibrary.Core.Interfaces;

namespace WakeOnLanLibrary.Infrastructure.Execution
{
    public class RemotePowerShellExecutor : IRemotePowerShellExecutor
    {
        private readonly Func<IPowerShellExecutor> _powerShellExecutorFactory;

        public RemotePowerShellExecutor(Func<IPowerShellExecutor> powerShellExecutorFactory)
        {
            _powerShellExecutorFactory = powerShellExecutorFactory ?? throw new ArgumentNullException(nameof(powerShellExecutorFactory));
        }

        public async Task ExecuteAsync(IRunspacePool runspacePool, string script)
        {
            if (runspacePool == null)
                throw new ArgumentNullException(nameof(runspacePool), "RunspacePool cannot be null.");

            if (string.IsNullOrWhiteSpace(script))
                throw new ArgumentNullException(nameof(script), "Script cannot be null or empty.");

            if (runspacePool.GetRunspaceState() != RunspacePoolState.Opened)
                throw new InvalidOperationException("RunspacePool is not in an open state.");

            var powerShellExecutor = _powerShellExecutorFactory();
            powerShellExecutor.AddScript(script);

            try
            {
                var results = await powerShellExecutor.InvokeAsync();

                if (powerShellExecutor.HadErrors)
                {
                    var errorMessages = powerShellExecutor.Errors
                        .Select(error => $"{error.Message} (ID: {error.ErrorId}, Target: {error.TargetObject})")
                        .ToList();

                    throw new AggregateException("Errors occurred during script execution.",
                        errorMessages.Select(msg => new InvalidOperationException(msg)).ToArray());
                }
            }
            catch (AggregateException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while executing the PowerShell script.", ex);
            }
        }


        private string ExtractErrorMessage(ErrorRecord errorRecord)
        {
            return $"{errorRecord.Exception?.Message ?? "Unknown exception"} " +
                   $"(ID: {errorRecord.FullyQualifiedErrorId}, Target: {errorRecord.TargetObject})";
        }
    }
}
