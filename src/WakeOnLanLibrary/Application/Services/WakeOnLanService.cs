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
        private readonly IProxyRequestProcessor _proxyRequestProcessor;
        private readonly IResultManager _resultManager;
        private readonly IRunspaceManager _runspaceManager;
        private readonly IRequestQueue _requestQueue;
        private readonly IMonitorService _monitorService;

        public WakeOnLanService(
            IProxyRequestProcessor proxyRequestProcessor,
            IResultManager resultManager,
            IRunspaceManager runspaceManager,
            IRequestQueue requestQueue,
            IMonitorService monitorService)
        {
            _proxyRequestProcessor = proxyRequestProcessor ?? throw new ArgumentNullException(nameof(proxyRequestProcessor));
            _resultManager = resultManager ?? throw new ArgumentNullException(nameof(resultManager));
            _runspaceManager = runspaceManager ?? throw new ArgumentNullException(nameof(runspaceManager));
            _requestQueue = requestQueue ?? throw new ArgumentNullException(nameof(requestQueue));
            _monitorService = monitorService ?? throw new ArgumentNullException(nameof(monitorService));

            _monitorService.MonitoringCompleted += UpdateMonitoringResult;
        }

        public async Task<IEnumerable<WakeOnLanReturn>> WakeUpAndMonitorAsync(
            Dictionary<string, List<(string MacAddress, string ComputerName)>> proxyToTargets,
            int port = 9,
            PSCredential credential = null,
            int maxPingAttempts = 5,
            int timeoutInSeconds = 60)
        {
            foreach (var proxyEntry in proxyToTargets)
            {
                var proxyComputerName = proxyEntry.Key;
                var targets = proxyEntry.Value;

                // Enqueue processing for each proxy
                _requestQueue.Enqueue(async () =>
                {
                    try
                    {
                        var runspacePool = _runspaceManager.GetOrCreateRunspacePool(proxyComputerName, credential, 1, 5);
                        await _proxyRequestProcessor.ProcessProxyRequestsAsync(
                            proxyComputerName,
                            targets,
                            port,
                            credential,
                            runspacePool,
                            maxPingAttempts,
                            timeoutInSeconds);
                    }
                    catch (Exception ex)
                    {
                        _resultManager.AddFailureResults(proxyComputerName, targets, port, $"Runspace pool creation failed: {ex.Message}");
                    }
                });
            }

            // Process queued requests
            await _requestQueue.ProcessQueueAsync();

            // Start monitoring asynchronously
            _ = Task.Run(() => _monitorService.StartMonitoringAsync(maxPingAttempts, timeoutInSeconds));

            return _resultManager.GetAllResults();
        }

        public void UpdateMonitoringResult(string computerName, bool success, string errorMessage = null)
        {
            _resultManager.UpdateMonitoringResult(computerName, success, errorMessage);
        }
    }
}
