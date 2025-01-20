using System;
using System.Management.Automation.Runspaces;

namespace WakeOnLanLibrary.Core.Interfaces
{
    public interface IRunspace : IDisposable
    {
        /// <summary>
        /// Gets the current state of the runspace.
        /// </summary>
        RunspaceState State { get; }

        /// <summary>
        /// Opens the runspace, establishing the connection.
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the runspace, terminating the connection.
        /// </summary>
        void Close();
    }

}
