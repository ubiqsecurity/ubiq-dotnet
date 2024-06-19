using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UbiqSecurity.Model
{
    internal class FfsRecord
	{
		[JsonProperty("encryption_algorithm")]
		public string EncryptionAlgorithm { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("regex")]
		public string Regex { get; set; }

		[JsonProperty("tweak_source")]
		public string TweakSource { get; set; }

		[JsonProperty("min_input_length")]
		public int MinInputLength { get; set; }

		[JsonProperty("max_input_length")]
		public int MaxInputLength { get; set; }

		[JsonProperty("fpe_definable")]
		public bool FpeDefinable { get; set; }

		[JsonProperty("input_character_set")]
		public string InputCharacters { get; set; }

		[JsonProperty("output_character_set")]
		public string OutputCharacters { get; set; }

		[JsonProperty("passthrough")]
		public string PassthroughCharacters { get; set; }

		[JsonProperty("msb_encoding_bits")]
		public long? MsbEncodingBits { get; set; }

		[JsonProperty("tweak_min_len")]
		public long TweakMinLength { get; set; }

		[JsonProperty("tweak_max_len")]
		public long TweakMaxLength { get; set; }

		[JsonProperty("tweak")]
		public string Tweak { get; set; }

        [JsonProperty("passthrough_rules")]
        public IEnumerable<PassthroughRuleDto> PassthroughRules { get; set; }
	}
}
