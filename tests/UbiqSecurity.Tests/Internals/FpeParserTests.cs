using UbiqSecurity.Internals;
using UbiqSecurity.Model;

namespace UbiqSecurity.Tests.Internals
{
    public class FpeParserTests
    {

        [Fact]
        public void ParseInput_PassthroughSuffixPrefix_ReturnsExpectedModel()
        {
            var plaintText = "--0123-4--0--5-67890--";

            var dataset = new FfsRecord()
            {
                InputCharacters = "0123456789",
                OutputCharacters = "0123456789",
                PassthroughCharacters = "-",
                PassthroughRules = new List<PassthroughRuleDto>()
                {
                    new()
                    {
                        Priority = 1,
                        RuleType = PassthroughRuleType.Passthrough,
                        Value = "-",
                    },
                    new()
                    {
                        Priority = 2,
                        RuleType = PassthroughRuleType.Suffix,
                        Value = "6",
                    },
                    new()
                    {
                        Priority = 3,
                        RuleType = PassthroughRuleType.Prefix,
                        Value = "5",
                    },
                }
            };

            var result = FpeParser.Parse(plaintText, dataset, false);

            Assert.Equal("0", result.Trimmed);
            Assert.Equal("--0--", result.StringTemplate);
            Assert.Equal("--0123-4", result.Prefix);
            Assert.Equal("5-67890--", result.Suffix);
        }

        [Fact]
        public void ParseInput_PrefixPassthrough_ReturnsExpectedModel()
        {
            var plaintText = "123-45-6789";

            var dataset = new FfsRecord()
            {
                InputCharacters = "0123456789",
                OutputCharacters = "0123456789",
                PassthroughCharacters = "-",
                PassthroughRules = new List<PassthroughRuleDto>()
                {
                    new()
                    {
                        Priority = 2,
                        RuleType = PassthroughRuleType.Passthrough,
                        Value = "-",
                    },
                    new()
                    {
                        Priority = 1,
                        RuleType = PassthroughRuleType.Prefix,
                        Value = "4",
                    },
                }
            };

            var result = FpeParser.Parse(plaintText, dataset, false);

            Assert.Equal("456789", result.Trimmed);
            Assert.Equal("00-0000", result.StringTemplate);
            Assert.Equal("123-", result.Prefix);
        }
    }
}
