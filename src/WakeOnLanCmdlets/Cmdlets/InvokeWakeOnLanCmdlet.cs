using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;
using WakeOnLanCmdlets.Base;
using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Infrastructure.Factories;

/// <summary>
/// <para type="synopsis">Sends Wake-on-LAN (WOL) packets to target computers and monitors their wake-up status.</para>
/// <para type="description">The Invoke-WakeOnLan cmdlet sends WOL packets to specified target computers via proxy computers. It supports single-target, multiple-target, and hashtable-based input scenarios. The cmdlet also monitors the wake-up status of the targets and provides results. Users can choose between synchronous and asynchronous execution modes.</para>
/// </summary>
/// <example>
/// <code>Invoke-WakeOnLan -MacAddress "00:11:22:33:44:55" -ComputerName "TargetPC" -ProxyComputerName "ProxyPC"</code>
/// <para>Sends a WOL packet to the target computer "TargetPC" via the proxy computer "ProxyPC".</para>
/// </example>
/// <example>
/// <code>$targets = @{
///     "Proxy1" = @(
///         @{ MacAddress = "00:11:22:33:44:55"; ComputerName = "Target1" },
///         @{ MacAddress = "66:77:88:99:AA:BB"; ComputerName = "Target2" }
///     )
/// }
/// Invoke-WakeOnLan -HashtableTargets $targets</code>
/// <para>Sends WOL packets to the specified targets using the hashtable input.</para>
/// </example>
/// <example>
/// <code>$proxyToTargets = @{
///     "Proxy1" = @(
///         (MacAddress = "00:11:22:33:44:55", ComputerName = "Target1"),
///         (MacAddress = "66:77:88:99:AA:BB", ComputerName = "Target2")
///     )
/// }
/// Invoke-WakeOnLan -ProxyToTargets $proxyToTargets -Async</code>
/// <para>Sends WOL packets asynchronously to the specified targets using the dictionary input.</para>
/// </example>
[Cmdlet(VerbsLifecycle.Invoke, "WakeOnLan", DefaultParameterSetName = "SingleTarget")]
public class InvokeWakeOnLanCmdlet : BaseCmdlet
{
    /// <summary>
    /// <para type="description">The MAC address of the target computer. This parameter is required for the "SingleTarget" parameter set.</para>
    /// </summary>
    [Parameter(Mandatory = true, ParameterSetName = "SingleTarget")]
    public string MacAddress { get; set; }

    /// <summary>
    /// <para type="description">The name of the target computer. This parameter is required for the "SingleTarget" parameter set.</para>
    /// </summary>
    [Parameter(Mandatory = true, ParameterSetName = "SingleTarget")]
    public string ComputerName { get; set; }

    /// <summary>
    /// <para type="description">The name of the proxy computer that sends the WOL packet. This parameter is required for the "SingleTarget" parameter set.</para>
    /// </summary>
    [Parameter(Mandatory = true, ParameterSetName = "SingleTarget")]
    public string ProxyComputerName { get; set; }

    /// <summary>
    /// <para type="description">A dictionary mapping proxy computer names to their associated target systems. This parameter is required for the "MultipleTargets" parameter set.</para>
    /// </summary>
    [Parameter(Mandatory = true, ParameterSetName = "MultipleTargets")]
    public Dictionary<string, List<(string MacAddress, string ComputerName)>> ProxyToTargets { get; set; }

    /// <summary>
    /// <para type="description">A hashtable mapping proxy computer names to target systems. Each target must include keys for "MacAddress" and "ComputerName".</para>
    /// </summary>
    [Parameter(Mandatory = true, ParameterSetName = "HashtableTargets")]
    public Hashtable HashtableTargets { get; set; }

    /// <summary>
    /// <para type="description">The port used for sending WOL packets. Defaults to 9.</para>
    /// </summary>
    [Parameter]
    public int Port { get; set; } = 9;

    /// <summary>
    /// <para type="description">Credentials for authenticating with the proxy computer, if required.</para>
    /// </summary>
    [Parameter]
    public PSCredential Credential { get; set; }

    /// <summary>
    /// <para type="description">The maximum number of ping attempts for monitoring. Defaults to 5.</para>
    /// </summary>
    [Parameter]
    public int MaxPingAttempts { get; set; } = 5;

    /// <summary>
    /// <para type="description">The timeout period for monitoring, in seconds. Defaults to 60 seconds.</para>
    /// </summary>
    [Parameter]
    public int TimeoutInSeconds { get; set; } = 60;

    /// <summary>
    /// <para type="description">Runs the WOL operation asynchronously.</para>
    /// </summary>
    [Parameter]
    public SwitchParameter Async { get; set; }

    /// <summary>
    /// <para type="description">If specified, results are written to the host instead of the output pipeline.</para>
    /// </summary>
    [Parameter]
    public SwitchParameter WriteHost { get; set; }

    // Declare the CancellationTokenSource
    private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    /// <summary>
    /// Handles the main logic for the cmdlet's execution.
    /// </summary>
    protected override void ProcessRecord()
    {
        base.ProcessRecord();

        var targets = GetTargets();

        if (Async.IsPresent)
        {
            ExecuteAsync(targets);
            WriteVerbose("The Wake-on-LAN operation is running asynchronously.");
        }
        else
        {
            ExecuteSynchronously(targets);
        }
    }

    /// <summary>
    /// Cancels processing if the cmdlet is interrupted.
    /// </summary>
    protected override void StopProcessing()
    {
        base.StopProcessing();
        _cancellationTokenSource.Cancel();
    }

    /// <summary>
    /// Resolves the target systems based on the selected parameter set.
    /// </summary>
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
            //"HashtableTargets" => ParseHashtableTargets(HashtableTargets),
            _ => throw new PSArgumentException("Invalid parameter set.")
        };
    }

    /// <summary>
    /// Executes the Wake-on-LAN operation asynchronously.
    /// </summary>
    /// <param name="targets">The target systems mapped to their respective proxy computers.</param>
    private void ExecuteAsync(Dictionary<string, List<(string MacAddress, string ComputerName)>> targets)
    {
        Task.Run(async () =>
        {
            try
            {
                // Perform the Wake-on-LAN operation and monitor results.
                var results = await WolService.WakeUpAndMonitorAsync(
                    targets,
                    Port,
                    Credential,
                    MaxPingAttempts,
                    TimeoutInSeconds);

                foreach (var result in results)
                {
                    OutputResult(result);
                }
            }
            catch (OperationCanceledException)
            {
                WriteWarning("The asynchronous operation was canceled.");
            }
            catch (Exception ex)
            {
                WriteError(new ErrorRecord(ex, "AsyncOperationFailed", ErrorCategory.OperationStopped, targets));
            }
        }, _cancellationTokenSource.Token);
    }

    /// <summary>
    /// Executes the Wake-on-LAN operation synchronously.
    /// </summary>
    /// <param name="targets">The target systems mapped to their respective proxy computers.</param>
    private void ExecuteSynchronously(Dictionary<string, List<(string MacAddress, string ComputerName)>> targets)
    {
        try
        {
            // Perform the Wake-on-LAN operation and monitor results.
            var results = WolService.WakeUpAndMonitorAsync(
                targets,
                Port,
                Credential,
                MaxPingAttempts,
                TimeoutInSeconds).GetAwaiter().GetResult();

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

    /// <summary>
    /// Outputs the result of the Wake-on-LAN operation.
    /// </summary>
    /// <param name="result">The result of the operation for a single target system.</param>
    private void OutputResult(WakeOnLanReturn result)
    {
        if (WriteHost.IsPresent)
        {
            // Write results directly to the host.
            Host.UI.WriteLine(result.WolSuccess
                ? $"WOL succeeded for {result.TargetComputerName} ({result.TargetMacAddress})."
                : $"WOL failed for {result.TargetComputerName} ({result.TargetMacAddress}). Error: {result.ErrorMessage ?? "Unknown error."}");
        }
        else
        {
            // Output results to the pipeline.
            WriteObject(result);
        }
    }
}

