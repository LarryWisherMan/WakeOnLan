using Microsoft.Extensions.DependencyInjection;
using System;
using System.Management.Automation;
using WakeOnLanCmdlets.Base;
using WakeOnLanLibrary.Application.Interfaces;

[Cmdlet(VerbsCommon.Get, "WakeOnLanMonitorLog")]
public class GetWakeOnLanMonitorLogCmdlet : BaseCmdlet
{
    [Parameter]
    public SwitchParameter ClearAfterRetrieval { get; set; }

    protected override void ProcessRecord()
    {
        base.ProcessRecord();

        // Resolve IResultManager from the service provider
        var resultManager = ServiceProvider.GetService<IMonitorCache>();

        if (resultManager == null)
        {
            WriteError(new ErrorRecord(
                new InvalidOperationException("Unable to resolve IResultManager."),
                "ResultManagerResolutionFailed",
                ErrorCategory.InvalidOperation,
                null));
            return;
        }

        // Retrieve monitor log entries
        var monitorEntries = resultManager.GetAll();

        // Output monitor entries
        WriteObject(monitorEntries, enumerateCollection: true);

        // Clear monitor cache if the switch is specified
        if (ClearAfterRetrieval.IsPresent)
        {
            resultManager.Clear();
        }
    }
}
