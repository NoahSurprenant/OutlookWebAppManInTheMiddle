#nullable disable

namespace OutlookWebAppManInTheMiddle
{
    public class ForwardProxyOptions
    {
        public const string ForwardProxy = "ForwardProxy";

        public string Host { get; set; }
        public int Port { get; set; }

        public bool IsValid => string.IsNullOrWhiteSpace(Host) is false && Port is not 0;
    }
}
