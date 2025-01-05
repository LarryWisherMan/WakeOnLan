using System.Collections.Generic;
using WakeOnLanLibrary.Application.Models;

namespace WakeOnLanLibrary.Application.Interfaces
{
    public interface IResultManager
    {
        /// <summary>
        /// Adds or updates a success result in the result cache.
        /// </summary>
        /// <param name="requests">The list of WakeOnLan requests.</param>
        void AddSuccessResults(IEnumerable<WakeOnLanRequest> requests);

        /// <summary>
        /// Adds or updates a failure result in the result cache.
        /// </summary>
        /// <param name="proxyComputerName">The name of the proxy computer.</param>
        /// <param name="targets">The list of target computers and their MAC addresses.</param>
        /// <param name="port">The port used for the request.</param>
        /// <param name="errorMessage">The error message describing the failure.</param>
        void AddFailureResults(string proxyComputerName, List<(string MacAddress, string ComputerName)> targets, int port, string errorMessage);

        /// <summary>
        /// Gets all results from the result cache.
        /// </summary>
        /// <returns>A collection of all results.</returns>
        IEnumerable<WakeOnLanReturn> GetAllResults();

        /// <summary>
        /// Updates the monitoring result for a specific computer.
        /// </summary>
        /// <param name="computerName">The name of the computer being monitored.</param>
        /// <param name="success">Indicates whether the monitoring was successful.</param>
        /// <param name="errorMessage">The error message if monitoring failed.</param>
        void UpdateMonitoringResult(string computerName, bool success, string errorMessage = null);
    }
}
