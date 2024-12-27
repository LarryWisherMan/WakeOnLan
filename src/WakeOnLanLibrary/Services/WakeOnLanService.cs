using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading.Tasks;
using WakeOnLanLibrary.Interfaces;
using WakeOnLanLibrary.Models;

namespace WakeOnLanLibrary.Services
{
    public class WakeOnLanService
    {
        private readonly IMagicPacketSender _packetSender;
        private readonly IComputerValidator _computerValidator;
        private readonly IComputerFactory _computerFactory;
        private readonly IRunspaceManager _runspaceManager;
        private readonly WakeOnLanResultCache _resultCache;
        private readonly IMonitorService _monitorService;

        public WakeOnLanService(
            IMagicPacketSender packetSender,
            IComputerValidator computerValidator,
            IComputerFactory computerFactory,
            IRunspaceManager runspaceManager,
            WakeOnLanResultCache resultCache,
            IMonitorService monitorService)
        {
            _packetSender = packetSender ?? throw new ArgumentNullException(nameof(packetSender));
            _computerValidator = computerValidator ?? throw new ArgumentNullException(nameof(computerValidator));
            _computerFactory = computerFactory ?? throw new ArgumentNullException(nameof(computerFactory));
            _runspaceManager = runspaceManager ?? throw new ArgumentNullException(nameof(runspaceManager));
            _resultCache = resultCache ?? throw new ArgumentNullException(nameof(resultCache));
            _monitorService = monitorService ?? throw new ArgumentNullException(nameof(monitorService));
        }

        /// <summary>
        /// Sends multiple WoL requests for a single proxy and monitors their status.
        /// </summary>
        private async Task<IEnumerable<WakeOnLanReturn>> WakeUpAndMonitorForProxyAsync(
            string proxyComputerName,
            List<(string MacAddress, string ComputerName)> targets,
            int port,
            PSCredential credential,
            int maxPingAttempts,
            int timeoutInSeconds)
        {
            var results = new ConcurrentBag<WakeOnLanReturn>();

            // Validate the proxy computer
            var proxyComputer = _computerFactory.CreateProxyComputer(proxyComputerName, port);
            var proxyValidation = _computerValidator.Validate(proxyComputer);

            if (!proxyValidation.IsValid)
            {
                foreach (var target in targets)
                {
                    var failureResult = CreateFailureResult(target.ComputerName, target.MacAddress, proxyComputerName, port, proxyValidation.Message, requestSent: false);
                    results.Add(failureResult);
                    _resultCache.AddOrUpdate($"{target.ComputerName}:{target.MacAddress}", failureResult);
                }
                return results;
            }

            // Get or create the proxy computer's runspace
            Runspace proxyRunspace;
            try
            {
                proxyRunspace = _runspaceManager.GetOrCreateRunspace(proxyComputerName, credential);
            }
            catch (Exception ex)
            {
                // Handle runspace creation failure
                foreach (var target in targets)
                {
                    var failureResult = CreateFailureResult(target.ComputerName, target.MacAddress, proxyComputerName, port, $"Failed to create runspace: {ex.Message}", requestSent: false);
                    results.Add(failureResult);
                    _resultCache.AddOrUpdate($"{target.ComputerName}:{target.MacAddress}", failureResult);
                }
                return results;
            }

            var wolRequests = new List<WakeOnLanRequest>();
            foreach (var target in targets)
            {
                var targetComputer = _computerFactory.CreateTargetComputer(target.ComputerName, target.MacAddress);
                var targetValidation = _computerValidator.Validate(targetComputer);

                if (!targetValidation.IsValid)
                {
                    var failureResult = CreateFailureResult(target.ComputerName, target.MacAddress, proxyComputerName, port, targetValidation.Message, requestSent: false);
                    results.Add(failureResult);
                    _resultCache.AddOrUpdate($"{target.ComputerName}:{target.MacAddress}", failureResult);
                    continue;
                }

                wolRequests.Add(new WakeOnLanRequest
                {
                    TargetMacAddress = target.MacAddress,
                    TargetComputerName = target.ComputerName,
                    Port = port,
                    ProxyRunspace = proxyRunspace
                });
            }

            if (wolRequests.Any())
            {
                try
                {
                    await _packetSender.SendPacketAsync(wolRequests);
                    foreach (var request in wolRequests)
                    {
                        results.Add(new WakeOnLanReturn
                        {
                            TargetComputerName = request.TargetComputerName,
                            TargetMacAddress = request.TargetMacAddress,
                            ProxyComputerName = proxyComputerName,
                            Port = port,
                            RequestSent = true,
                            WolSuccess = false,
                            Timestamp = DateTime.UtcNow
                        });
                    }
                }
                catch (Exception ex)
                {
                    foreach (var request in wolRequests)
                    {
                        var failureResult = CreateFailureResult(request.TargetComputerName, request.TargetMacAddress, proxyComputerName, port, $"Failed to send WoL packet: {ex.Message}", requestSent: false);
                        results.Add(failureResult);
                        _resultCache.AddOrUpdate($"{request.TargetComputerName}:{request.TargetMacAddress}", failureResult);
                    }
                    return results;
                }

                var monitoringTasks = wolRequests.Select(request => MonitorTargetAsync(request, maxPingAttempts, timeoutInSeconds));
                var monitoredResults = await Task.WhenAll(monitoringTasks);
                foreach (var result in monitoredResults)
                {
                    results.Add(result);
                }
            }

            return results;
        }

        /// <summary>
        /// Sends multiple WoL requests and monitors their status.
        /// </summary>
        public async Task<IEnumerable<WakeOnLanReturn>> WakeUpAndMonitorMultipleAsync(
            Dictionary<string, List<(string MacAddress, string ComputerName)>> proxyToTargets,
            int port = 9,
            PSCredential credential = null,
            int maxPingAttempts = 5,
            int timeoutInSeconds = 60)
        {
            var tasks = proxyToTargets.Select(async proxyEntry =>
            {
                try
                {
                    return await WakeUpAndMonitorForProxyAsync(
                        proxyEntry.Key,
                        proxyEntry.Value,
                        port,
                        credential,
                        maxPingAttempts,
                        timeoutInSeconds);
                }
                catch (Exception ex)
                {
                    return proxyEntry.Value.Select(target =>
                        CreateFailureResult(target.ComputerName, target.MacAddress, proxyEntry.Key, port, $"Unexpected error: {ex.Message}", requestSent: false));
                }
            });

            var results = await Task.WhenAll(tasks);
            return results.SelectMany(r => r);
        }

        /// <summary>
        /// Monitors a single target and updates the cache with the result.
        /// </summary>
        private async Task<WakeOnLanReturn> MonitorTargetAsync(WakeOnLanRequest request, int maxPingAttempts, int timeoutInSeconds)
        {
            var monitorEntry = new MonitorEntry
            {
                ComputerName = request.TargetComputerName,
                ProxyComputerName = request.ProxyRunspace.ConnectionInfo.ComputerName,
                WolSentTime = DateTime.UtcNow,
                WolSuccess = false,
                IsMonitoringComplete = false
            };

            _monitorService.MonitorCache.Add(monitorEntry);

            var isAwake = await _monitorService.MonitorComputerAsync(monitorEntry, maxPingAttempts, timeoutInSeconds);

            monitorEntry.WolSuccess = isAwake;
            monitorEntry.IsMonitoringComplete = true;

            var result = new WakeOnLanReturn
            {
                TargetComputerName = request.TargetComputerName,
                TargetMacAddress = request.TargetMacAddress,
                ProxyComputerName = request.ProxyRunspace.ConnectionInfo.ComputerName,
                Port = request.Port,
                RequestSent = true,
                WolSuccess = isAwake,
                ErrorMessage = isAwake ? null : "The target computer did not respond to pings.",
                Timestamp = DateTime.UtcNow
            };

            _resultCache.AddOrUpdate($"{request.TargetComputerName}:{request.TargetMacAddress}", result);
            return result;
        }

        /// <summary>
        /// Creates a failure result object.
        /// </summary>
        private WakeOnLanReturn CreateFailureResult(string computerName, string macAddress, string proxyComputerName, int port, string errorMessage, bool requestSent)
        {
            return new WakeOnLanReturn
            {
                TargetComputerName = computerName,
                TargetMacAddress = macAddress,
                ProxyComputerName = proxyComputerName,
                Port = port,
                RequestSent = requestSent,
                WolSuccess = false,
                ErrorMessage = errorMessage,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
