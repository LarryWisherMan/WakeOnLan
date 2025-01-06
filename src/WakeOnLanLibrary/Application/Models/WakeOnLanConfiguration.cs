namespace WakeOnLanLibrary.Application.Models
{
    public class WakeOnLanConfiguration
    {
        public int DefaultPort { get; set; } = 9;
        public int MaxPingAttempts { get; set; } = 5;
        public int DefaultTimeoutInSeconds { get; set; } = 60;

        public int RunspacePoolMinThreads { get; set; } = 1;
        public int RunspacePoolMaxThreads { get; set; } = 5;
    }

}
