namespace WakeOnLanLibrary.Application.Interfaces.Helpers
{
    public interface IMacAddressHelper
    {
        bool IsValidMacAddress(string macAddress);
        byte[] ParseMacAddress(string macAddress);
    }

}
