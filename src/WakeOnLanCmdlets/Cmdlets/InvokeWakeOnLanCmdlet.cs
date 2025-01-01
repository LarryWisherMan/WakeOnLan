using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using WakeOnLanCmdlets.Base;
using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Infrastructure.Factories;

[Cmdlet(VerbsLifecycle.Invoke, "WakeOnLan", DefaultParameterSetName = "SingleTarget")]
public class InvokeWakeOnLanCmdlet : BaseCmdlet
{
    // Parameters
    [Parameter(Mandatory = true, ParameterSetName = "SingleTarget")]
    public string MacAddress { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = "SingleTarget")]
    public string ComputerName { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = "SingleTarget")]
    public string ProxyComputerName { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = "MultipleTargets")]
    public Dictionary<string, List<(string MacAddress, string ComputerName)>> ProxyToTargets { get; set; }

    [Parameter(Mandatory = true, ParameterSetName = "HashtableTargets")]
    public Hashtable HashtableTargets { get; set; }

    [Parameter]
    public int Port { get; set; } = 9;

    [Parameter]
    public PSCredential Credential { get; set; }

    [Parameter]
    public int MaxPingAttempts { get; set; } = 5;

    [Parameter]
    public int TimeoutInSeconds { get; set; } = 60;

    [Parameter]
    public SwitchParameter Async { get; set; }

    [Parameter]
    public SwitchParameter WriteHost { get; set; }

    protected override void ProcessRecord()
    {
        base.ProcessRecord();

        // Determine targets
        var targets = GetTargets();

        if (Async.IsPresent)
        {
            ExecuteAsync(targets);
            WriteVerbose("The Wake-on-LAN operation is running asynchronously in the background.");
        }
        else
        {
            ExecuteSynchronously(targets);
        }
    }

    private Dictionary<string, List<(string MacAddress, string ComputerName)>> GetTargets()
    {
        return ParameterSetName switch
        {
            "SingleTarget" => new Dictionary<string, List<(string, string)>>
            {
                { ProxyComputerName, new List<(string, string)> { (MacAddress, ComputerName) } }
            },
            "MultipleTargets" => ProxyToTargets,
            "HashtableTargets" => TargetMappingFactory.CreateProxyToTargetsMapping(HashtableTargets),
            _ => throw new PSArgumentException("Invalid parameter set.")
        };
    }

    private void ExecuteAsync(Dictionary<string, List<(string MacAddress, string ComputerName)>> targets)
    {
        Task.Run(async () =>
        {
            try
            {
                var results = await WolService.WakeUpAndMonitorAsync(
                    proxyToTargets: targets,
                    port: Port,
                    credential: Credential,
                    maxPingAttempts: MaxPingAttempts,
                    timeoutInSeconds: TimeoutInSeconds);

                foreach (var result in results)
                {
                    OutputResult(result);
                }
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "AsyncOperationFailed", ErrorCategory.OperationStopped, targets));
            }
        });
    }

    private void ExecuteSynchronously(Dictionary<string, List<(string MacAddress, string ComputerName)>> targets)
    {
        try
        {
            var results = WolService.WakeUpAndMonitorAsync(
                proxyToTargets: targets,
                port: Port,
                credential: Credential,
                maxPingAttempts: MaxPingAttempts,
                timeoutInSeconds: TimeoutInSeconds).GetAwaiter().GetResult();

            foreach (var result in results)
            {
                OutputResult(result);
            }
        }
        catch (Exception ex)
        {
            WriteError(new ErrorRecord(ex, "SyncOperationFailed", ErrorCategory.OperationStopped, targets));
        }
    }

    private void OutputResult(WakeOnLanReturn result)
    {
        if (WriteHost.IsPresent)
        {
            string message = result.WolSuccess
                ? $"Wake-on-LAN succeeded for {result.TargetComputerName} ({result.TargetMacAddress}) via {result.ProxyComputerName}."
                : $"Wake-on-LAN failed for {result.TargetComputerName} ({result.TargetMacAddress}) via {result.ProxyComputerName}. Error: {result.ErrorMessage ?? "Unknown error."}";

            Host.UI.WriteLine(message);
        }
        else
        {
            WriteObject(result);
        }
    }
}

