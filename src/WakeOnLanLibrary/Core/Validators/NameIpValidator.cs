using System.Net;
using System.Text.RegularExpressions;
using WakeOnLanLibrary.Application.Interfaces.Validation;

public class NameIpValidator : INameIpValidator
{
    private static readonly Regex ComputerNameRegex = new Regex(
        @"^[a-zA-Z0-9\-]{1,63}$", // Matches valid hostname characters (alphanumeric and hyphen)
        RegexOptions.Compiled);

    /// <summary>
    /// Validates whether the input is a valid computer name.
    /// </summary>
    /// <param name="name">The computer name to validate.</param>
    /// <returns>True if the name is valid, otherwise false.</returns>
    public bool IsValidComputerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        // Perform a regex match to check for valid characters
        return ComputerNameRegex.IsMatch(name);
    }

    /// <summary>
    /// Validates whether the input is a valid IPv4 or IPv6 address.
    /// </summary>
    /// <param name="ipAddress">The IP address to validate.</param>
    /// <returns>True if the IP address is valid, otherwise false.</returns>
    public bool IsValidIpAddress(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
            return false;

        return IPAddress.TryParse(ipAddress, out _);
    }
}

