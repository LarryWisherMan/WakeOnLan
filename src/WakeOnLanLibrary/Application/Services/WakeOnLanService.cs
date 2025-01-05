using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Threading.Tasks;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Core.UseCases;

namespace WakeOnLanLibrary.Application.Services
{
    public class WakeOnLanService : IWakeOnLanService
    {
        private readonly WakeOnLanConfiguration _config;
        private readonly IProxyRequestProcessor _proxyRequestProcessor;
        private readonly IResultManager _resultManager;
        private readonly IRunspaceManager _runspaceManager;
        private readonly IRequestScheduler _requestScheduler;
        private readonly IMonitorService _monitorService;
        private readonly ITaskRunner _taskRunner;


        public WakeOnLanService(
            IProxyRequestProcessor proxyRequestProcessor,
            IResultManager resultManager,
            IRunspaceManager runspaceManager,
            IRequestScheduler requestScheduler,
            IMonitorService monitorService,
            ITaskRunner taskRunner,
            IOptions<WakeOnLanConfiguration> config)

        {
            _proxyRequestProcessor = proxyRequestProcessor ?? throw new ArgumentNullException(nameof(proxyRequestProcessor));
            _resultManager = resultManager ?? throw new ArgumentNullException(nameof(resultManager));
            _runspaceManager = runspaceManager ?? throw new ArgumentNullException(nameof(runspaceManager));
            _requestScheduler = requestScheduler ?? throw new ArgumentNullException(nameof(requestScheduler));
            _monitorService = monitorService ?? throw new ArgumentNullException(nameof(monitorService));
            _taskRunner = taskRunner ?? throw new ArgumentNullException(nameof(taskRunner));
            _config = config?.Value ?? throw new ArgumentNullException(nameof(config));

            _monitorService.MonitoringCompleted += UpdateMonitoringResult;
        }

        public async Task<IEnumerable<WakeOnLanReturn>> WakeUpAndMonitorAsync(
            Dictionary<string, List<(string MacAddress, string ComputerName)>> proxyToTargets,
            int? port = null,
            PSCredential credential = null,
            int? maxPingAttempts = null,
            int? timeoutInSeconds = null)

        {

            int resolvedPort = port ?? _config.DefaultPort;
            int resolvedMaxPingAttempts = maxPingAttempts ?? _config.MaxPingAttempts;
            int resolvedTimeout = timeoutInSeconds ?? _config.DefaultTimeoutInSeconds;

            int minRunspaces = _config.RunspacePoolMinThreads;
            int maxRunspaces = _config.RunspacePoolMaxThreads;


            foreach (var proxyEntry in proxyToTargets)
            {
                var proxyComputerName = proxyEntry.Key;
                var targets = proxyEntry.Value;

                // Enqueue processing for each proxy
                _requestScheduler.Schedule(async () =>
                {
                    try
                    {
                        var runspacePool = _runspaceManager.GetOrCreateRunspacePool(proxyComputerName, credential, minRunspaces, maxRunspaces);
                        await _proxyRequestProcessor.ProcessProxyRequestsAsync(
                            proxyComputerName,
                            targets,
                            resolvedPort,
                            credential,
                            runspacePool,
                            resolvedMaxPingAttempts,
                            resolvedTimeout);
                    }
                    catch (Exception ex)
                    {
                        _resultManager.AddFailureResults(proxyComputerName, targets, resolvedPort, $"Runspace pool creation failed: {ex.Message}");
                    }
                });
            }

            // Process queued requests
            await _requestScheduler.ExecuteScheduledTasksAsync();

            // Start monitoring asynchronously
            _taskRunner.Run(() => _monitorService.StartMonitoringAsync(resolvedMaxPingAttempts, resolvedTimeout));

            return _resultManager.GetAllResults();
        }

        public void UpdateMonitoringResult(string computerName, bool success, string errorMessage = null)
        {
            _resultManager.UpdateMonitoringResult(computerName, success, errorMessage);
        }
    }
}
