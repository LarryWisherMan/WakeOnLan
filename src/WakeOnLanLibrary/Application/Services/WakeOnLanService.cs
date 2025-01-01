using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Application.Interfaces.Validation;
using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Core.Interfaces;
using WakeOnLanLibrary.Core.UseCases;
using WakeOnLanLibrary.Infrastructure.Caching;

namespace WakeOnLanLibrary.Application.Services
{
    public class WakeOnLanService : IWakeOnLanService
    {
        private readonly IMagicPacketSender _packetSender;
        private readonly IComputerValidator _computerValidator;
        private readonly IComputerFactory _computerFactory;
        private readonly IRunspaceManager _runspaceManager;
        private readonly IRequestQueue _requestQueue;
        private readonly WakeOnLanResultCache _resultCache;
        private readonly IMonitorService _monitorService;
        private readonly MonitorCache _monitorCache;

        public MonitorCache MonitorCache => _monitorCache;
        public WakeOnLanResultCache ResultCache => _resultCache;
        public IRunspaceManager RunspaceManager => _runspaceManager;
        public IMagicPacketSender PacketSender => _packetSender;
        public IComputerValidator ComputerValidator => _computerValidator;
        public IComputerFactory ComputerFactory => _computerFactory;
        public IRequestQueue RequestQueue => _requestQueue;
        public IMonitorService MonitorService => _monitorService;




        public WakeOnLanService(
            IMagicPacketSender packetSender,
            IComputerValidator computerValidator,
            IComputerFactory computerFactory,
            IRunspaceManager runspaceManager,
            IRequestQueue requestQueue,
            WakeOnLanResultCache resultCache,
            IMonitorService monitorService,
            MonitorCache monitorCache)
        {
            _packetSender = packetSender ?? throw new ArgumentNullException(nameof(packetSender));
            _computerValidator = computerValidator ?? throw new ArgumentNullException(nameof(computerValidator));
            _computerFactory = computerFactory ?? throw new ArgumentNullException(nameof(computerFactory));
            _runspaceManager = runspaceManager ?? throw new ArgumentNullException(nameof(runspaceManager));
            _requestQueue = requestQueue ?? throw new ArgumentNullException(nameof(requestQueue));
            _resultCache = resultCache ?? throw new ArgumentNullException(nameof(resultCache));
            _monitorService = monitorService ?? throw new ArgumentNullException(nameof(monitorService));
            _monitorCache = monitorCache ?? throw new ArgumentNullException(nameof(monitorCache));

            _monitorService.MonitoringCompleted += UpdateMonitoringResult;


        }

        /// <summary>
        /// Sends multiple Wake-on-LAN requests and adds them to the monitoring cache.
        /// </summary>
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

                // Validate proxy computer name
                var proxyValidation = _computerValidator.Validate(_computerFactory.CreateProxyComputer(proxyComputerName, port));
                if (!proxyValidation.IsValid)
                {
                    AddFailureResults(proxyComputerName, targets, port, proxyValidation.Message);
                    continue;
                }

                _requestQueue.Enqueue(async () =>
                {
                    try
                    {
                        // Get or create a runspace pool for the proxy
                        var runspacePool = _runspaceManager.GetOrCreateRunspacePool(proxyComputerName, credential, minRunspaces: 1, maxRunspaces: 5);

                        // Process requests for the proxy
                        await ProcessProxyRequestsAsync(
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
                        Console.WriteLine($"Error processing proxy {proxyComputerName}: {ex.Message}");
                        AddFailureResults(proxyComputerName, targets, port, $"Runspace pool creation failed: {ex.Message}");
                    }
                });
            }

            // Start processing the queue
            await _requestQueue.ProcessQueueAsync();

            // Start monitoring asynchronously
            _ = Task.Run(() => _monitorService.StartMonitoringAsync(maxPingAttempts, timeoutInSeconds));

            return _resultCache.GetAll();
        }


        /// <summary>
        /// Processes Wake-on-LAN requests for a specific proxy.
        /// </summary>
        private async Task ProcessProxyRequestsAsync(
            string proxyComputerName,
            List<(string MacAddress, string ComputerName)> targets,
            int port,
            PSCredential credential,
            RunspacePool runspacePool,
            int maxPingAttempts,
            int timeoutInSeconds)
        {

            // Create WOL requests
            var wolRequests = new List<WakeOnLanRequest>();
            foreach (var target in targets)
            {
                var targetComputer = _computerFactory.CreateTargetComputer(target.ComputerName, target.MacAddress);
                var targetValidation = _computerValidator.Validate(targetComputer);

                if (!targetValidation.IsValid)
                {
                    AddFailureResults(proxyComputerName, new List<(string, string)> { target }, port, targetValidation.Message);
                    continue;
                }

                var wolRequest = new WakeOnLanRequest
                {
                    TargetMacAddress = target.MacAddress,
                    TargetComputerName = target.ComputerName,
                    Port = port,
                    ProxyRunspacePool = runspacePool
                };

                wolRequests.Add(wolRequest);

                // Add to monitoring cache
                _monitorCache.AddOrUpdate(target.ComputerName, new MonitorEntry
                {
                    ComputerName = target.ComputerName,
                    ProxyComputerName = proxyComputerName,
                    WolSentTime = DateTime.UtcNow,
                    MonitoringEndTime = DateTime.UtcNow.AddSeconds(timeoutInSeconds),
                    WolSuccess = false,
                    IsMonitoringComplete = false
                });

            }

            if (wolRequests.Any())
            {
                await _packetSender.SendPacketAsync(wolRequests);
                AddSuccessResults(wolRequests);
            }
        }

        /// <summary>
        /// Adds failure results to the result cache.
        /// </summary>
        private void AddFailureResults(
            string proxyComputerName,
            List<(string MacAddress, string ComputerName)> targets,
            int port,
            string errorMessage)
        {
            foreach (var target in targets)
            {
                var result = CreateFailureResult(target.ComputerName, target.MacAddress, proxyComputerName, port, errorMessage);
                _resultCache.AddOrUpdate($"{target.ComputerName}:{target.MacAddress}", result);
            }
        }

        /// <summary>
        /// Adds success results to the result cache.
        /// </summary>
        private void AddSuccessResults(IEnumerable<WakeOnLanRequest> wolRequests)
        {
            foreach (var request in wolRequests)
            {
                var result = new WakeOnLanReturn
                {
                    TargetComputerName = request.TargetComputerName,
                    TargetMacAddress = request.TargetMacAddress,
                    ProxyComputerName = request.ProxyRunspacePool.ConnectionInfo.ComputerName,
                    Port = request.Port,
                    RequestSent = true,
                    WolSuccess = false, // Success will be updated after monitoring
                    Timestamp = DateTime.UtcNow
                };

                _resultCache.AddOrUpdate($"{request.TargetComputerName}:{request.TargetMacAddress}", result);
            }
        }

        /// <summary>
        /// Creates a failure result object.
        /// </summary>
        private WakeOnLanReturn CreateFailureResult(
            string computerName,
            string macAddress,
            string proxyComputerName,
            int port,
            string errorMessage)
        {
            return new WakeOnLanReturn
            {
                TargetComputerName = computerName,
                TargetMacAddress = macAddress,
                ProxyComputerName = proxyComputerName,
                Port = port,
                RequestSent = false,
                WolSuccess = false,
                ErrorMessage = errorMessage,
                Timestamp = DateTime.UtcNow
            };
        }




        public void UpdateMonitoringResult(string computerName, bool success, string errorMessage = null)
        {
            // Find the first matching key with the computer name
            var cacheKey = _resultCache.GetAllKeys()
                                       .FirstOrDefault(key => key.StartsWith($"{computerName}:"));

            if (cacheKey == null) return; // No matching key found

            if (success)
            {
                if (_resultCache.TryGetValue(cacheKey, out var result))
                {
                    result.WolSuccess = true;
                    _resultCache.AddOrUpdate(cacheKey, result);
                }
            }
            else
            {
                if (_resultCache.TryGetValue(cacheKey, out var result))
                {
                    result.WolSuccess = false;
                    result.ErrorMessage = errorMessage;
                    _resultCache.AddOrUpdate(cacheKey, result);
                }
            }
        }





    }
}
