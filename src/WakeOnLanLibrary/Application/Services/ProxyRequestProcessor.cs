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

namespace WakeOnLanLibrary.Application.Services
{
    public class ProxyRequestProcessor : IProxyRequestProcessor
    {
        private readonly IMagicPacketSender _packetSender;
        private readonly IComputerValidator _computerValidator;
        private readonly IComputerFactory _computerFactory;
        private readonly IMonitorCache _monitorCache;
        private readonly IResultManager _resultManager;
        public ProxyRequestProcessor(
            IMagicPacketSender packetSender,
            IComputerValidator computerValidator,
            IComputerFactory computerFactory,
            IMonitorCache monitorCache,
            IResultManager resultManager)
        {
            _packetSender = packetSender ?? throw new ArgumentNullException(nameof(packetSender));
            _computerValidator = computerValidator ?? throw new ArgumentNullException(nameof(computerValidator));
            _computerFactory = computerFactory ?? throw new ArgumentNullException(nameof(computerFactory));
            _monitorCache = monitorCache ?? throw new ArgumentNullException(nameof(monitorCache));
            _resultManager = resultManager ?? throw new ArgumentNullException(nameof(resultManager));
        }

        public async Task ProcessProxyRequestsAsync(
            string proxyComputerName,
            List<(string MacAddress, string ComputerName)> targets,
            int port,
            PSCredential credential,
            RunspacePool runspacePool,
            int maxPingAttempts,
            int timeoutInSeconds)
        {
            var wolRequests = new List<WakeOnLanRequest>();

            foreach (var target in targets)
            {
                var targetComputer = _computerFactory.CreateTargetComputer(target.ComputerName, target.MacAddress);
                var targetValidation = _computerValidator.Validate(targetComputer);

                if (!targetValidation.IsValid)
                {
                    _resultManager.AddFailureResults(proxyComputerName, new List<(string, string)> { target }, port, targetValidation.Message);
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
    }
}
