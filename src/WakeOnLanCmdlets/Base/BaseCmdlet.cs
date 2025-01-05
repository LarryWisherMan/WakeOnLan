using Microsoft.Extensions.DependencyInjection;
using System;
using System.Management.Automation;
using WakeOnLanCmdlets.LifeCycle;
using WakeOnLanLibrary.Application.Interfaces;

namespace WakeOnLanCmdlets.Base
{
    public abstract class BaseCmdlet : PSCmdlet
    {
        protected IWakeOnLanService WolService { get; private set; }
        protected IServiceProvider ServiceProvider { get; private set; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            try
            {
                // Retrieve the service provider
                ServiceProvider = ServiceContainer.Instance.GetService<IServiceProvider>();
                if (ServiceProvider == null)
                {
                    throw new InvalidOperationException("The service provider is not registered.");
                }

                // Retrieve the Wake-on-LAN service
                WolService = ServiceProvider.GetService<IWakeOnLanService>();
                if (WolService == null)
                {
                    throw new InvalidOperationException("The Wake-on-LAN service is not registered.");
                }
            }
            catch (Exception ex)
            {
                // Handle initialization errors gracefully
                WriteError(new ErrorRecord(
                    ex,
                    "ServiceInitializationFailed",
                    ErrorCategory.InvalidOperation,
                    targetObject: null
                ));

                // Prevent further processing
                StopProcessing();
            }
        }

        /// <summary>
        /// Resolves a service of the specified type from the service provider.
        /// </summary>
        /// <typeparam name="T">The type of service to resolve.</typeparam>
        /// <returns>The resolved service instance.</returns>
        protected T ResolveService<T>() where T : class
        {
            var service = ServiceProvider.GetService<T>();
            if (service == null)
            {
                throw new InvalidOperationException($"The service of type {typeof(T).Name} is not registered.");
            }

            return service;
        }
    }
}
