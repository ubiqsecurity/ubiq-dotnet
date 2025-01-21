using Newtonsoft.Json;

namespace UbiqSecurity.Model
{
    internal class DecryptionKeyRequest : IPayloadEncryptionRequest
    {
        public DecryptionKeyRequest()
        {
        }

        public DecryptionKeyRequest(string encryptedDataKey)
		{
            EncryptedDataKey = encryptedDataKey;
		}

		[JsonProperty("encrypted_data_key")]
		public string EncryptedDataKey { get; set; }

        [JsonProperty("payload_cert")]
        public string PayloadCertificate { get; set; }
    }
}
