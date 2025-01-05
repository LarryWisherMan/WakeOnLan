using Microsoft.Extensions.DependencyInjection;
using Moq;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Application.Interfaces.Helpers;
using WakeOnLanLibrary.Application.Interfaces.Validation;
using WakeOnLanLibrary.Application.Services;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Core.Interfaces;
using WakeOnLanLibrary.Core.UseCases;
using WakeOnLanLibrary.Infrastructure.Builders;
using WakeOnLanLibrary.Infrastructure.Caching;
using WakeOnLanLibrary.Infrastructure.Monitoring;

namespace WakeOnLanLibrary.Tests.Mocks
{
    public static class ServiceCollectionExtensionsMocks
    {
        public static IServiceCollection AddMockServices(this IServiceCollection services)
        {
            // Add mock validators
            services.AddSingleton(Mock.Of<INameIpValidator>());
            services.AddSingleton(Mock.Of<IMacAddressHelper>());
            services.AddSingleton(Mock.Of<IValidationStrategy<Computer>>());
            services.AddSingleton(Mock.Of<IValidationStrategy<TargetComputer>>());
            services.AddSingleton(Mock.Of<IValidationStrategy<ProxyComputer>>());

            // Mock composite validators
            services.AddSingleton(Mock.Of<IComputerValidator>());

            // Mock caches - keeping real implementations if needed
            // Register generic cache
            services.AddSingleton(typeof(ICache<,>), typeof(Cache<,>));

            // Register specific caches using their interfaces
            services.AddSingleton<IMonitorCache, MonitorCache>();
            services.AddSingleton<IWakeOnLanResultCache, WakeOnLanResultCache>();

            // Register ResultManager
            services.AddSingleton<IResultManager>(provider =>
            {
                var resultCache = provider.GetRequiredService<IWakeOnLanResultCache>();
                return new ResultManager(resultCache);
            });

            // Mock monitoring services
            services.AddSingleton(Mock.Of<IMonitorTask>());
            //services.AddSingleton<IMonitorService>(provider =>
            //{
            //    var monitorCache = provider.GetRequiredService<IMonitorCache>();
            //    var monitorTask = provider.GetRequiredService<IMonitorTask>();
            //    return new MonitorService(monitorCache, monitorTask, maxConcurrentTasks: 5, intervalInSeconds: 10);
            //});

            services.AddSingleton(Mock.Of<IMonitorService>());

            // Mock runspace services
            services.AddSingleton(Mock.Of<IRunspaceProvider>());
            services.AddSingleton(Mock.Of<IRunspaceManager>());
            services.AddSingleton(Mock.Of<IRequestQueue>());

            // Mock builder and executor services
            services.AddSingleton(Mock.Of<IScriptBuilder>());
            services.AddSingleton(Mock.Of<IRemotePowerShellExecutor>());

            // Mock proxy request processor
            services.AddSingleton(Mock.Of<IProxyRequestProcessor>());

            // Mock computer factory and packet sender
            services.AddSingleton(Mock.Of<IComputerFactory>());
            services.AddSingleton(Mock.Of<IMagicPacketSender>());

            // Register WakeOnLanService
            services.AddSingleton<IWakeOnLanService, WakeOnLanService>();

            return services;
        }
    }
}
