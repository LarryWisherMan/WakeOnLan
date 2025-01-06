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
            var wolRequests = new List<WakeOnLanRequest>();

            var runspacePool = TryInitializeProxy(proxyComputerName, port, credential, minRunspaces, maxRunspaces, targets);

            if (runspacePool == null)
            {
                return;
            }

            foreach (var target in targets)
            {
                if (!TryValidateTarget(target, proxyComputerName, port, out var validatedTarget))
                    continue;

                var wolRequest = new WakeOnLanRequest
                {
                    TargetMacAddress = target.MacAddress,
                    TargetComputerName = target.ComputerName,
                    Port = port,
                    ProxyRunspacePool = runspacePool
                };

                wolRequests.Add(wolRequest);

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
                _resultManager.AddSuccessResults(wolRequests);
            }
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

        private IRunspacePool TryInitializeProxy(
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
                out var validatedProxy))
            {
                return null;
            }

            return _runspaceManager.GetOrCreateRunspacePool(proxyComputerName, credential, minRunspaces, maxRunspaces);
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
