using System;
using System.Management.Automation;
using WakeOnLanCmdlets.factories;

namespace WakeOnLanCmdlets.LifeCycle
{
    public class ModuleInitializer : IModuleAssemblyInitializer, IModuleAssemblyCleanup
    {
        // Initialize logic when the module is loaded
        public void OnImport()
        {
            Console.WriteLine("WakeOnLan module loaded.");
        }

        // Cleanup logic when the module is removed
        public void OnRemove(PSModuleInfo psModuleInfo)
        {
            PersistentWolServiceFactory.Cleanup();
        }
    }

}
