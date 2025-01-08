using System;
using System.Linq;
using System.Text.RegularExpressions;
using WakeOnLanLibrary.Application.Interfaces.Validation;

public class MacAddressValidator : IMacAddressHelper
{
    private static readonly Regex MacAddressRegex = new Regex(
        @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$",
        RegexOptions.Compiled);

    public bool IsValidMacAddress(string macAddress)
    {
        if (string.IsNullOrWhiteSpace(macAddress))
            return false;

        return MacAddressRegex.IsMatch(macAddress);
    }

    public byte[] ParseMacAddress(string macAddress)
    {
        if (!IsValidMacAddress(macAddress))
            throw new ArgumentException("Invalid MAC address format.", nameof(macAddress));

        return macAddress
            .Split(new[] { ':', '-' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(hex => Convert.ToByte(hex, 16))
            .ToArray();
    }
}
