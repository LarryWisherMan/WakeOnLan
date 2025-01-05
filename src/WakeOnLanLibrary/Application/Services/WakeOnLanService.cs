using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Application.Models;

namespace WakeOnLanLibrary.Application.Services
{
    public class WakeOnLanService : IWakeOnLanService
    {
        private readonly WakeOnLanConfiguration _config;
        private readonly IProxyRequestProcessor _proxyRequestProcessor;
        private readonly IResultManager _resultManager;
        private readonly IRequestScheduler _requestScheduler;
        private readonly IMonitoringManager _monitoringManager;
        private readonly ITaskRunner _taskRunner;


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

        private void OnMonitoringCompleted(string computerName, bool success, string errorMessage)
        {
            _resultManager.UpdateMonitoringResult(computerName, success, errorMessage);
        }

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

        private (int ResolvedPort, int ResolvedMaxPingAttempts, int ResolvedTimeout, int MinRunspaces, int MaxRunspaces) ResolveParameters(
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

        private void EnqueueProxyTasks(
           Dictionary<string, List<(string MacAddress, string ComputerName)>> proxyToTargets,
           PSCredential credential,
           (int ResolvedPort, int ResolvedMaxPingAttempts, int ResolvedTimeout, int MinRunspaces, int MaxRunspaces) parameters)
        {
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


        private void StartMonitoring((int ResolvedPort, int ResolvedMaxPingAttempts, int ResolvedTimeout, int MinRunspaces, int MaxRunspaces) parameters)
        {
            _taskRunner.Run(() => _monitoringManager.StartMonitoringAsync(
                parameters.ResolvedMaxPingAttempts,
                parameters.ResolvedTimeout));
        }
    }

}
