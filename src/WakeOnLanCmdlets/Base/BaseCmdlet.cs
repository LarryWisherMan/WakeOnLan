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

        protected override void BeginProcessing()
        {
            base.BeginProcessing();

            try
            {
                // Retrieve the Wake-on-LAN service from the container
                WolService = ServiceContainer.Instance.GetService<IWakeOnLanService>();
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
    }
}
