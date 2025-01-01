using Microsoft.Extensions.DependencyInjection;
using System;
using WakeOnLanLibrary.Shared.Extensions;

namespace WakeOnLanLibrary.Infrastructure.Factories
{
    public class ServiceProviderFactory
    {
        private readonly IServiceCollection _services;

        public ServiceProviderFactory()
        {
            _services = new ServiceCollection();
        }

        /// <summary>
        /// Adds all WakeOnLan services to the service collection.
        /// </summary>
        public ServiceProviderFactory AddWakeOnLanServices()
        {
            _services.AddWakeOnLanServices();
            return this;
        }

        /// <summary>
        /// Adds custom services or overrides specific implementations.
        /// </summary>
        /// <param name="configureServices">Action to configure additional services.</param>
        public ServiceProviderFactory Configure(Action<IServiceCollection> configureServices)
        {
            configureServices?.Invoke(_services);
            return this;
        }

        /// <summary>
        /// Builds the service provider.
        /// </summary>
        /// <returns>An IServiceProvider with all registered services.</returns>
        public IServiceProvider Build()
        {
            return _services.BuildServiceProvider();
        }

        /// <summary>
        /// Retrieves a specific service from the built service provider.
        /// </summary>
        /// <typeparam name="T">Type of the service to resolve.</typeparam>
        /// <returns>The resolved service.</returns>
        public T GetService<T>()
        {
            var serviceProvider = Build();
            return serviceProvider.GetRequiredService<T>();
        }
    }
}
