using Newtonsoft.Json;

namespace UbiqSecurity.Internals.Idp
{
    internal class SsoRequest
    {
        [JsonIgnore]
        public string AccessToken { get; set; }

        [JsonProperty("csr")]
        public string Csr { get; set; }
    }
}
