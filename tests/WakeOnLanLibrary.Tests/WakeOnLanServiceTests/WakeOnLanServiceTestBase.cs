using Microsoft.Extensions.DependencyInjection;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Shared.Extensions;
using WakeOnLanLibrary.Tests.Mocks;

namespace WakeOnLanLibrary.Tests.WakeOnLanServiceTests
{
    public abstract class WakeOnLanServiceTestBase
    {
        protected IServiceProvider ServiceProvider { get; private set; }
        protected IWakeOnLanService Service { get; private set; }

        protected WakeOnLanServiceTestBase()
        {
            InitializeServiceProvider();
            InitializeService();
        }

        private void InitializeServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddMockServices(); // Use the test service extension
            ServiceProvider = services.BuildServiceProvider();
        }

        private void OuchInitializeServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddWakeOnLanServices(); // Use the test service extension
            ServiceProvider = services.BuildServiceProvider();
        }

        private void InitializeService()
        {
            Service = ServiceProvider.GetRequiredService<IWakeOnLanService>();
        }
    }
}
