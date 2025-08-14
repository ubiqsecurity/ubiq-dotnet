using Newtonsoft.Json;

namespace UbiqSecurity.Internals.WebService.Models
{
    internal class DecryptionKeyResponse
    {
        [JsonProperty("encrypted_private_key")]
        public string EncryptedPrivateKey { get; set; }

        [JsonProperty("encryption_session")]
        public string EncryptionSession { get; set; }

        [JsonProperty("key_fingerprint")]
        public string KeyFingerprint { get; set; }

        [JsonProperty("wrapped_data_key")]
        public string WrappedDataKey { get; set; }

        [JsonIgnore]
        public byte[] UnwrappedDataKey { get; set; }

        [JsonIgnore]
        public int KeyUseCount { get; set; }

        [JsonIgnore]
        public byte[] LastCipherHeaderEncryptedDataKeyBytes { get; set; }
    }
}
