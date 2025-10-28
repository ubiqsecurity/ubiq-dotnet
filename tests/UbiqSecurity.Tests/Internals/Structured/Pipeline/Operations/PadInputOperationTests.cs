using UbiqSecurity.Internals.Structured.Pipeline;
using UbiqSecurity.Internals.Structured.Pipeline.Operations;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity.Tests.Internals.Structured.Pipeline.Operations
{
    public class PadInputOperationTests
    {
        internal readonly OperationContext TestContext = new()
        {
            OriginalValue = "abc-123",
            CurrentValue = "654zyx",
            Dataset = new FfsRecord
            {
                InputCharacters = "abc123",
                OutputCharacters = "xyz456",
                InputPadCharacter = "*",
                MinInputLength = 10,
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

        //[Fact]
        //public async Task InvokeAsync_NullInputPadCharacter_ReturnsCurrentValue()
        //{
        //    TestContext.Dataset.MinInputLength = 10;
        //    TestContext.Dataset.InputPadCharacter = null;

        //    var sut = new PadInputOperation();

        //    var result = await sut.InvokeAsync(TestContext);

        //    Assert.Equal("654zyx", result);
        //}

        //[Fact]
        //public async Task InvokeAsync_PadLengthLessThanCurrentValueLength_ReturnsCurrentValue()
        //{
        //    TestContext.Dataset.MinInputLength = 5;
        //    TestContext.Dataset.InputPadCharacter = "*";

        //    var sut = new PadInputOperation();

        //    var result = await sut.InvokeAsync(TestContext);

        //    Assert.Equal("654zyx", result);
        //}

        [Fact]
        public async Task InvokeAsync_MinInputLengthGreaterThanCurrentValueLength_ReturnsPaddedString()
        {
            TestContext.Dataset.MinInputLength = 10;
            TestContext.Dataset.InputPadCharacter = "*";

            var sut = new PadInputOperation();

            var result = await sut.InvokeAsync(TestContext);

            Assert.Equal("****654zyx", result);
        }

        [Fact]
        public async Task InvokeAsync_PadLengthGreaterThanCurrentValueLength_PadsPassthroughTemplate()
        {
            TestContext.Dataset.MinInputLength = 10;
            TestContext.Dataset.InputPadCharacter = "*";

            var sut = new PadInputOperation();

            await sut.InvokeAsync(TestContext);

            Assert.Equal("***xxx-xxx", TestContext.Data["PassthroughTemplate"]);
        }
    }
}
