using UbiqSecurity.Internals.Structured.Pipeline;
using UbiqSecurity.Internals.Structured.Pipeline.Operations;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity.Tests.Internals.Structured.Pipeline.Operations
{
    public class DecryptOperationTests
    {
        private readonly OperationContext TestContext = new()
        {
            CurrentValue = "abc-123",
            IsEncrypt = false,
            Dataset = new FfsRecord
            {
                MinInputLength = 4,
            },
        };

        [Fact]
        public async Task InvokeAsync_EncryptContext_ThrowsException()
        {
            TestContext.IsEncrypt = true;

            var sut = new DecryptOperation();

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.InvokeAsync(TestContext));
        }

        [Fact]
        public async Task InvokeAsync_CurrentValueLengthLessThanInputMinimum_ThrowsException()
        {
            TestContext.Dataset.MinInputLength = 11;

            var sut = new DecryptOperation();

            await Assert.ThrowsAsync<ArgumentException>(() => sut.InvokeAsync(TestContext));
        }

        [Fact]
        public async Task InvokeAsync_CurrentValueLengthGreaterThanInputMaximum_ThrowsException()
        {
            TestContext.Dataset.MaxInputLength = 1;

            var sut = new DecryptOperation();

            await Assert.ThrowsAsync<ArgumentException>(() => sut.InvokeAsync(TestContext));
        }
    }
}
