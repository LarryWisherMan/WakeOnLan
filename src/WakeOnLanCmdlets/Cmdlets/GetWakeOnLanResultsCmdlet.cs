using Microsoft.Extensions.DependencyInjection;
using System;
using System.Management.Automation;
using WakeOnLanCmdlets.Base;
using WakeOnLanLibrary.Application.Interfaces;

[Cmdlet(VerbsCommon.Get, "WakeOnLanResults")]
public class GetWakeOnLanResultsCmdlet : BaseCmdlet
{
    [Parameter]
    public SwitchParameter ClearAfterRetrieval { get; set; }

    protected override void ProcessRecord()
    {
        base.ProcessRecord();

        // Resolve IResultManager from the service provider
        var resultManager = ServiceProvider.GetService<IWakeOnLanResultCache>();

        if (resultManager == null)
        {
            WriteError(new ErrorRecord(
                new InvalidOperationException("Unable to resolve IResultManager."),
                "ResultManagerResolutionFailed",
                ErrorCategory.InvalidOperation,
                null));
            return;
        }

        // Retrieve results
        var results = resultManager.GetAll();

        // Output results
        WriteObject(results, enumerateCollection: true);

        // Clear results if the switch is specified
        if (ClearAfterRetrieval.IsPresent)
        {
            resultManager.Clear();
        }
    }
}

