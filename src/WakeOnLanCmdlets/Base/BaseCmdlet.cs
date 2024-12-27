using System.Management.Automation;
using WakeOnLanCmdlets.factories;


namespace WakeOnLanCmdlets.Base
{
    public abstract class BaseCmdlet : PSCmdlet
    {
        protected PersistentWolService WolService { get; private set; }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            // Initialize or retrieve the PersistentWolService
            WolService = PersistentWolServiceFactory.GetOrCreateService();
        }
    }
}
