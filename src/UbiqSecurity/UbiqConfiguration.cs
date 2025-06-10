using Newtonsoft.Json;
using UbiqSecurity.Config;

namespace UbiqSecurity
{
    public class UbiqConfiguration
	{
        [JsonProperty("idp")]
        public IdpConfig Idp { get; set; }

        [JsonProperty("event_reporting")]
        public EventReportingConfig EventReporting { get; set; } = new EventReportingConfig();

        [JsonProperty("key_caching")]
        public KeyCachingConfig KeyCaching { get; set; } = new KeyCachingConfig();
	}
}
