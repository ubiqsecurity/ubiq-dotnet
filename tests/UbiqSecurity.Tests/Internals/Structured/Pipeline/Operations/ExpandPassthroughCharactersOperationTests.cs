using UbiqSecurity.Internals.Structured.Pipeline;
using UbiqSecurity.Internals.Structured.Pipeline.Operations;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity.Tests.Internals.Structured.Pipeline.Operations
{
    public class ExpandPassthroughCharactersOperationTests
    {
        internal readonly OperationContext TestContext = new()
        {
            OriginalValue = "abc-123",
            CurrentValue = "654zyx",
            Dataset = new FfsRecord
            {
                InputCharacters = "abc123",
                OutputCharacters = "xyz456",
                PassthroughRules = new List<PassthroughRuleDto>
                {
                    new () { RuleType = PassthroughRuleType.Passthrough, Value = "-" },
                },
            },
            Data = new Dictionary<string, string>
            {
                { "PassthroughTemplate", "xxx-xxx" },
            },
        };

        [Fact]
        public async Task InvokeAsync_NoPassthroughRules_ReturnsCurrentValue()
        {
            TestContext.Dataset.PassthroughRules = new List<PassthroughRuleDto>();

            var sut = new ExpandPassthroughCharactersOperation();

            var result = await sut.InvokeAsync(TestContext);

            Assert.Equal("654zyx", result);
        }

        [Fact]
        public async Task InvokeAsync_NoPassthroughTemplate_ReturnsCurrentValue()
        {
            TestContext.Data.Remove("PassthroughTemplate");

            var sut = new ExpandPassthroughCharactersOperation();

            var result = await sut.InvokeAsync(TestContext);

            Assert.Equal("654zyx", result);
        }

        [Fact]
        public async Task InvokeAsync_ValidPassthroughTemplate_ReturnsFormattedValue()
        {
            var sut = new ExpandPassthroughCharactersOperation();

            var result = await sut.InvokeAsync(TestContext);

            Assert.Equal("654-zyx", result);
        }
    }
}
