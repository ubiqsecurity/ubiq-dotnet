using Newtonsoft.Json;

namespace UbiqSecurity.Internals.WebService.Models
{
    internal class FpeKeyRequest : IPayloadEncryptionRequest
    {
        [JsonProperty("payload_cert")]
        public string PayloadCertificate { get; set; }
    }
}
