using UbiqSecurity.Internals.Structured.Pipeline;
using UbiqSecurity.Internals.Structured.Pipeline.Operations;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity.Tests.Internals.Structured.Pipeline.Operations
{
    public class TrimPassthroughCharactersOperationTests
    {
        [Fact]
        public async Task InvokeAsync_NoPassthroughRules_ReturnsCurrentValue()
        {
            var context = new OperationContext()
            {
                CurrentValue = "abc-123",
                Dataset = new FfsRecord
                {
                    InputCharacters = "abc123",
                    OutputCharacters = "xyz456",
                    PassthroughRules = new List<PassthroughRuleDto>(),
                },
            };

            var sut = new TrimPassthroughCharactersOperation();

            var result = await sut.InvokeAsync(context);

            Assert.Equal("abc-123", result);
        }

        [Fact]
        public async Task InvokeAsync_PrefixCharacterSetEmpty_ReturnsCurrentValue()
        {
            var context = new OperationContext()
            {
                CurrentValue = "abc-123",
                Dataset = new FfsRecord
                {
                    InputCharacters = "abc123",
                    OutputCharacters = "xyz456",
                    PassthroughRules = new List<PassthroughRuleDto>()
                    {
                        new() { RuleType =  PassthroughRuleType.Passthrough, Value = "" },
                    }
                }
            };

            var sut = new TrimPassthroughCharactersOperation();

            var result = await sut.InvokeAsync(context);

            Assert.Equal("abc-123", result);
        }

        [Fact]
        public async Task InvokeAsync_PassthroughCharactersNotFound_ReturnsCurrentValue()
        {
            var context = new OperationContext()
            {
                CurrentValue = "abc-123",
                Dataset = new FfsRecord
                {
                    InputCharacters = "abc123",
                    OutputCharacters = "xyz456",
                    PassthroughRules = new List<PassthroughRuleDto>()
                    {
                        new() { RuleType =  PassthroughRuleType.Passthrough, Value = "!" },
                    }
                }
            };

            var sut = new TrimPassthroughCharactersOperation();

            var result = await sut.InvokeAsync(context);

            Assert.Equal("abc-123", result);
        }

        [Fact]
        public async Task InvokeAsync_PassthroughCharactersExists_ReturnsTrimmedValue()
        {
            var context = new OperationContext()
            {
                CurrentValue = "abc-123",
                Dataset = new FfsRecord
                {
                    InputCharacters = "abc123",
                    OutputCharacters = "xyz456",
                    PassthroughRules = new List<PassthroughRuleDto>()
                    {
                        new() { RuleType =  PassthroughRuleType.Passthrough, Value = "-" },
                    }
                }
            };

            var sut = new TrimPassthroughCharactersOperation();

            var result = await sut.InvokeAsync(context);

            Assert.Equal("abc123", result);
        }

        [Fact]
        public async Task InvokeAsync_Encrypt_PassthroughTemplateContainsFirstOutputCharacter()
        {
            var context = new OperationContext()
            {
                CurrentValue = "abc-123",
                IsEncrypt = true,
                Dataset = new FfsRecord
                {
                    InputCharacters = "abc123",
                    OutputCharacters = "xyz456",
                    PassthroughRules = new List<PassthroughRuleDto>()
                    {
                        new() { RuleType =  PassthroughRuleType.Passthrough, Value = "-" },
                    }
                }
            };

            var sut = new TrimPassthroughCharactersOperation();

            await sut.InvokeAsync(context);

            Assert.Equal("xxx-xxx", context.Data["PassthroughTemplate"]);
        }

        [Fact]
        public async Task InvokeAsync_Decrypt_PassthroughTemplateContainsFirstInputCharacter()
        {
            var context = new OperationContext()
            {
                CurrentValue = "abc-123",
                IsEncrypt = false,
                Dataset = new FfsRecord
                {
                    InputCharacters = "abc123",
                    OutputCharacters = "xyz456",
                    PassthroughRules = new List<PassthroughRuleDto>()
                    {
                        new() { RuleType =  PassthroughRuleType.Passthrough, Value = "-" },
                    }
                }
            };

            var sut = new TrimPassthroughCharactersOperation();

            await sut.InvokeAsync(context);

            Assert.Equal("aaa-aaa", context.Data["PassthroughTemplate"]);
        }
    }
}
