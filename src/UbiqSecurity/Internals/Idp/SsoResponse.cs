using Newtonsoft.Json;

namespace UbiqSecurity.Internals.Idp
{
    internal class SsoResponse
    {
        [JsonProperty("public_value")]
        public string PublicValue { get; set; }

        [JsonProperty("signing_value")]
        public string SigningValue { get; set; }

        [JsonProperty("enabled")]
        public bool Enabled { get; set; }

        [JsonProperty("api_cert")]
        public string ApiCert { get; set; }
    }
}
