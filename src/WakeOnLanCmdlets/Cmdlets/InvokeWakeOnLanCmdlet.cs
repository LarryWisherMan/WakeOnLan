using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management.Automation;
using System.Threading.Tasks;
using WakeOnLanCmdlets.Base;
using WakeOnLanLibrary.Factories;

/// <summary>
/// Cmdlet to send Wake-on-LAN (WoL) requests to target computers through proxy computers.
/// </summary>
/// <para type="synopsis">Sends Wake-on-LAN requests to specified targets using proxy computers.</para>
/// <para type="description">
/// The cmdlet allows sending WoL requests to individual or multiple target computers.
/// It supports synchronous and asynchronous execution modes, and can accept targets as 
/// dictionaries, hashtables, or individual parameters.
/// </para>
/// <example>
/// <code>
/// Invoke-WakeOnLan -MacAddress "00:1A:2B:3C:4D:5E" -ComputerName "Target1" -ProxyComputerName "Proxy1"
/// </code>
/// Sends a Wake-on-LAN request to a single target through a specified proxy computer.
/// </example>
/// <example>
/// <code>
/// $targets = @{
///     "Proxy1" = @(
///         @{ MacAddress = "00:1A:2B:3C:4D:5E"; ComputerName = "Target1" },
///         @{ MacAddress = "00:2B:3C:4D:5E:6F"; ComputerName = "Target2" }
///     )
///     "Proxy2" = @(
///         @{ MacAddress = "00:3C:4D:5E:6F:7G"; ComputerName = "Target3" }
///     )
/// }
/// Invoke-WakeOnLan -HashtableTargets $targets
/// </code>
/// Sends Wake-on-LAN requests to multiple targets grouped by proxy computers.
/// </example>
[Cmdlet(VerbsLifecycle.Invoke, "WakeOnLan", DefaultParameterSetName = "SingleTarget")]
public class InvokeWakeOnLanCmdlet : BaseCmdlet
{
    /// <summary>
    /// The MAC address of the target computer for the SingleTarget parameter set.
    /// </summary>
    [Parameter(Mandatory = true, ParameterSetName = "SingleTarget")]
    [ValidateNotNullOrEmpty]
    [Alias("MAC")]
    [Description("The MAC address of the target computer.")]
    public string MacAddress { get; set; }

    /// <summary>
    /// The name of the target computer for the SingleTarget parameter set.
    /// </summary>
    [Parameter(Mandatory = true, ParameterSetName = "SingleTarget")]
    [ValidateNotNullOrEmpty]
    [Description("The name of the target computer.")]
    public string ComputerName { get; set; }

    /// <summary>
    /// The name of the proxy computer for the SingleTarget parameter set.
    /// </summary>
    [Parameter(Mandatory = true, ParameterSetName = "SingleTarget")]
    [ValidateNotNullOrEmpty]
    [Description("The name of the proxy computer.")]
    public string ProxyComputerName { get; set; }

    /// <summary>
    /// A dictionary of proxies and their associated target computers.
    /// </summary>
    [Parameter(Mandatory = true, ParameterSetName = "MultipleTargets")]
    [Description("A dictionary where keys are proxy computer names and values are lists of target computers.")]
    public Dictionary<string, List<(string MacAddress, string ComputerName)>> ProxyToTargets { get; set; }

    /// <summary>
    /// A hashtable of proxies and their associated target computers.
    /// </summary>
    [Parameter(Mandatory = true, ParameterSetName = "HashtableTargets")]
    [Description("A hashtable where keys are proxy computer names and values are lists of target computers.")]
    public Hashtable HashtableTargets { get; set; }

    /// <summary>
    /// The port number to use for sending the Wake-on-LAN request.
    /// </summary>
    [Parameter]
    [Description("The port number to use for sending the WoL packet.")]
    [ValidateRange(1, 65535)]
    public int Port { get; set; } = 9;

    /// <summary>
    /// The credentials for authenticating with the proxy computer.
    /// </summary>
    [Parameter]
    [Description("The credentials for authenticating with the proxy computer.")]
    public PSCredential Credential { get; set; }

    /// <summary>
    /// The maximum number of ping attempts to verify the target computer's state.
    /// </summary>
    [Parameter]
    [Description("The maximum number of ping attempts to verify if the target computer is online.")]
    [ValidateRange(1, 10)]
    public int MaxPingAttempts { get; set; } = 5;

    /// <summary>
    /// The timeout duration in seconds for each ping attempt.
    /// </summary>
    [Parameter]
    [Description("The timeout duration in seconds for each ping attempt.")]
    [ValidateRange(1, 300)]
    public int TimeoutInSeconds { get; set; } = 60;

    /// <summary>
    /// Specifies whether to execute the operation asynchronously.
    /// </summary>
    [Parameter]
    [Description("Executes the operation asynchronously.")]
    public SwitchParameter Async { get; set; }

    protected override void ProcessRecord()
    {
        base.ProcessRecord();

        Dictionary<string, List<(string MacAddress, string ComputerName)>> targets;

        // Determine the active parameter set
        if (this.ParameterSetName == "SingleTarget")
        {
            targets = new Dictionary<string, List<(string MacAddress, string ComputerName)>>
            {
                { ProxyComputerName, new List<(string, string)> { (MacAddress, ComputerName) } }
            };
        }
        else if (this.ParameterSetName == "MultipleTargets")
        {
            targets = ProxyToTargets;
        }
        else if (this.ParameterSetName == "HashtableTargets")
        {
            targets = TargetMappingFactory.CreateProxyToTargetsMapping(HashtableTargets);
        }
        else
        {
            throw new PSArgumentException("Invalid parameter set.");
        }

        if (Async.IsPresent)
        {
            // Start asynchronous operation
            Task.Run(async () =>
            {
                var results = await WolService.WakeUpAndMonitorMultipleAsync(
                    proxyToTargets: targets,
                    port: Port,
                    credential: Credential,
                    maxPingAttempts: MaxPingAttempts,
                    timeoutInSeconds: TimeoutInSeconds
                );

                foreach (var result in results)
                {
                    WriteObject(result);
                }
            });

            WriteVerbose("The Wake-on-LAN operation is running asynchronously in the background.");
        }
        else
        {
            // Execute synchronously
            var results = WolService.WakeUpAndMonitorMultipleAsync(
                proxyToTargets: targets,
                port: Port,
                credential: Credential,
                maxPingAttempts: MaxPingAttempts,
                timeoutInSeconds: TimeoutInSeconds
            ).GetAwaiter().GetResult();

            WriteObject(results, enumerateCollection: true);
        }
    }
}
