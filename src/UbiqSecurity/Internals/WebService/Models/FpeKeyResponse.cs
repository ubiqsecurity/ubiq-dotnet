using Newtonsoft.Json;

namespace UbiqSecurity.Internals.WebService.Models
{
    internal class FpeKeyResponse : IEncryptedPrivateKeyModel
    {
        [JsonProperty("encrypted_private_key")]
        public string EncryptedPrivateKey { get; set; }

        [JsonProperty("key_number")]
        public int KeyNumber { get; set; }

        [JsonProperty("wrapped_data_key")]
        public string WrappedDataKey { get; set; }

        [JsonIgnore]
        public byte[] UnwrappedDataKey { get; set; }
    }
}
