using System.Management.Automation;
using System.Management.Automation.Runspaces;
using WakeOnLanLibrary.Core.Interfaces;

namespace WakeOnLanLibrary.Tests.TestObjects
{
    public class StubRunspacePool : IRunspacePool
    {
        public RunspacePoolStateInfo RunspacePoolStateInfo { get; set; } = new RunspacePoolStateInfo(RunspacePoolState.BeforeOpen, null);

        public void Dispose() { /* No-op */ }
        public void Open() => RunspacePoolStateInfo = new RunspacePoolStateInfo(RunspacePoolState.Opened, null);
        public void Close() => RunspacePoolStateInfo = new RunspacePoolStateInfo(RunspacePoolState.Closed, null);

        //CustomMembers
        public RunspacePoolState GetRunspaceState() => RunspacePoolStateInfo.State;


        // Implement other members as needed
        public TimeSpan CleanupInterval { get; set; }
        public RunspaceConnectionInfo ConnectionInfo => null;
        public InitialSessionState InitialSessionState => null;
        public Guid InstanceId => Guid.NewGuid();
        public bool IsDisposed => false;
        public RunspacePoolAvailability RunspacePoolAvailability => RunspacePoolAvailability.None;
        public PSThreadOptions ThreadOptions { get; set; }
        public IAsyncResult BeginClose(AsyncCallback callback, object state) => null;
        public IAsyncResult BeginConnect(AsyncCallback callback, object state) => null;
        public IAsyncResult BeginDisconnect(AsyncCallback callback, object state) => null;
        public IAsyncResult BeginOpen(AsyncCallback callback, object state) => null;
        public void Connect() { }
        public PowerShell[] CreateDisconnectedPowerShells() => Array.Empty<PowerShell>();
        public void Disconnect() { }
        public void EndClose(IAsyncResult asyncResult) { }
        public void EndConnect(IAsyncResult asyncResult) { }
        public void EndDisconnect(IAsyncResult asyncResult) { }
        public void EndOpen(IAsyncResult asyncResult) { }
        public PSPrimitiveDictionary GetApplicationPrivateData() => new PSPrimitiveDictionary();
        public int GetAvailableRunspaces() => 0;
        public RunspacePoolCapability GetCapabilities() => RunspacePoolCapability.Default;
        public int GetMaxRunspaces() => 1;
        public int GetMinRunspaces() => 1;
        public void SetMaxRunspaces(int maxRunspaces) { }
        public void SetMinRunspaces(int minRunspaces) { }
        public event EventHandler<RunspacePoolStateChangedEventArgs> StateChanged;
    }

}
