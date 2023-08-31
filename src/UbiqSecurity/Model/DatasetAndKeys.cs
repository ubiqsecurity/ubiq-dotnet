using System.Collections.Generic;
using Newtonsoft.Json;

namespace UbiqSecurity.Model
{
	internal class DatasetAndKeys
	{
		[JsonProperty("ffs")]
		public FfsRecord Dataset { get; set; }

		[JsonProperty("encrypted_private_key")]
		public string EncryptedPrivateKey { get; set; }

		[JsonProperty("current_key_number")]
		public int CurrentKeyNumber { get; set; }

		[JsonProperty("keys")]
		public IEnumerable<string> Keys { get; set; }
	}
}
