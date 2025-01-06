using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Threading.Tasks;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Application.Interfaces.Validation;
using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Core.Interfaces;

namespace WakeOnLanLibrary.Application.Services
{
    public class ProxyRequestProcessor : IProxyRequestProcessor
    {
        private readonly IMagicPacketSender _packetSender;
        private readonly IComputerValidator _computerValidator;
        private readonly IComputerFactory _computerFactory;
        private readonly IMonitorCache _monitorCache;
        private readonly IResultManager _resultManager;
        private readonly IRunspaceManager _runspaceManager;

        public ProxyRequestProcessor(
            IMagicPacketSender packetSender,
            IComputerValidator computerValidator,
            IComputerFactory computerFactory,
            IMonitorCache monitorCache,
            IResultManager resultManager,
            IRunspaceManager runspaceManager)
        {
            _packetSender = packetSender ?? throw new ArgumentNullException(nameof(packetSender));
            _computerValidator = computerValidator ?? throw new ArgumentNullException(nameof(computerValidator));
            _computerFactory = computerFactory ?? throw new ArgumentNullException(nameof(computerFactory));
            _monitorCache = monitorCache ?? throw new ArgumentNullException(nameof(monitorCache));
            _resultManager = resultManager ?? throw new ArgumentNullException(nameof(resultManager));
            _runspaceManager = runspaceManager ?? throw new ArgumentNullException(nameof(runspaceManager));
        }

        public async Task ProcessProxyRequestsAsync(
               string proxyComputerName,
               List<(string MacAddress, string ComputerName)> targets,
               int port,
               PSCredential credential,
               int minRunspaces,
               int maxRunspaces,
               int maxPingAttempts,
               int timeoutInSeconds)
        {
            if (targets == null || !targets.Any()) return;

            // Early exit if proxyComputerName is null or empty
            if (string.IsNullOrWhiteSpace(proxyComputerName)) return;

            // Step 1: Initialize the runspace pool.
            var runspacePool = InitializeRunspacePool(proxyComputerName, port, credential, minRunspaces, maxRunspaces, targets);
            if (runspacePool == null) return;

            // Step 2: Prepare the Wake-on-LAN requests.
            var wolRequests = PrepareWakeOnLanRequests(proxyComputerName, targets, port, runspacePool, timeoutInSeconds);

            // Step 3: Send Wake-on-LAN requests.
            if (wolRequests.Any())
            {
                try
                {
                    await ExecuteWakeOnLanRequestsAsync(wolRequests);
                }
                catch (Exception ex)
                {
                    _resultManager.AddFailureResults(proxyComputerName, targets, port, $"An error occurred: {ex.Message}");
                }
            }
        }


        private IRunspacePool InitializeRunspacePool(
        string proxyComputerName,
        int port,
        PSCredential credential,
        int minRunspaces,
        int maxRunspaces,
       List<(string MacAddress, string ComputerName)> targets)
        {
            var proxyComputer = _computerFactory.CreateProxyComputer(proxyComputerName, port);

            // Validate proxy computer.
            if (!TryValidateComputer(proxyComputer,
                message => _resultManager.AddFailureResults(proxyComputerName, targets, port, message),
                out _))
            {
                return null;
            }

            return _runspaceManager.GetOrCreateRunspacePool(proxyComputerName, credential, minRunspaces, maxRunspaces);
        }


        internal List<WakeOnLanRequest> PrepareWakeOnLanRequests(
            string proxyComputerName,
            List<(string MacAddress, string ComputerName)> targets,
            int port,
            IRunspacePool runspacePool,
            int timeoutInSeconds)
        {
            var wolRequests = new List<WakeOnLanRequest>();

            foreach (var target in targets)
            {
                if (!TryValidateTarget(target, proxyComputerName, port, out _)) continue;

                // Create and add Wake-on-LAN request.
                wolRequests.Add(CreateWakeOnLanRequest(target, port, runspacePool));

                try
                {
                    // Update monitor cache
                    AddToMonitorCache(proxyComputerName, target, timeoutInSeconds);
                }
                catch (Exception ex)
                {
                    // Log exception and continue processing other targets
                    _resultManager.AddFailureResults(proxyComputerName, new List<(string, string)> { target }, port,
                        $"Failed to update monitor cache: {ex.Message}");
                }
            }

            return wolRequests;
        }

        private WakeOnLanRequest CreateWakeOnLanRequest(
       (string MacAddress, string ComputerName) target,
       int port,
       IRunspacePool runspacePool)
        {
            return new WakeOnLanRequest
            {
                TargetMacAddress = target.MacAddress,
                TargetComputerName = target.ComputerName,
                Port = port,
                ProxyRunspacePool = runspacePool
            };
        }

        private void AddToMonitorCache(string proxyComputerName, (string MacAddress, string ComputerName) target, int timeoutInSeconds)
        {
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

        private async Task ExecuteWakeOnLanRequestsAsync(List<WakeOnLanRequest> wolRequests)
        {
            await _packetSender.SendPacketAsync(wolRequests);
            _resultManager.AddSuccessResults(wolRequests);
        }

        private bool TryValidateComputer<TComputer>(
            TComputer computer,
            Action<string> handleFailure,
            out TComputer validatedComputer) where TComputer : Computer
        {
            validatedComputer = computer;
            var validationResult = _computerValidator.Validate(computer);

            if (!validationResult.IsValid)
            {
                handleFailure?.Invoke(validationResult.Message);
                return false;
            }

            return true;
        }

        private bool TryValidateTarget(
            (string MacAddress, string ComputerName) target,
            string proxyComputerName,
            int port,
            out TargetComputer validatedTarget)
        {
            var targetComputer = _computerFactory.CreateTargetComputer(target.ComputerName, target.MacAddress);

            return TryValidateComputer(targetComputer,
                message => _resultManager.AddFailureResults(proxyComputerName, new List<(string, string)> { target }, port, message),
                out validatedTarget);
        }

    }
}
