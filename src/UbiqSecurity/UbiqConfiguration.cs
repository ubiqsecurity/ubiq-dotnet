using UbiqSecurity.Config;

namespace UbiqSecurity
{
    public class UbiqConfiguration
	{
        public IdpConfig Idp { get; set; }

        public EventReportingConfig EventReporting { get; set; } = new EventReportingConfig();

        public KeyCachingConfig KeyCaching { get; set; } = new KeyCachingConfig();
	}
}
