using UbiqSecurity.Tests.Fixtures;

namespace UbiqSecurity.Tests
{
    public class UbiqStructuredEncryptDecryptGenericStringTests : IClassFixture<UbiqStructuredEncryptDecryptFixture>
    {
        // Note Datasets including padding "_" to 15 characters
        public const string GenericStringDatasetName = "generic_string";
        public const string GenericBase32DatasetName = "generic_string_32";
        public const string GenericBase64DatasetName = "generic_string_64";

        private readonly UbiqStructuredEncryptDecryptFixture _fixture;

        public UbiqStructuredEncryptDecryptGenericStringTests(UbiqStructuredEncryptDecryptFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData(GenericBase32DatasetName, "0123")]              // less than min_length
        [InlineData(GenericBase32DatasetName, "0123456789ABCDEF")]  // more than min_length
        [InlineData(GenericBase64DatasetName, "0123456789")]        // less than min_length
        [InlineData(GenericBase64DatasetName, "0123")]              // more than min_length
        [InlineData(GenericStringDatasetName, "abcd")]
        public async Task EncryptAsync_PaddedInput_EncryptionIsReversible(string datasetName, string plainText)
        {
            var sut = _fixture.UbiqStructuredEncryptDecrypt;

            var cipherText = await sut.EncryptAsync(datasetName, plainText);

            var actualPlainText = await sut.DecryptAsync(datasetName, cipherText);

            Assert.Equal(plainText, actualPlainText);
        }

        [Fact]
        public async Task EncryptAsync_InputAlreadyContainsPadCharacter_ThrowsException()
        {
            var sut = _fixture.UbiqStructuredEncryptDecrypt;

            await Assert.ThrowsAsync<ArgumentException>(() => sut.EncryptAsync(GenericStringDatasetName, "!abcd!"));
        }
    }
}
