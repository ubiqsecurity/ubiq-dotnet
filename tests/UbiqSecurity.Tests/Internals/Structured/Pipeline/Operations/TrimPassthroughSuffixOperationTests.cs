using UbiqSecurity.Internals.Structured.Pipeline;
using UbiqSecurity.Internals.Structured.Pipeline.Operations;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity.Tests.Internals.Structured.Pipeline.Operations
{
    public class TrimPassthroughSuffixOperationTests
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

            var sut = new TrimPassthroughSuffixOperation();

            var result = await sut.InvokeAsync(context);

            Assert.Equal("abc123", result);
        }

        [Fact]
        public async Task InvokeAsync_SuffixLengthZero_ReturnsCurrentValue()
        {
            var context = new OperationContext()
            {
                CurrentValue = "abc123",
                Dataset = new FfsRecord
                {
                    PassthroughRules = new List<PassthroughRuleDto>()
                    {
                        new() { RuleType =  PassthroughRuleType.Suffix, Value = "0" },
                    }
                }
            };

            var sut = new TrimPassthroughSuffixOperation();

            var result = await sut.InvokeAsync(context);

            Assert.Equal("abc123", result);
        }

        [Fact]
        public async Task InvokeAsync_SuffixLengthGreaterThanCurrentValueLength_ThrowsException()
        {
            var context = new OperationContext()
            {
                CurrentValue = "abc123",
                Dataset = new FfsRecord
                {
                    PassthroughRules = new List<PassthroughRuleDto>()
                    {
                        new() { RuleType =  PassthroughRuleType.Suffix, Value = "7" },
                    }
                }
            };

            var sut = new TrimPassthroughSuffixOperation();

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => sut.InvokeAsync(context));
        }

        [Fact]
        public async Task InvokeAsync_SuffixLengthEqualToCurrentValueLength_ReturnsEmptyString()
        {
            var context = new OperationContext()
            {
                CurrentValue = "abc123",
                Dataset = new FfsRecord
                {
                    PassthroughRules = new List<PassthroughRuleDto>()
                    {
                        new() { RuleType =  PassthroughRuleType.Suffix, Value = "6" },
                    }
                }
            };

            var sut = new TrimPassthroughSuffixOperation();

            var result = await sut.InvokeAsync(context);

            Assert.Equal(string.Empty, result);
            Assert.Equal("abc123", context.Data["Suffix"]);
        }

        [Fact]
        public async Task InvokeAsync_SuffixLengthThree_ReturnsTrimmedValue()
        {
            var context = new OperationContext()
            {
                CurrentValue = "abc123",
                Dataset = new FfsRecord
                {
                    PassthroughRules = new List<PassthroughRuleDto>()
                    {
                        new() { RuleType =  PassthroughRuleType.Suffix, Value = "3" },
                    }
                }
            };

            var sut = new TrimPassthroughSuffixOperation();

            var result = await sut.InvokeAsync(context);

            Assert.Equal("abc", result);
        }

        [Fact]
        public async Task InvokeAsync_SuffixLengthThree_AddsTrimmedValueToData()
        {
            var context = new OperationContext()
            {
                CurrentValue = "abc123",
                Dataset = new FfsRecord
                {
                    PassthroughRules = new List<PassthroughRuleDto>()
                    {
                        new() { RuleType =  PassthroughRuleType.Suffix, Value = "3" },
                    }
                }
            };

            var sut = new TrimPassthroughSuffixOperation();

            await sut.InvokeAsync(context);

            Assert.Equal("123", context.Data["Suffix"]);
        }
    }
}
