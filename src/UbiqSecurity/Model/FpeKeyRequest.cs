using Newtonsoft.Json;

namespace UbiqSecurity.Model
{
    internal class FpeKeyRequest : IPayloadEncryptionRequest
    {
        [JsonProperty("payload_cert")]
        public string PayloadCertificate { get; set; }
    }
}
