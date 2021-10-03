using Newtonsoft.Json;

namespace UbiqSecurity.Model
{
	public class FpeEncryptionKeyResponse : IEncryptedPrivateKeyModel
	{
		#region Serializable Properties

		[JsonProperty("encrypted_private_key")]
		public string EncryptedPrivateKey { get; set; }

		[JsonProperty("key_number")]
		public int KeyNumber { get; set; }

		[JsonProperty("wrapped_data_key")]
		public string WrappedDataKey { get; set; }

		#endregion

		#region Non-serialized Properties (runtime only)

		[JsonIgnore]
		public byte[] UnwrappedDataKey { get; set; }

		#endregion
	}
}
