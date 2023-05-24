using Newtonsoft.Json;

namespace UbiqSecurity.Model
{
	internal class SecurityModel
    {
        [JsonProperty("algorithm")]
        public string Algorithm { get; set; }

        [JsonProperty("enable_data_fragmentation")]
        public bool EnableDataFragmentation { get; set; }
    }
}
