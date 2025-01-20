using Microsoft.Extensions.DependencyInjection;
using System;
using WakeOnLanLibrary.Application.Interfaces;
using WakeOnLanLibrary.Application.Interfaces.Execution;
using WakeOnLanLibrary.Application.Interfaces.Helpers;
using WakeOnLanLibrary.Application.Interfaces.Validation;
using WakeOnLanLibrary.Application.Models;
using WakeOnLanLibrary.Application.Services;
using WakeOnLanLibrary.Application.Validation;
using WakeOnLanLibrary.Core.Entities;
using WakeOnLanLibrary.Core.Interfaces;
using WakeOnLanLibrary.Core.Interfaces.Validation;
using WakeOnLanLibrary.Core.UseCases;
using WakeOnLanLibrary.Core.Validation;
using WakeOnLanLibrary.Core.ValidationStrategies;
using WakeOnLanLibrary.Infrastructure.Builders;
using WakeOnLanLibrary.Infrastructure.Caching;
using WakeOnLanLibrary.Infrastructure.Execution;
using WakeOnLanLibrary.Infrastructure.Factories;
using WakeOnLanLibrary.Infrastructure.Helpers;
using WakeOnLanLibrary.Infrastructure.Monitoring;
using WakeOnLanLibrary.Infrastructure.Services;
using WakeOnLanLibrary.Infrastructure.ValidationStrategies;

namespace WakeOnLanLibrary.Shared.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all validators and their dependencies.
        /// </summary>
        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            // Validator Factory
            services.AddSingleton<IValidatorFactory, ValidatorFactory>();

            // Validation Strategies
            services.AddTransient<IValidationStrategy<Computer>, GeneralComputerValidationStrategy>();
            services.AddTransient<IValidationStrategy<TargetComputer>, TargetComputerValidationStrategy>();
            services.AddTransient<IValidationStrategy<ProxyComputer>, ProxyComputerValidationStrategy>();

            // Composite Validators
            AddCompositeValidators(services);

            // Computer Validator
            services.AddSingleton<IComputerValidator, ComputerValidator>();

            // Helpers
            services.AddSingleton<INameIpValidator, NameIpValidator>();
            services.AddSingleton<IMacAddressHelper, MacAddressValidator>();

            return services;
        }

        /// <summary>
        /// Registers composite validators for all supported computer types.
        /// </summary>
        private static void AddCompositeValidators(IServiceCollection services)
        {
            services.AddTransient(provider =>
                new CompositeValidator<Computer>(provider.GetServices<IValidationStrategy<Computer>>()));
            services.AddTransient(provider =>
                new CompositeValidator<TargetComputer>(provider.GetServices<IValidationStrategy<TargetComputer>>()));
            services.AddTransient(provider =>
                new CompositeValidator<ProxyComputer>(provider.GetServices<IValidationStrategy<ProxyComputer>>()));
        }

        /// <summary>
        /// Registers caching services and their dependencies.
        /// </summary>
        public static IServiceCollection AddCaches(this IServiceCollection services)
        {
            services.AddSingleton(typeof(ICache<,>), typeof(Cache<,>));
            services.AddSingleton<IMonitorCache, MonitorCache>();
            services.AddSingleton<IWakeOnLanResultCache, WakeOnLanResultCache>();

            // Result Manager
            services.AddSingleton<IResultManager>(provider =>
                new ResultManager(provider.GetRequiredService<IWakeOnLanResultCache>()));

            return services;
        }

        /// <summary>
        /// Registers monitoring services and their dependencies.
        /// </summary>
        public static IServiceCollection AddMonitoringServices(this IServiceCollection services)
        {
            services.AddSingleton<INetworkHelper, NetworkHelper>();
            services.AddSingleton<IMonitorTask, MonitorTask>();
            services.AddSingleton<IMonitorService>(provider =>
                new MonitorService(
                    provider.GetRequiredService<IMonitorCache>(),
                    provider.GetRequiredService<IMonitorTask>(),
                    maxConcurrentTasks: 5,
                    intervalInSeconds: 10));

            services.AddSingleton<IMonitoringManager>(provider =>
                new MonitoringManager(provider.GetRequiredService<IMonitorService>()));

            return services;
        }

        /// <summary>
        /// Registers runspace-related services and their dependencies.
        /// </summary>
        public static IServiceCollection AddRunspaceServices(this IServiceCollection services)
        {
            services.AddSingleton<IRunspaceProvider, RunspaceProvider>();
            services.AddSingleton<IRunspaceManager, RunspaceManager>();
            services.AddSingleton<IRequestScheduler>(provider =>
                new RequestScheduler(maxConcurrency: 5));

            return services;
        }

        /// <summary>
        /// Configures application options.
        /// </summary>
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

        public static IServiceCollection AddExecutionServices(this IServiceCollection services)
        {

            services.AddTransient<IPowerShell, PowerShellWrapper>();

            services.AddTransient<IPowerShellExecutor, PowerShellExecutor>();
            services.AddTransient<Func<IPowerShellExecutor>>(provider =>
            {
                return () => provider.GetRequiredService<IPowerShellExecutor>();
            });

            return services;
        }


        /// <summary>
        /// Registers all services required for the WakeOnLanLibrary.
        /// </summary>
        public static IServiceCollection AddWakeOnLanServices(this IServiceCollection services)
        {
            services.AddConfigOptions();
            services.AddValidators();
            services.AddCaches();
            services.AddMonitoringServices();
            services.AddRunspaceServices();

            // Other Services
            services.AddSingleton<IScriptBuilder, ScriptBuilder>();
            services.AddExecutionServices();
            services.AddSingleton<IRemotePowerShellExecutor, RemotePowerShellExecutor>();
            services.AddSingleton<IProxyRequestProcessor, ProxyRequestProcessor>();
            //services.AddSingleton<IRunspacePool, RunspacePoolWrapper>();
            //services.AddSingleton<IRunspace, RunspaceWrapper>();

            services.AddSingleton<ITaskRunner, TaskRunner>();
            services.AddSingleton<IMagicPacketSender, ProxyMagicPacketSender>();
            services.AddSingleton<IComputerFactory, ComputerFactory>();
            services.AddSingleton<IWakeOnLanService, WakeOnLanService>();

            return services;
        }
    }
}
