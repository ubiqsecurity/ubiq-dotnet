using UbiqSecurity.Internals.Structured.Pipeline;
using UbiqSecurity.Internals.Structured.Pipeline.Operations;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity.Tests.Internals.Structured.Pipeline.Operations
{
    public class TrimPassthroughPrefixOperationTests
    {
        [Fact]
        public async Task InvokeAsync_NoPassthroughRules_ReturnsCurrentValue()
        {
            var context = new OperationContext()
            {
                CurrentValue = "abc123",
                Dataset = new FfsRecord
                {
                    PassthroughRules = new List<PassthroughRuleDto>(),
                },
            };

            var sut = new TrimPassthroughPrefixOperation();

            var result = await sut.InvokeAsync(context);

            Assert.Equal("abc123", result);
        }

        [Fact]
        public async Task InvokeAsync_PrefixLengthZero_ReturnsCurrentValue()
        {
            var context = new OperationContext()
            {
                CurrentValue = "abc123",
                Dataset = new FfsRecord
                {
                    PassthroughRules = new List<PassthroughRuleDto>()
                    {
                        new() { RuleType =  PassthroughRuleType.Prefix, Value = "0" },
                    }
                }
            };

            var sut = new TrimPassthroughPrefixOperation();

            var result = await sut.InvokeAsync(context);

            Assert.Equal("abc123", result);
        }

        [Fact]
        public async Task InvokeAsync_PrefixLengthGreaterThanCurrentValueLength_ThrowsException()
        {
            var context = new OperationContext()
            {
                CurrentValue = "abc123",
                Dataset = new FfsRecord
                {
                    PassthroughRules = new List<PassthroughRuleDto>()
                    {
                        new() { RuleType =  PassthroughRuleType.Prefix, Value = "7" },
                    }
                }
            };

            var sut = new TrimPassthroughPrefixOperation();

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sut.InvokeAsync(context));
        }

        [Fact]
        public async Task InvokeAsync_PrefixLengthEqualToCurrentValueLength_ReturnsEmptyString()
        {
            var context = new OperationContext()
            {
                CurrentValue = "abc123",
                Dataset = new FfsRecord
                {
                    PassthroughRules = new List<PassthroughRuleDto>()
                    {
                        new() { RuleType =  PassthroughRuleType.Prefix, Value = "6" },
                    }
                }
            };

            var sut = new TrimPassthroughPrefixOperation();

            var result = await sut.InvokeAsync(context);

            Assert.Equal(string.Empty, result);
            Assert.Equal("abc123", context.Data["Prefix"]);
        }

        [Fact]
        public async Task InvokeAsync_PrefixLengthThree_ReturnsTrimmedValue()
        {
            var context = new OperationContext()
            {
                CurrentValue = "abc123",
                Dataset = new FfsRecord
                {
                    PassthroughRules = new List<PassthroughRuleDto>()
                    {
                        new() { RuleType =  PassthroughRuleType.Prefix, Value = "3" },
                    }
                }
            };

            var sut = new TrimPassthroughPrefixOperation();

            var result = await sut.InvokeAsync(context);

            Assert.Equal("123", result);
        }

        [Fact]
        public async Task InvokeAsync_PrefixLengthThree_AddsTrimmedValueToData()
        {
            var context = new OperationContext()
            {
                CurrentValue = "abc123",
                Dataset = new FfsRecord
                {
                    PassthroughRules = new List<PassthroughRuleDto>()
                    {
                        new() { RuleType =  PassthroughRuleType.Prefix, Value = "3" },
                    }
                }
            };

            var sut = new TrimPassthroughPrefixOperation();

            await sut.InvokeAsync(context);

            Assert.Equal("abc", context.Data["Prefix"]);
        }
    }
}
