using System;
using Newtonsoft.Json;

namespace UbiqSecurity.Model
{
	internal class EncryptionKeyResponse : IEncryptedPrivateKeyModel
    {
        [JsonProperty("encrypted_private_key")]
        public string EncryptedPrivateKey { get; set; }

        [JsonProperty("encryption_session")]
        public string EncryptionSession { get; set; }

        [JsonProperty("key_fingerprint")]
        public string KeyFingerprint { get; set; }

        [JsonProperty("security_model")]
        public SecurityModel SecurityModel { get; set; }

        [JsonProperty("max_uses")]
        public int MaxUses { get; set; }

        [JsonProperty("wrapped_data_key")]
        public string WrappedDataKey { get; set; }

        [JsonProperty("encrypted_data_key")]
        public string EncryptedDataKey { get; set; }

        [JsonIgnore]
        public int Uses { get; set; }

        [JsonIgnore]
        public byte[] UnwrappedDataKey { get; set; }

        [JsonIgnore]
        public byte[] EncryptedDataKeyBytes => Convert.FromBase64String(EncryptedDataKey);
    }
}
