using Microsoft.Extensions.DependencyInjection;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Application.Interfaces.Helpers;
using WakeOnLanLibrary.Application.Interfaces.Validation;
using WakeOnLanLibrary.Application.Models;
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
            services.AddSingleton<INameIpValidator, NameIpValidator>();

            // Register IMacAddressHelper
            services.AddSingleton<IMacAddressHelper, MacAddressHelper>();

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

            // Register specific caches using their interfaces
            services.AddSingleton<IMonitorCache, MonitorCache>();
            services.AddSingleton<IWakeOnLanResultCache, WakeOnLanResultCache>();

            // Register ResultManager
            services.AddSingleton<IResultManager>(provider =>
            {
                var resultCache = provider.GetRequiredService<IWakeOnLanResultCache>();
                return new ResultManager(resultCache);
            });

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
                    provider.GetRequiredService<IMonitorCache>(),
                    provider.GetRequiredService<IMonitorTask>(),
                    maxConcurrentTasks: 5,
                    intervalInSeconds: 10);

                return monitorService;
            });

            return services;
        }

        public static IServiceCollection AddRunspaceServices(this IServiceCollection services)
        {
            services.AddSingleton<IRunspaceProvider, RunspaceProvider>();
            services.AddSingleton<IRunspaceManager, RunspaceManager>();

            // Register IRequestQueue with a default maxConcurrency value
            services.AddSingleton<IRequestQueue>(provider =>
                new RequestQueue(maxConcurrency: 5));

            return services;
        }

        public static IServiceCollection AddConfigOptions(this IServiceCollection services)
        {
            services.Configure<WakeOnLanConfiguration>(options =>
            {
                options.DefaultPort = 9;
                options.MaxPingAttempts = 5;
                options.DefaultTimeoutInSeconds = 60;
                options.RunspacePoolMinThreads = 1;
                options.RunspacePoolMaxThreads = 5;
            });

            return services;
        }

        public static IServiceCollection AddWakeOnLanServices(this IServiceCollection services)
        {

            //Set Config Options
            services.AddConfigOptions();

            // Register Validators
            services.AddValidators();

            // Register Caches
            services.AddCaches();

            // Register Monitoring Services
            services.AddMonitoringServices();

            // Register Runspace Services
            services.AddRunspaceServices();

            // Register Builder Services
            services.AddSingleton<IScriptBuilder, ScriptBuilder>();

            // Register Remote PowerShell Executor
            services.AddSingleton<IRemotePowerShellExecutor, RemotePowerShellExecutor>();

            // Register Proxy Request Processor
            services.AddSingleton<IProxyRequestProcessor, ProxyRequestProcessor>();


            // Register Other Services
            services.AddSingleton<IMagicPacketSender, ProxyMagicPacketSender>();
            services.AddSingleton<IComputerFactory, ComputerFactory>();

            // Register WakeOnLanService
            services.AddSingleton<IWakeOnLanService, WakeOnLanService>();

            return services;
        }
    }
}

