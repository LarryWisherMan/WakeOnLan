using System;
using System.Collections.Generic;
using System.Linq;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Application.Models;

namespace WakeOnLanLibrary.Application.Services
{
    public class ResultManager : IResultManager
    {
        private readonly IWakeOnLanResultCache _resultCache;

        public ResultManager(IWakeOnLanResultCache resultCache)
        {
            _resultCache = resultCache ?? throw new ArgumentNullException(nameof(resultCache));
        }

        public void AddSuccessResults(IEnumerable<WakeOnLanRequest> requests)
        {
            foreach (var request in requests)
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

        public void AddFailureResults(string proxyComputerName, List<(string MacAddress, string ComputerName)> targets, int port, string errorMessage)
        {
            foreach (var target in targets)
            {
                var result = CreateFailureResult(target.ComputerName, target.MacAddress, proxyComputerName, port, errorMessage);
                _resultCache.AddOrUpdate($"{target.ComputerName}:{target.MacAddress}", result);
            }
        }

        public IEnumerable<WakeOnLanReturn> GetAllResults()
        {
            return _resultCache.GetAllResults();
        }

        public void UpdateMonitoringResult(string computerName, bool success, string errorMessage = null)
        {
            var cacheKey = _resultCache.GetAllKeys().FirstOrDefault(key => key.StartsWith($"{computerName}:"));

            if (cacheKey == null) return;

            if (_resultCache.TryGetValue(cacheKey, out var result))
            {
                result.WolSuccess = success;
                result.ErrorMessage = success ? null : errorMessage;
                _resultCache.AddOrUpdate(cacheKey, result);
            }
        }

        private WakeOnLanReturn CreateFailureResult(string computerName, string macAddress, string proxyComputerName, int port, string errorMessage)
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
    }
}
