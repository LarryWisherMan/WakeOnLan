using System.Text.RegularExpressions;

namespace WakeOnLanLibrary.Helpers
{
    public static class MacAddressHelper
    {
        private static readonly Regex MacRegex = new Regex(
            "^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$",
            RegexOptions.Compiled);

        public static bool IsValidMacAddress(string macAddress)
        {
            return !string.IsNullOrEmpty(macAddress) && MacRegex.IsMatch(macAddress);
        }
    }
}
