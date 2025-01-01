using Microsoft.Extensions.DependencyInjection;
using System;
using System.Management.Automation;
using WakeOnLanLibrary.Shared.Extensions;

namespace WakeOnLanCmdlets.LifeCycle
{
    public class ModuleInitializer : IModuleAssemblyInitializer, IModuleAssemblyCleanup
    {
        // Initialize logic when the module is loaded
        public void OnImport()
        {
            Console.WriteLine("WakeOnLan module loaded.");

            // Initialize the service container
            var serviceCollection = new ServiceCollection();

            // Use the extension method to add services from WakeOnLanLibrary
            serviceCollection.AddWakeOnLanServices();

            // Build the service provider and set it in the service container
            ServiceContainer.Initialize(serviceCollection);

            Console.WriteLine("Service container initialized.");
        }

        // Cleanup logic when the module is removed
        public void OnRemove(PSModuleInfo psModuleInfo)
        {


            // Dispose of the service provider if needed
            if (ServiceContainer.Instance is IDisposable disposable)
            {
                disposable.Dispose();
            }

            Console.WriteLine("WakeOnLan module removed and services disposed.");
        }
    }
}
