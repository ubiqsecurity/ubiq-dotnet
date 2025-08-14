using UbiqSecurity.Internals.Structured.Pipeline;
using UbiqSecurity.Internals.Structured.Pipeline.Operations;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity.Tests.Internals.Structured.Pipeline.Operations
{
    public class ExpandPassthroughSuffixOperationTests
    {
        [Fact]
        public async Task InvokeAsync_NoSuffix_ReturnsCurrentValue()
        {
            var context = new OperationContext()
            {
                CurrentValue = "abc",
                Dataset = new FfsRecord
                {
                    PassthroughRules = new List<PassthroughRuleDto>(),
                },
            };

            var sut = new ExpandPassthroughSuffixOperation();

            var result = await sut.InvokeAsync(context);

            Assert.Equal("abc", result);
        }

        [Fact]
        public async Task InvokeAsync_PrefixExists_ReturnsCurrentValueWithPrefix()
        {
            var context = new OperationContext()
            {
                CurrentValue = "abc",
                Dataset = new FfsRecord
                {
                    PassthroughRules = new List<PassthroughRuleDto>()
                    {
                        new() { RuleType =  PassthroughRuleType.Suffix, Value = "3" },
                    },
                },
                Data = new Dictionary<string, string>
                {
                    { "Suffix", "123" },
                },
            };

            var sut = new ExpandPassthroughSuffixOperation();

            var result = await sut.InvokeAsync(context);

            Assert.Equal("abc123", result);
        }
    }
}
