namespace WakeOnLanLibrary.Models
{
    public abstract class Computer
    {
        public string Name { get; set; }

        public string MacAddress { get; set; }

        public string Status { get; set; }

        public string IpAddress { get; set; }
    }
}
