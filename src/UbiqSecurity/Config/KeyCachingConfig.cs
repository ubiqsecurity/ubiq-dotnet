using Newtonsoft.Json;

namespace UbiqSecurity.Config
{
    public class KeyCachingConfig
    {
        [JsonProperty("unstructured")]
        public bool Unstructured { get; set; } = true;

        [JsonProperty("structured")]
        public bool Structured { get; set; } = true;

        [JsonProperty("encrypt")]
        public bool Encrypt { get; set; }

        [JsonProperty("ttl_seconds")]
        public int TtlSeconds { get; set; } = 1800;
    }
}
