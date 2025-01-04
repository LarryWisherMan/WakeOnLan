using Moq;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Application.Interfaces.Validation;
using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Application.Services;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Core.Interfaces;
using WakeOnLanLibrary.Core.UseCases;
using WakeOnLanLibrary.Infrastructure.Caching;

namespace WakeOnLanLibrary.Tests.WakeOnLanServiceTests
{
    public abstract class WakeOnLanServiceTestBase
    {
        protected Mock<IMagicPacketSender> MockPacketSender;
        protected Mock<IComputerValidator> MockComputerValidator;
        protected Mock<IComputerFactory> MockComputerFactory;
        protected Mock<IRunspaceManager> MockRunspaceManager;
        protected Mock<IRequestQueue> MockRequestQueue;
        protected Mock<IMonitorService> MockMonitorService;
        protected Mock<ICache<string, MonitorEntry>> MockMonitorCache;
        protected Mock<ICache<string, WakeOnLanReturn>> MockResultCache;
        protected WakeOnLanResultCache ResultCache;
        protected MonitorCache MonitorCache;

        protected WakeOnLanService Service;

        protected WakeOnLanServiceTestBase()
        {
            InitializeMocks();
            InitializeCaches();
            InitializeService();
        }

        private void InitializeMocks()
        {
            MockPacketSender = new Mock<IMagicPacketSender>();
            MockComputerValidator = new Mock<IComputerValidator>();
            MockComputerFactory = new Mock<IComputerFactory>();
            MockRunspaceManager = new Mock<IRunspaceManager>();
            MockRequestQueue = new Mock<IRequestQueue>();
            MockMonitorService = new Mock<IMonitorService>();


        }

        private void InitializeCaches()
        {
            // Use real instances of caches with in-memory implementations
            var monitorCacheImplementation = new Cache<string, MonitorEntry>();
            var resultCacheImplementation = new Cache<string, WakeOnLanReturn>();
            MonitorCache = new MonitorCache(monitorCacheImplementation);
            ResultCache = new WakeOnLanResultCache(resultCacheImplementation);
        }

        private void InitializeService()
        {
            Service = new WakeOnLanService(
                MockPacketSender.Object,
                MockComputerValidator.Object,
                MockComputerFactory.Object,
                MockRunspaceManager.Object,
                MockRequestQueue.Object,
                ResultCache,
                MockMonitorService.Object,
                MonitorCache
            );
        }

        public void ClearCaches()
        {
            MonitorCache.Clear();
            ResultCache.Clear();
        }
    }
}
