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
    /// <summary>
    /// Service responsible for processing Wake-on-LAN (WOL) proxy requests, managing runspaces, and monitoring cache updates.
    /// </summary>
    public class ProxyRequestProcessor : IProxyRequestProcessor
    {
        private readonly IMagicPacketSender _packetSender;
        private readonly IComputerValidator _computerValidator;
        private readonly IComputerFactory _computerFactory;
        private readonly IMonitorCache _monitorCache;
        private readonly IResultManager _resultManager;
        private readonly IRunspaceManager _runspaceManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProxyRequestProcessor"/> class.
        /// </summary>
        /// <param name="packetSender">Service to send Wake-on-LAN magic packets.</param>
        /// <param name="computerValidator">Validator to ensure computers meet criteria for WOL requests.</param>
        /// <param name="computerFactory">Factory to create proxy and target computers.</param>
        /// <param name="monitorCache">Cache to store and manage monitoring entries.</param>
        /// <param name="resultManager">Service to manage success and failure results.</param>
        /// <param name="runspaceManager">Manager for creating and managing PowerShell runspace pools.</param>
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

        /// <summary>
        /// Processes proxy requests for Wake-on-LAN (WOL).
        /// </summary>
        /// <param name="proxyComputerName">Name of the proxy computer handling the requests.</param>
        /// <param name="targets">List of target computers and their MAC addresses.</param>
        /// <param name="port">Port used for WOL packets.</param>
        /// <param name="credential">Credentials for accessing the proxy computer.</param>
        /// <param name="minRunspaces">Minimum number of runspaces in the pool.</param>
        /// <param name="maxRunspaces">Maximum number of runspaces in the pool.</param>
        /// <param name="maxPingAttempts">Maximum number of ping attempts for monitoring.</param>
        /// <param name="timeoutInSeconds">Timeout duration in seconds for monitoring.</param>
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
            if (string.IsNullOrWhiteSpace(proxyComputerName)) return;

            var runspacePool = InitializeRunspacePool(proxyComputerName, port, credential, minRunspaces, maxRunspaces, targets);
            if (runspacePool == null) return;

            var wolRequests = PrepareWakeOnLanRequests(proxyComputerName, targets, port, runspacePool, timeoutInSeconds);

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

        /// <summary>
        /// Initializes a runspace pool for the proxy computer.
        /// </summary>
        private IRunspacePool InitializeRunspacePool(
            string proxyComputerName,
            int port,
            PSCredential credential,
            int minRunspaces,
            int maxRunspaces,
            List<(string MacAddress, string ComputerName)> targets)
        {
            var proxyComputer = _computerFactory.CreateProxyComputer(proxyComputerName, port);

            if (!TryValidateComputer(proxyComputer,
                message => _resultManager.AddFailureResults(proxyComputerName, targets, port, message),
                out _))
            {
                return null;
            }

            return _runspaceManager.GetOrCreateRunspacePool(proxyComputerName, credential, minRunspaces, maxRunspaces);
        }

        /// <summary>
        /// Prepares Wake-on-LAN requests for the target computers.
        /// </summary>
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

                wolRequests.Add(CreateWakeOnLanRequest(target, port, runspacePool));

                try
                {
                    AddToMonitorCache(proxyComputerName, target, timeoutInSeconds);
                }
                catch (Exception ex)
                {
                    _resultManager.AddFailureResults(proxyComputerName, new List<(string, string)> { target }, port,
                        $"Failed to update monitor cache: {ex.Message}");
                }
            }

            return wolRequests;
        }

        /// <summary>
        /// Creates a Wake-on-LAN request.
        /// </summary>
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

        /// <summary>
        /// Adds a target to the monitoring cache.
        /// </summary>
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

        /// <summary>
        /// Sends Wake-on-LAN requests asynchronously.
        /// </summary>
        private async Task ExecuteWakeOnLanRequestsAsync(List<WakeOnLanRequest> wolRequests)
        {
            await _packetSender.SendPacketAsync(wolRequests);
            _resultManager.AddSuccessResults(wolRequests);
        }

        /// <summary>
        /// Validates a computer object.
        /// </summary>
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

        /// <summary>
        /// Validates a target computer for Wake-on-LAN.
        /// </summary>
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
