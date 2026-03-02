using UbiqSecurity.Internals.Structured.Pipeline;
using UbiqSecurity.Internals.Structured.Pipeline.Operations;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity.Tests.Internals.Structured.Pipeline.Operations
{
    public class EncodeInputOperationTests
    {
        internal readonly OperationContext TestContext = new()
        {
            CurrentValue = "654zyx",
            Dataset = new FfsRecord(),
            Data = new Dictionary<string, string>(),
        };

        [Fact]
        public async Task InvokeAsync_NullInputEncoding_ReturnsCurrentValue()
        {
            TestContext.Dataset.InputEncoding = null;

            var sut = new EncodeInputOperation();

            var result = await sut.InvokeAsync(TestContext);

            Assert.Equal("654zyx", result);
        }

        [Fact]
        public async Task InvokeAsync_EncodeBase64_ReturnsExpectedBase64EncodedString()
        {
            TestContext.Dataset.InputEncoding = "base64";

            var sut = new EncodeInputOperation();

            var result = await sut.InvokeAsync(TestContext);

            Assert.Equal("NjU0enl4", result);
        }

        [Fact]
        public async Task InvokeAsync_EncodeBase32_ReturnsExpectedBase32EncodedString()
        {
            TestContext.Dataset.InputEncoding = "base32";

            var sut = new EncodeInputOperation();

            var result = await sut.InvokeAsync(TestContext);

            Assert.Equal("GY2TI6TZPA======", result);
        }
    }
}
