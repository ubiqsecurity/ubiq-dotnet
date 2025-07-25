using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace UbiqSecurity.Config
{
    public enum IdpProvider
    {
        [EnumMember(Value = "entra")]
        MicrosoftEntraId,

        [EnumMember(Value = "okta")]
        Okta,
    }

    public class IdpConfig
    {
        public IdpConfig()
        {
        }

        [JsonProperty("provider")]
        public IdpProvider Provider { get; set; }

        [JsonProperty("idp_token_endpoint_url")]
        public string IdpTokenEndpointUrl { get; set; }

        [JsonProperty("idp_client_secret")]
        public string IdpClientSecret { get; set; }

        [JsonProperty("idp_tenant_id")]
        public string IdpTenantId { get; set; }

        [JsonProperty("ubiq_customer_id")]
        public string UbiqCustomerId { get; set; }
    }
}
