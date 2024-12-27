using System;
using System.Management.Automation;
using System.Threading.Tasks;
using WakeOnLanLibrary.interfaces;
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

        public WakeOnLanService(
            IMagicPacketSender packetSender,
            IComputerValidator computerValidator,
            IComputerFactory computerFactory,
            IRunspaceManager runspaceManager,
            WakeOnLanResultCache resultCache
            )
        {
            _packetSender = packetSender ?? throw new ArgumentNullException(nameof(packetSender));
            _computerValidator = computerValidator ?? throw new ArgumentNullException(nameof(computerValidator));
            _computerFactory = computerFactory ?? throw new ArgumentNullException(nameof(computerFactory));
            _runspaceManager = runspaceManager ?? throw new ArgumentNullException(nameof(runspaceManager));
            _resultCache = resultCache ?? throw new ArgumentNullException(nameof(resultCache));
        }

        public async Task<WakeOnLanReturn> WakeUpAsync(string macAddress, string computerName, string proxyComputerName, int port = 9, PSCredential credential = null)
        {
            var result = new WakeOnLanReturn
            {
                TargetComputerName = computerName,
                TargetMacAddress = macAddress,
                ProxyComputerName = proxyComputerName,
                Port = port,
                Timestamp = DateTime.UtcNow
            };

            try
            {
                // Create and validate the target computer
                var targetComputer = _computerFactory.CreateTargetComputer(computerName, macAddress);
                ValidateComputer(targetComputer);

                // Create and validate the proxy computer
                var proxyComputer = _computerFactory.CreateProxyComputer(proxyComputerName, port);
                ValidateComputer(proxyComputer);

                // Get or create the proxy computer's runspace
                var proxyRunspace = _runspaceManager.GetOrCreateRunspace(proxyComputerName, credential);

                // Construct the WOL request
                var wolRequest = new WakeOnLanRequest
                {
                    TargetComputerName = computerName,
                    TargetMacAddress = macAddress,
                    Port = port,
                    ProxyRunspace = proxyRunspace
                };

                // Send the WOL request via the proxy
                await _packetSender.SendPacketAsync(wolRequest);

                // Mark success
                result.IsSuccess = true;
            }
            catch (Exception ex)
            {
                // Mark failure and log error
                result.IsSuccess = false;
                result.ErrorMessage = ex.Message;
            }
            finally
            {
                // Cache the result
                var key = $"{computerName}:{macAddress}";
                _resultCache.AddOrUpdate(key, result);
            }

            return result;
        }
        private void ValidateComputer(Computer computer)
        {
            var result = _computerValidator.Validate(computer);
            if (!result.IsValid)
            {
                throw new ArgumentException(result.Message);
            }
        }
    }

}
