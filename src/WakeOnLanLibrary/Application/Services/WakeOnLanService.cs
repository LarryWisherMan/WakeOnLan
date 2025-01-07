using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Application.Models;

namespace WakeOnLanLibrary.Application.Services
{
    /// <summary>
    /// Service for managing Wake-on-LAN (WOL) requests, scheduling proxy tasks, and monitoring target systems.
    /// </summary>
    public class WakeOnLanService : IWakeOnLanService
    {
        private readonly WakeOnLanConfiguration _config;
        private readonly IProxyRequestProcessor _proxyRequestProcessor;
        private readonly IResultManager _resultManager;
        private readonly IRequestScheduler _requestScheduler;
        private readonly IMonitoringManager _monitoringManager;
        private readonly ITaskRunner _taskRunner;

        /// <summary>
        /// Initializes a new instance of the <see cref="WakeOnLanService"/> class.
        /// </summary>
        /// <param name="proxyRequestProcessor">Processor for handling WOL proxy requests.</param>
        /// <param name="resultManager">Manager for recording results of WOL operations.</param>
        /// <param name="requestScheduler">Scheduler for managing asynchronous task execution.</param>
        /// <param name="monitoringManager">Manager for monitoring target systems after WOL requests.</param>
        /// <param name="taskRunner">Service for running tasks in a controlled manner.</param>
        /// <param name="config">Configuration for WOL operations.</param>
        public WakeOnLanService(
            IProxyRequestProcessor proxyRequestProcessor,
            IResultManager resultManager,
            IRequestScheduler requestScheduler,
            IMonitoringManager monitoringManager,
            ITaskRunner taskRunner,
            IOptions<WakeOnLanConfiguration> config)
        {
            _proxyRequestProcessor = proxyRequestProcessor ?? throw new ArgumentNullException(nameof(proxyRequestProcessor));
            _resultManager = resultManager ?? throw new ArgumentNullException(nameof(resultManager));
            _requestScheduler = requestScheduler ?? throw new ArgumentNullException(nameof(requestScheduler));
            _monitoringManager = monitoringManager ?? throw new ArgumentNullException(nameof(monitoringManager));
            _taskRunner = taskRunner ?? throw new ArgumentNullException(nameof(taskRunner));
            _config = config?.Value ?? throw new ArgumentNullException(nameof(config));

            // Subscribe to the MonitoringCompleted event
            _monitoringManager.MonitoringCompleted += OnMonitoringCompleted;
        }

        /// <summary>
        /// Handles the event triggered when monitoring is completed for a target system.
        /// </summary>
        /// <param name="computerName">Name of the target computer.</param>
        /// <param name="success">Indicates whether the monitoring was successful.</param>
        /// <param name="errorMessage">Error message, if any.</param>
        private void OnMonitoringCompleted(string computerName, bool success, string errorMessage)
        {
            _resultManager.UpdateMonitoringResult(computerName, success, errorMessage);
        }

        /// <summary>
        /// Sends Wake-on-LAN requests and monitors the status of target systems.
        /// </summary>
        /// <param name="proxyToTargets">Mapping of proxy computers to their respective target systems.</param>
        /// <param name="port">Optional port for WOL requests.</param>
        /// <param name="credential">Optional credentials for accessing proxy computers.</param>
        /// <param name="maxPingAttempts">Optional maximum number of ping attempts for monitoring.</param>
        /// <param name="timeoutInSeconds">Optional timeout for monitoring operations, in seconds.</param>
        /// <returns>A collection of WOL results.</returns>
        public async Task<IEnumerable<WakeOnLanReturn>> WakeUpAndMonitorAsync(
            Dictionary<string, List<(string MacAddress, string ComputerName)>> proxyToTargets,
            int? port = null,
            PSCredential credential = null,
            int? maxPingAttempts = null,
            int? timeoutInSeconds = null)
        {
            var parameters = ResolveParameters(port, maxPingAttempts, timeoutInSeconds);

            EnqueueProxyTasks(proxyToTargets, credential, parameters);

            // Process queued requests
            await _requestScheduler.ExecuteScheduledTasksAsync();

            // Start monitoring asynchronously
            StartMonitoring(parameters);
            return _resultManager.GetAllResults();
        }

        /// <summary>
        /// Resolves WOL configuration parameters, applying defaults where necessary.
        /// </summary>
        /// <param name="port">Optional port for WOL requests.</param>
        /// <param name="maxPingAttempts">Optional maximum ping attempts for monitoring.</param>
        /// <param name="timeoutInSeconds">Optional timeout for monitoring, in seconds.</param>
        /// <returns>Resolved WOL parameters.</returns>
        internal (int ResolvedPort, int ResolvedMaxPingAttempts, int ResolvedTimeout, int MinRunspaces, int MaxRunspaces) ResolveParameters(
            int? port,
            int? maxPingAttempts,
            int? timeoutInSeconds)
        {
            return (
                ResolvedPort: port ?? _config.DefaultPort,
                ResolvedMaxPingAttempts: maxPingAttempts ?? _config.MaxPingAttempts,
                ResolvedTimeout: timeoutInSeconds ?? _config.DefaultTimeoutInSeconds,
                MinRunspaces: _config.RunspacePoolMinThreads,
                MaxRunspaces: _config.RunspacePoolMaxThreads
            );
        }

        /// <summary>
        /// Enqueues tasks for processing WOL proxy requests.
        /// </summary>
        /// <param name="proxyToTargets">Mapping of proxy computers to their respective targets.</param>
        /// <param name="credential">Credentials for accessing proxy computers.</param>
        /// <param name="parameters">Resolved WOL parameters.</param>
        private void EnqueueProxyTasks(
            Dictionary<string, List<(string MacAddress, string ComputerName)>> proxyToTargets,
            PSCredential credential,
            (int ResolvedPort, int ResolvedMaxPingAttempts, int ResolvedTimeout, int MinRunspaces, int MaxRunspaces) parameters)
        {
            if (proxyToTargets == null || !proxyToTargets.Any())
            {
                return; // Exit early if the dictionary is empty
            }

            foreach (var proxyEntry in proxyToTargets)
            {
                var proxyComputerName = proxyEntry.Key;
                var targets = proxyEntry.Value;

                _requestScheduler.Schedule(async () =>
                {
                    try
                    {
                        await _proxyRequestProcessor.ProcessProxyRequestsAsync(
                            proxyComputerName,
                            targets,
                            parameters.ResolvedPort,
                            credential,
                            parameters.MinRunspaces,
                            parameters.MaxRunspaces,
                            parameters.ResolvedMaxPingAttempts,
                            parameters.ResolvedTimeout);
                    }
                    catch (Exception ex)
                    {
                        _resultManager.AddFailureResults(
                            proxyComputerName,
                            targets,
                            parameters.ResolvedPort,
                            $"{ex.Message}");
                    }
                });
            }
        }

        /// <summary>
        /// Starts the monitoring process for target systems.
        /// </summary>
        /// <param name="parameters">Resolved WOL parameters.</param>
        private void StartMonitoring((int ResolvedPort, int ResolvedMaxPingAttempts, int ResolvedTimeout, int MinRunspaces, int MaxRunspaces) parameters)
        {
            _taskRunner.Run(() => _monitoringManager.StartMonitoringAsync(
                parameters.ResolvedMaxPingAttempts,
                parameters.ResolvedTimeout));
        }
    }
}
