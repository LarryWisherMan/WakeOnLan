using Microsoft.Extensions.DependencyInjection;
using System;
using System.Management.Automation;
using WakeOnLanCmdlets.Base;
using WakeOnLanLibrary.Application.Interfaces;

/// <summary>
/// <para type="synopsis">Retrieves Wake-on-LAN monitor logs or results.</para>
/// <para type="description">The Get-WakeOnLanData cmdlet retrieves either monitoring logs or Wake-on-LAN results, depending on the specified parameter set. It can optionally clear the retrieved data after retrieval.</para>
/// </summary>
/// <example>
/// <code>Get-WakeOnLanData -MonitorLog</code>
/// <para>Retrieves all monitoring logs.</para>
/// </example>
/// <example>
/// <code>Get-WakeOnLanData -Results</code>
/// <para>Retrieves all Wake-on-LAN results.</para>
/// </example>
/// <example>
/// <code>Get-WakeOnLanData -Results -ClearAfterRetrieval</code>
/// <para>Retrieves all Wake-on-LAN results and clears them from the result cache.</para>
/// </example>
[Cmdlet(VerbsCommon.Get, "WakeOnLanData", DefaultParameterSetName = "MonitorLog")]
public class GetWakeOnLanDataCmdlet : BaseCmdlet
{
    /// <summary>
    /// <para type="description">Retrieves monitoring logs from the monitoring cache.</para>
    /// </summary>
    [Parameter(Mandatory = true, ParameterSetName = "MonitorLog")]
    public SwitchParameter MonitorLog { get; set; }

    /// <summary>
    /// <para type="description">Retrieves Wake-on-LAN results from the result cache.</para>
    /// </summary>
    [Parameter(Mandatory = true, ParameterSetName = "Results")]
    public SwitchParameter Results { get; set; }

    /// <summary>
    /// <para type="description">Clears the retrieved data from the respective cache after retrieval.</para>
    /// </summary>
    [Parameter]
    public SwitchParameter ClearAfterRetrieval { get; set; }

    /// <summary>
    /// Processes the cmdlet logic based on the specified parameter set.
    /// </summary>
    protected override void ProcessRecord()
    {
        base.ProcessRecord();

        if (MonitorLog.IsPresent)
        {
            ProcessMonitorLog();
        }
        else if (Results.IsPresent)
        {
            ProcessResults();
        }
        else
        {
            WriteError(new ErrorRecord(
                new ArgumentException("You must specify either -MonitorLog or -Results."),
                "InvalidParameterSet",
                ErrorCategory.InvalidArgument,
                null));
        }
    }

    /// <summary>
    /// Retrieves monitoring logs and optionally clears them after retrieval.
    /// </summary>
    private void ProcessMonitorLog()
    {
        // Resolve IMonitorCache from the service provider
        var monitorCache = ServiceProvider.GetService<IMonitorCache>();

        if (monitorCache == null)
        {
            WriteError(new ErrorRecord(
                new InvalidOperationException("Unable to resolve IMonitorCache."),
                "MonitorCacheResolutionFailed",
                ErrorCategory.InvalidOperation,
                null));
            return;
        }

        // Retrieve monitor log entries
        var monitorEntries = monitorCache.GetAll();

        // Output monitor entries
        WriteObject(monitorEntries, enumerateCollection: true);

        // Clear monitor cache if the switch is specified
        if (ClearAfterRetrieval.IsPresent)
        {
            monitorCache.Clear();
        }
    }

    /// <summary>
    /// Retrieves Wake-on-LAN results and optionally clears them after retrieval.
    /// </summary>
    private void ProcessResults()
    {
        // Resolve IWakeOnLanResultCache from the service provider
        var resultCache = ServiceProvider.GetService<IWakeOnLanResultCache>();

        if (resultCache == null)
        {
            WriteError(new ErrorRecord(
                new InvalidOperationException("Unable to resolve IWakeOnLanResultCache."),
                "ResultCacheResolutionFailed",
                ErrorCategory.InvalidOperation,
                null));
            return;
        }

        // Retrieve results
        var results = resultCache.GetAll();

        // Output results
        WriteObject(results, enumerateCollection: true);

        // Clear result cache if the switch is specified
        if (ClearAfterRetrieval.IsPresent)
        {
            resultCache.Clear();
        }
    }
}
