using UbiqSecurity.Internals.Structured.Pipeline;
using UbiqSecurity.Internals.Structured.Pipeline.Operations;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity.Tests.Internals.Structured.Pipeline.Operations
{
    public class ExpandPassthroughPrefixOperationTests
    {
        internal readonly OperationContext TestContext = new()
        {
            CurrentValue = "123",
            Dataset = new FfsRecord
            {
                PassthroughRules = new List<PassthroughRuleDto>()
                    {
                        new() { RuleType =  PassthroughRuleType.Prefix, Value = "3" },
                    },
            },
            Data = new Dictionary<string, string>
            {
                { "Prefix", "abc" },
            },
        };

        [Fact]
        public async Task InvokeAsync_NoPrefixRule_ReturnsCurrentValue()
        {
            TestContext.Dataset.PassthroughRules = new List<PassthroughRuleDto>();

            var sut = new ExpandPassthroughPrefixOperation();

            var result = await sut.InvokeAsync(TestContext);

            Assert.Equal("123", result);
        }

        [Fact]
        public async Task InvokeAsync_NoPrefix_ReturnsCurrentValue()
        {
            TestContext.Data.Remove("Prefix");

            var sut = new ExpandPassthroughPrefixOperation();

            var result = await sut.InvokeAsync(TestContext);

            Assert.Equal("123", result);
        }

        [Fact]
        public async Task InvokeAsync_PrefixExists_ReturnsCurrentValueWithPrefix()
        {
            var sut = new ExpandPassthroughPrefixOperation();

            var result = await sut.InvokeAsync(TestContext);

            Assert.Equal("abc123", result);
        }
    }
}
