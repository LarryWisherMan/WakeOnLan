using System;
using System.Management.Automation.Runspaces;
using WakeOnLanLibrary.Core.Interfaces;

namespace WakeOnLanLibrary.Infrastructure.Adapters
{
    public class RunspaceWrapper : IRunspace
    {
        private readonly Runspace _runspace;

        /// <summary>
        /// Initializes a new instance of the <see cref="RunspaceWrapper"/> class.
        /// </summary>
        /// <param name="runspace">The underlying PowerShell runspace.</param>
        public RunspaceWrapper(Runspace runspace)
        {
            _runspace = runspace ?? throw new ArgumentNullException(nameof(runspace));
        }

        /// <inheritdoc />
        public RunspaceState State => _runspace.RunspaceStateInfo.State;

        /// <inheritdoc />
        public void Open()
        {
            _runspace.Open();
        }

        /// <inheritdoc />
        public void Close()
        {
            _runspace.Close();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _runspace.Dispose();
        }
    }

}
