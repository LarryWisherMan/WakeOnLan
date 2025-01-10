using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace WakeOnLanLibrary.Core.Interfaces
{
    public interface IRunspacePool : IDisposable
    {
        // Removed ApartmentState
        TimeSpan CleanupInterval { get; set; }
        RunspaceConnectionInfo ConnectionInfo { get; }
        InitialSessionState InitialSessionState { get; }
        Guid InstanceId { get; }
        bool IsDisposed { get; }
        RunspacePoolAvailability RunspacePoolAvailability { get; }
        RunspacePoolStateInfo RunspacePoolStateInfo { get; }
        PSThreadOptions ThreadOptions { get; set; }

        IAsyncResult BeginClose(AsyncCallback callback, object state);
        IAsyncResult BeginConnect(AsyncCallback callback, object state);
        IAsyncResult BeginDisconnect(AsyncCallback callback, object state);
        IAsyncResult BeginOpen(AsyncCallback callback, object state);

        void Close();
        void Connect();
        PowerShell[] CreateDisconnectedPowerShells();
        void Disconnect();
        void Dispose();
        void EndClose(IAsyncResult asyncResult);
        void EndConnect(IAsyncResult asyncResult);
        void EndDisconnect(IAsyncResult asyncResult);
        void EndOpen(IAsyncResult asyncResult);
        PSPrimitiveDictionary GetApplicationPrivateData();
        int GetAvailableRunspaces();
        RunspacePoolCapability GetCapabilities();
        int GetMaxRunspaces();
        int GetMinRunspaces();
        void Open();
        void SetMaxRunspaces(int maxRunspaces);
        void SetMinRunspaces(int minRunspaces);

        event EventHandler<RunspacePoolStateChangedEventArgs> StateChanged;

        //Custom Members
        RunspacePoolState GetRunspaceState();
    }

}
