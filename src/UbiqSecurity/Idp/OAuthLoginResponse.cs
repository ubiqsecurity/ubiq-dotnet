using Newtonsoft.Json;

namespace UbiqSecurity.Idp
{
    internal class OAuthLoginResponse
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresInSeconds { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}
