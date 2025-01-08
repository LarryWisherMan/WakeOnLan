namespace WakeOnLanLibrary.Application.Interfaces.Validation
{
    public interface IMacAddressHelper
    {
        bool IsValidMacAddress(string macAddress);
        byte[] ParseMacAddress(string macAddress);
    }
}
