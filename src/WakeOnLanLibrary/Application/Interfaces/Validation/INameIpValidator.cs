namespace WakeOnLanLibrary.Application.Interfaces.Validation
{
    public interface INameIpValidator
    {
        /// <summary>
        /// Validates whether the provided string is a valid computer name.
        /// </summary>
        /// <param name="name">The computer name to validate.</param>
        /// <returns>True if the name is valid, otherwise false.</returns>
        bool IsValidComputerName(string name);

        /// <summary>
        /// Validates whether the provided string is a valid IP address (IPv4 or IPv6).
        /// </summary>
        /// <param name="ipAddress">The IP address to validate.</param>
        /// <returns>True if the IP address is valid, otherwise false.</returns>
        bool IsValidIpAddress(string ipAddress);
    }


}
