using WakeOnLanLibrary.Services;

namespace WakeOnLanLibrary.Factories
{
    public static class WolServiceFactory
    {
        /// <summary>
        /// Creates a fully configured instance of WakeOnLanService with all dependencies.
        /// </summary>
        public static WakeOnLanService Create()
        {
            // Instantiate dependencies
            var executor = new RemotePowerShellExecutor();
            var packetSender = new ProxyMagicPacketSender(executor);
            var validator = new ComputerValidator();
            var runspaceManager = new RunspaceManager();
            var resultCache = new WakeOnLanResultCache();
            var computerFactory = new ComputerFactory();

            // Create the WakeOnLanService
            return new WakeOnLanService(
                packetSender,
                validator,
                computerFactory,
                runspaceManager,
                resultCache
            );
        }
    }
}
