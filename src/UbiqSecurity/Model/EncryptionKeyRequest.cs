using Newtonsoft.Json;

namespace UbiqSecurity.Model
{
    internal class EncryptionKeyRequest : IPayloadEncryptionRequest
    {
        public EncryptionKeyRequest()
        {
        }

        public EncryptionKeyRequest(int uses)
		{
            Uses = uses;
		}

		[JsonProperty("uses")]
		public int Uses { get; set; }

        [JsonProperty("payload_cert")]
        public string PayloadCertificate { get; set; }
    }
}
