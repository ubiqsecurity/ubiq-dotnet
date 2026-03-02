using UbiqSecurity.Tests.Fixtures;

namespace UbiqSecurity.Tests
{
    public class UbiqStructuredEncryptDecryptInt32Tests : IClassFixture<UbiqStructuredEncryptDecryptFixture>
    {
        private readonly UbiqStructuredEncryptDecryptFixture _fixture;

        public UbiqStructuredEncryptDecryptInt32Tests(UbiqStructuredEncryptDecryptFixture fixture)
        {
            _fixture = fixture;
        }

        public const string Int32DatasetName = "integer32";

        [Theory]
        [InlineData(151223)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-12312)]
        [InlineData(99999999)] // max value
        [InlineData(-99999999)] // min value
        public async Task EncryptAsync_ValidInt32_DecryptsToOriginalValue(int plainInteger)
        {
            var sut = _fixture.UbiqStructuredEncryptDecrypt;

            var cipherInteger = await sut.EncryptAsync(Int32DatasetName, plainInteger);

            var decryptedInteger = await sut.DecryptAsync(Int32DatasetName, cipherInteger);

            Assert.Equal(plainInteger, decryptedInteger);
        }

        [Theory]
        [InlineData(99999999 + 1)] // max value
        [InlineData(-99999999 - 1)] // min value
        public async Task EncryptAsync_Int32OutOfRange_ThrowsArgumentOutOfRangeException(int plainInteger)
        {
            var sut = _fixture.UbiqStructuredEncryptDecrypt;

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await sut.EncryptAsync(Int32DatasetName, plainInteger));
        }

        [Fact]
        public async Task EncryptForSearchAsync_ValidDataset_ReturnedArrayOfCiphersContainsExpectedCipher()
        {
            var sut = _fixture.UbiqStructuredEncryptDecrypt;
            var plainInteger = 42;

            // ensure we have multiple ciphers, they are all unique, and it contains the most recent ciphertext
            var allCiphers = await sut.EncryptForSearchAsync(Int32DatasetName, plainInteger);
            Assert.True(allCiphers.Count() > 1);
            Assert.Equal(allCiphers.Distinct().Count(), allCiphers.Count());

            // NOTE: this EncryptAsync call must come after EncryptForSearchAsync() call or this call will prime the dataset/key caches
            var mostRecentCipher = await sut.EncryptAsync(Int32DatasetName, plainInteger);
            Assert.Contains(mostRecentCipher, allCiphers);

            // ensure all ciphers can be decrypted to original value
            foreach (var cipher in allCiphers)
            {
                var decryptedInteger = await sut.DecryptAsync(Int32DatasetName, cipher);
                Assert.Equal(plainInteger, decryptedInteger);
            }
        }
    }
}
