using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UbiqSecurity.Internals.WebService.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    internal enum PassthroughRuleType
    {
        [EnumMember(Value = "none")]
        None,

        [EnumMember(Value = "passthrough")]
        Passthrough,

        [EnumMember(Value = "prefix")]
        Prefix,

        [EnumMember(Value = "suffix")]
        Suffix,
    }

    internal class PassthroughRuleDto
    {
        [JsonProperty("priority")]
        public int Priority { get; set; }

        [JsonProperty("type")]
        public PassthroughRuleType RuleType { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
