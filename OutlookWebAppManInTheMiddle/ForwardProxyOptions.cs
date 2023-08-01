#nullable disable

namespace OutlookWebAppManInTheMiddle
{
    public class ForwardProxyOptions
    {
        public const string ForwardProxy = "ForwardProxy";

        public string Host { get; set; }
        public string Port { get; set; }

        public int PortInt => int.TryParse(Port, out var portInt) ? portInt : 0;

        public bool IsValid => string.IsNullOrWhiteSpace(Host) is false && PortInt is not 0;
    }
}
