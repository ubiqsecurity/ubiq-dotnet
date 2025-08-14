using UbiqSecurity.Internals.Structured.Pipeline;
using UbiqSecurity.Internals.Structured.Pipeline.Operations;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity.Tests.Internals.Structured.Pipeline.Operations
{
    public class EncryptOperationTests
    {
        private readonly OperationContext TestContext = new()
        {
            CurrentValue = "abc-123",
            IsEncrypt = true,
            Dataset = new FfsRecord
            {
                MinInputLength = 4,
            },
        };

        [Fact]
        public async Task InvokeAsync_DecryptContext_ThrowsException()
        {
            TestContext.IsEncrypt = false;

            var sut = new EncryptOperation();

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.InvokeAsync(TestContext));
        }

        [Fact]
        public async Task InvokeAsync_CurrentValueLengthLessThanInputMinimum_ThrowsException()
        {
            TestContext.Dataset.MinInputLength = 11;

            var sut = new EncryptOperation();

            await Assert.ThrowsAsync<ArgumentException>(() => sut.InvokeAsync(TestContext));
        }

        [Fact]
        public async Task InvokeAsync_CurrentValueLengthGreaterThanInputMaximum_ThrowsException()
        {
            TestContext.Dataset.MaxInputLength = 1;

            var sut = new EncryptOperation();

            await Assert.ThrowsAsync<ArgumentException>(() => sut.InvokeAsync(TestContext));
        }
    }
}
