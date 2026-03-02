using UbiqSecurity.Tests.Fixtures;

namespace UbiqSecurity.Tests
{
    public class UbiqStructuredEncryptDecryptInt64Tests : IClassFixture<UbiqStructuredEncryptDecryptFixture>
    {
        private readonly UbiqStructuredEncryptDecryptFixture _fixture;

        public UbiqStructuredEncryptDecryptInt64Tests(UbiqStructuredEncryptDecryptFixture fixture)
        {
            _fixture = fixture;
        }

        public const string Int64DatasetName = "integer64";

        [Theory]
        [InlineData(151223)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(-12312)]
        [InlineData(9999999999999999)] // max value
        [InlineData(-9999999999999999)] // min value
        public async Task EncryptAsync_ValidInt64_DecryptsToOriginalValue(long plainInteger)
        {
            var sut = _fixture.UbiqStructuredEncryptDecrypt;

            var cipherInteger = await sut.EncryptAsync(Int64DatasetName, plainInteger);

            var decryptedInteger = await sut.DecryptAsync(Int64DatasetName, cipherInteger);

            Assert.Equal(plainInteger, decryptedInteger);
        }

        [Theory]
        [InlineData(9999999999999999 + 1)] // max value
        [InlineData(-9999999999999999 - 1)] // min value
        public async Task EncryptAsync_Int64OutOfRange_ThrowsArgumentOutOfRangeException(long plainInteger)
        {
            var sut = _fixture.UbiqStructuredEncryptDecrypt;

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await sut.EncryptAsync(Int64DatasetName, plainInteger));
        }

        [Fact]
        public async Task EncryptForSearchAsync_ValidDataset_ReturnedArrayOfCiphersContainsExpectedCipher()
        {
            var sut = _fixture.UbiqStructuredEncryptDecrypt;
            long plainInteger = 9999999999999999; // maxvalue

            // ensure we have multiple ciphers, they are all unique, and it contains the most recent ciphertext
            var allCiphers = await sut.EncryptForSearchAsync(Int64DatasetName, plainInteger);
            Assert.True(allCiphers.Count() > 1);
            Assert.Equal(allCiphers.Distinct().Count(), allCiphers.Count());

            // NOTE: this EncryptAsync call must come after EncryptForSearchAsync() call or this call will prime the dataset/key caches
            var mostRecentCipher = await sut.EncryptAsync(Int64DatasetName, plainInteger);
            Assert.Contains(mostRecentCipher, allCiphers);

            // ensure all ciphers can be decrypted to original value
            foreach (var cipher in allCiphers)
            {
                var decryptedInteger = await sut.DecryptAsync(Int64DatasetName, cipher);
                Assert.Equal(plainInteger, decryptedInteger);
            }
        }
    }
}
