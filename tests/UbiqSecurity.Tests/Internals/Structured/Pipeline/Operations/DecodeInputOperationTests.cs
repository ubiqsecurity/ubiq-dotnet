using UbiqSecurity.Internals.Structured.Pipeline;
using UbiqSecurity.Internals.Structured.Pipeline.Operations;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity.Tests.Internals.Structured.Pipeline.Operations
{
    public class DecodeInputOperationTests
    {
        internal readonly OperationContext TestContext = new()
        {
            CurrentValue = "NjU0enl4",
            Dataset = new FfsRecord(),
            Data = new Dictionary<string, string>(),
        };

        [Fact]
        public async Task InvokeAsync_NullInputEncoding_ReturnsCurrentValue()
        {
            TestContext.Dataset.InputEncoding = null;

            var sut = new DecodeInputOperation();

            var result = await sut.InvokeAsync(TestContext);

            Assert.Equal("NjU0enl4", result);
        }

        [Fact]
        public async Task InvokeAsync_EncodeBase64_ReturnsExpectedBase64DecodedString()
        {
            TestContext.Dataset.InputEncoding = "base64";

            var sut = new DecodeInputOperation();

            var result = await sut.InvokeAsync(TestContext);

            Assert.Equal("654zyx", result);
        }

        [Fact]
        public async Task InvokeAsync_EncodeBase32_ReturnsExpectedBase32DecodedString()
        {
            TestContext.CurrentValue = "GY2TI6TZPA======";
            TestContext.Dataset.InputEncoding = "base32";

            var sut = new DecodeInputOperation();

            var result = await sut.InvokeAsync(TestContext);

            Assert.Equal("654zyx", result);
        }
    }
}
