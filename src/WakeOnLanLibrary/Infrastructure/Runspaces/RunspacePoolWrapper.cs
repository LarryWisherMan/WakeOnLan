using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using WakeOnLanLibrary.Core.Interfaces;

namespace WakeOnLanLibrary.Infrastructure.Runspaces
{
    public class RunspacePoolWrapper : IRunspacePool
    {
        public RunspacePool RunspacePool => _runspacePool;
        private readonly RunspacePool _runspacePool;

        public RunspacePoolWrapper(RunspacePool runspacePool)
        {
            _runspacePool = runspacePool ?? throw new ArgumentNullException(nameof(runspacePool));
        }

        public TimeSpan CleanupInterval
        {
            get => _runspacePool.CleanupInterval;
            set => _runspacePool.CleanupInterval = value;
        }
        public RunspaceConnectionInfo ConnectionInfo => _runspacePool.ConnectionInfo;
        public InitialSessionState InitialSessionState => _runspacePool.InitialSessionState;
        public Guid InstanceId => _runspacePool.InstanceId;
        public bool IsDisposed => _runspacePool.IsDisposed;
        public RunspacePoolAvailability RunspacePoolAvailability => _runspacePool.RunspacePoolAvailability;
        public RunspacePoolStateInfo RunspacePoolStateInfo => _runspacePool.RunspacePoolStateInfo;
        public PSThreadOptions ThreadOptions
        {
            get => _runspacePool.ThreadOptions;
            set => _runspacePool.ThreadOptions = value;
        }

        public IAsyncResult BeginClose(AsyncCallback callback, object state) => _runspacePool.BeginClose(callback, state);
        public IAsyncResult BeginConnect(AsyncCallback callback, object state) => _runspacePool.BeginConnect(callback, state);
        public IAsyncResult BeginDisconnect(AsyncCallback callback, object state) => _runspacePool.BeginDisconnect(callback, state);
        public IAsyncResult BeginOpen(AsyncCallback callback, object state) => _runspacePool.BeginOpen(callback, state);

        public void Close() => _runspacePool.Close();
        public void Connect() => _runspacePool.Connect();
        public PowerShell[] CreateDisconnectedPowerShells()
        {
            return _runspacePool.CreateDisconnectedPowerShells().ToArray();
        }

        public void Disconnect() => _runspacePool.Disconnect();
        public void Dispose() => _runspacePool.Dispose();
        public void EndClose(IAsyncResult asyncResult) => _runspacePool.EndClose(asyncResult);
        public void EndConnect(IAsyncResult asyncResult) => _runspacePool.EndConnect(asyncResult);
        public void EndDisconnect(IAsyncResult asyncResult) => _runspacePool.EndDisconnect(asyncResult);
        public void EndOpen(IAsyncResult asyncResult) => _runspacePool.EndOpen(asyncResult);
        public PSPrimitiveDictionary GetApplicationPrivateData() => _runspacePool.GetApplicationPrivateData();
        public int GetAvailableRunspaces() => _runspacePool.GetAvailableRunspaces();
        public RunspacePoolCapability GetCapabilities() => _runspacePool.GetCapabilities();
        public int GetMaxRunspaces() => _runspacePool.GetMaxRunspaces();
        public int GetMinRunspaces() => _runspacePool.GetMinRunspaces();
        public void Open() => _runspacePool.Open();
        public void SetMaxRunspaces(int maxRunspaces) => _runspacePool.SetMaxRunspaces(maxRunspaces);
        public void SetMinRunspaces(int minRunspaces) => _runspacePool.SetMinRunspaces(minRunspaces);

        public event EventHandler<RunspacePoolStateChangedEventArgs> StateChanged
        {
            add => _runspacePool.StateChanged += value;
            remove => _runspacePool.StateChanged -= value;
        }
    }

}
