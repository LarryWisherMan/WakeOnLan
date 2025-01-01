using System.Management.Automation;
using WakeOnLanCmdlets.Base;

[Cmdlet(VerbsCommon.Get, "WakeOnLanMonitorLog")]
public class GetWakeOnLanMonitorLogCmdlet : BaseCmdlet
{
    [Parameter]
    public SwitchParameter ClearAfterRetrieval { get; set; }

    protected override void ProcessRecord()
    {
        base.ProcessRecord();

        // Retrieve monitor log entries from the PersistentWolService's MonitorCache
        var monitorEntries = WolService.MonitorCache.GetAll();

        // Output monitor entries
        WriteObject(monitorEntries, enumerateCollection: true);

        // Clear monitor cache if the switch is specified
        if (ClearAfterRetrieval.IsPresent)
        {
            WolService.MonitorCache.Clear();
        }
    }
}
