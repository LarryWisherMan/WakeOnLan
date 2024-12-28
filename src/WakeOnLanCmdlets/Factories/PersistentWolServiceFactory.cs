using System;
using WakeOnLanLibrary.Factories;
using WakeOnLanLibrary.Services;


namespace WakeOnLanCmdlets.factories
{
    public static class PersistentWolServiceFactory
    {
        private static PersistentWolService _persistentService;

        /// <summary>
        /// Gets or creates the singleton instance of the PersistentWolService.
        /// </summary>
        public static PersistentWolService GetOrCreateService()
        {
            if (_persistentService == null)
            {
                // Initialize dependencies
                var packetSender = new ProxyMagicPacketSender(new RemotePowerShellExecutor());
                var validator = new ComputerValidator();
                var computerFactory = new ComputerFactory();
                var runspaceManager = new RunspaceManager();

                // Create the PersistentWolService
                _persistentService = new PersistentWolService(
                    packetSender,
                    validator,
                    computerFactory,
                    runspaceManager);
            }

            return _persistentService;
        }

        /// <summary>
        /// Static cleanup method to be used in the module's OnRemove block.
        /// </summary>
        public static void Cleanup()
        {
            var service = PersistentWolServiceFactory.GetOrCreateService();
            service.ClearRunspaces();

            Console.WriteLine("All runspaces have been cleared successfully.");
        }
    }
}
