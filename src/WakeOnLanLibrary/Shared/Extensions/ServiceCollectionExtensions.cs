using Microsoft.Extensions.DependencyInjection;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Application.Interfaces.Helpers;
using WakeOnLanLibrary.Application.Interfaces.Validation;
using WakeOnLanLibrary.Application.Services;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Core.Interfaces;
using WakeOnLanLibrary.Core.UseCases;
using WakeOnLanLibrary.Core.Validators;
using WakeOnLanLibrary.Infrastructure.Builders;
using WakeOnLanLibrary.Infrastructure.Caching;
using WakeOnLanLibrary.Infrastructure.Factories;
using WakeOnLanLibrary.Infrastructure.Helpers;
using WakeOnLanLibrary.Infrastructure.Monitoring;
using WakeOnLanLibrary.Infrastructure.Services;

namespace WakeOnLanLibrary.Shared.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            // Register INameIpValidator
            services.AddSingleton<INameIpValidator, NameIpValidator>(); // Replace with actual implementation class

            // Register IMacAddressHelper
            services.AddSingleton<IMacAddressHelper, MacAddressHelper>(); // Replace `MacAddressHelper` with the actual implementation class

            // Register Validation Strategies
            services.AddSingleton<IValidationStrategy<Computer>, GeneralComputerValidationStrategy>();
            services.AddSingleton<IValidationStrategy<TargetComputer>, TargetComputerValidationStrategy>();
            services.AddSingleton<IValidationStrategy<ProxyComputer>, ProxyComputerValidationStrategy>();

            // Register Composite Validators
            services.AddSingleton(provider =>
                new CompositeValidator<Computer>(provider.GetServices<IValidationStrategy<Computer>>()));

            services.AddSingleton(provider =>
                new CompositeValidator<TargetComputer>(provider.GetServices<IValidationStrategy<TargetComputer>>()));

            services.AddSingleton(provider =>
                new CompositeValidator<ProxyComputer>(provider.GetServices<IValidationStrategy<ProxyComputer>>()));

            // Register ComputerValidator
            services.AddSingleton<IComputerValidator, ComputerValidator>();

            return services;
        }


        public static IServiceCollection AddCaches(this IServiceCollection services)
        {
            // Register generic cache
            services.AddSingleton(typeof(ICache<,>), typeof(Cache<,>));

            // Register specific caches
            services.AddSingleton<MonitorCache>();
            services.AddSingleton<WakeOnLanResultCache>();

            return services;
        }

        public static IServiceCollection AddMonitoringServices(this IServiceCollection services)
        {
            // Register NetworkHelper
            services.AddSingleton<INetworkHelper, NetworkHelper>();

            // Register MonitorTask
            services.AddSingleton<IMonitorTask, MonitorTask>();

            // Register MonitorService
            services.AddSingleton<IMonitorService>(provider =>
            {
                var monitorService = new MonitorService(
                    provider.GetRequiredService<MonitorCache>(),
                    provider.GetRequiredService<IMonitorTask>(),
                    maxConcurrentTasks: 5,
                    intervalInSeconds: 10);

                // No direct dependency on IMonitorCallback
                return monitorService;
            });

            // Ensure the IServiceCollection is returned
            return services;
        }


        public static IServiceCollection AddRunspaceServices(this IServiceCollection services)
        {
            services.AddSingleton<IRunspaceProvider, RunspaceProvider>();
            services.AddSingleton<IRunspaceManager, RunspaceManager>();

            // Register IRequestQueue with a default maxConcurrency value
            services.AddSingleton<IRequestQueue>(provider =>
                new RequestQueue(maxConcurrency: 5)); // Set the default maxConcurrency

            return services;
        }


        public static IServiceCollection AddWakeOnLanServices(this IServiceCollection services)
        {
            // Register Validators
            services.AddValidators();

            // Register Caches
            services.AddCaches();

            //Register CallBack


            // Register Monitoring Services
            services.AddMonitoringServices();

            // Register Runspace Services
            services.AddRunspaceServices();

            // Register Builder Services
            services.AddSingleton<IScriptBuilder, ScriptBuilder>();

            // Register Remote PowerShell Executor
            services.AddSingleton<IRemotePowerShellExecutor, RemotePowerShellExecutor>();

            // Register Other Services
            services.AddSingleton<IMagicPacketSender, ProxyMagicPacketSender>();
            services.AddSingleton<IComputerFactory, ComputerFactory>();

            // Register WakeOnLanService
            services.AddSingleton<IWakeOnLanService, WakeOnLanService>();

            //services.AddSingleton<IMonitorCallback>(provider =>
            //    new MonitorCallback(provider.GetRequiredService<IWakeOnLanService>()));

            return services;
        }
    }
}

