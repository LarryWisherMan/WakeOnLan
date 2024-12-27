using System.Management.Automation;
using WakeOnLanCmdlets.Base;

[Cmdlet(VerbsCommon.Get, "WakeOnLanResults")]
public class GetWakeOnLanResultsCmdlet : BaseCmdlet
{
    [Parameter]
    public SwitchParameter ClearAfterRetrieval { get; set; }

    protected override void ProcessRecord()
    {
        base.ProcessRecord();

        // Retrieve results from the PersistentWolService's ResultCache
        var results = WolService.ResultCache.GetAll();

        // Output results
        WriteObject(results, enumerateCollection: true);

        // Clear results if the switch is specified
        if (ClearAfterRetrieval.IsPresent)
        {
            WolService.ResultCache.Clear();
        }
    }
}
