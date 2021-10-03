using Newtonsoft.Json;

namespace UbiqSecurity.Model
{
	public class FfsConfigurationResponse
	{
		#region Serializable Properties

		[JsonProperty("encryption_algorithm")]
		public string EncryptionAlgorithm { get; set; }

		[JsonProperty("input_character_set")]
		public string InputCharacters { get; set; }

		[JsonProperty("msb_encoding_bits")]
		public long? MsbEncodingBits { get; set; }

		[JsonProperty("max_input_length")]
		public int MaxInputLength { get; set; }

		[JsonProperty("min_input_length")]
		public int MinInputLength { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("output_character_set")]
		public string OutputCharacters { get; set; }

		[JsonProperty("passthrough")]
		public string Passthrough { get; set; }

		[JsonProperty("salt")]
		public string Salt { get; set; }

		[JsonProperty("tweak")]
		public string Tweak { get; set; }

		[JsonProperty("tweak_max_len")]
		public long? TweakMaxLength { get; set; }

		[JsonProperty("tweak_min_len")]
		public long? TweakMinLength { get; set; }

		[JsonProperty("tweak_source")]
		public string TweakSource { get; set; }

		#endregion
	}
}
