using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Core.Interfaces;
using WakeOnLanLibrary.Infrastructure.Builders;

namespace WakeOnLanLibrary.Infrastructure.Services
{
    public class ProxyMagicPacketSender : IMagicPacketSender
    {
        private readonly IScriptBuilder _scriptBuilder;
        private readonly IRemotePowerShellExecutor _executor;

        public ProxyMagicPacketSender(IScriptBuilder scriptBuilder, IRemotePowerShellExecutor executor)
        {
            _scriptBuilder = scriptBuilder ?? throw new ArgumentNullException(nameof(scriptBuilder));
            _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        }

        /// <summary>
        /// Sends a single magic packet request via a proxy RunspacePool.
        /// </summary>
        public async Task SendPacketAsync(WakeOnLanRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            ValidateRequest(request);

            // Build and execute the script for a single request
            var script = _scriptBuilder.BuildScript(new[] { request });
            await _executor.ExecuteAsync(request.ProxyRunspacePool, script);
        }

        /// <summary>
        /// Sends multiple magic packet requests grouped by proxy RunspacePool.
        /// </summary>
        public async Task SendPacketAsync(IEnumerable<WakeOnLanRequest> requests)
        {
            if (requests == null) throw new ArgumentNullException(nameof(requests));

            // Group requests by proxy RunspacePool
            var groupedRequests = requests
                .GroupBy(request => request.ProxyRunspacePool)
                .ToDictionary(group => group.Key, group => group.ToList());

            // Execute scripts for each RunspacePool
            foreach (var group in groupedRequests)
            {
                var script = _scriptBuilder.BuildScript(group.Value);
                await _executor.ExecuteAsync(group.Key, script);
            }
        }

        /// <summary>
        /// Validates the WakeOnLanRequest object.
        /// </summary>
        private void ValidateRequest(WakeOnLanRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.TargetMacAddress))
                throw new ArgumentException("Target MAC address cannot be null or empty.", nameof(request.TargetMacAddress));

            if (request.ProxyRunspacePool == null)
                throw new ArgumentException("Proxy RunspacePool must be provided.", nameof(request.ProxyRunspacePool));
        }
    }
}
