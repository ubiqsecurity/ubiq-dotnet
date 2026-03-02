using UbiqSecurity.Tests.Fixtures;

namespace UbiqSecurity.Tests
{
    public class UbiqStructuredEncryptDecryptTokenTests : IClassFixture<UbiqStructuredEncryptDecryptFixture>
    {
        public const string Token64DatasetName = "token64";
        public const string Token128DatasetName = "token128";

        private readonly UbiqStructuredEncryptDecryptFixture _fixture;

        public UbiqStructuredEncryptDecryptTokenTests(UbiqStructuredEncryptDecryptFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory]
        [InlineData(Token64DatasetName, "1_23-45-6789", 64)]
        [InlineData(Token128DatasetName, "01_01/2020", 128)]
        public async Task EncryptAsync_ValidInput_EncryptionIsReversible(string datasetName, string plainText, int expectedOutputLength)
        {
            var sut = _fixture.UbiqStructuredEncryptDecrypt;

            var cipherText = await sut.EncryptAsync(datasetName, plainText);
            Assert.Equal(cipherText.Length, expectedOutputLength);
            var actualPlainText = await sut.DecryptAsync(datasetName, cipherText);

            Assert.Equal(plainText, actualPlainText);
        }

        [Theory]
        [InlineData(Token64DatasetName, "ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ")]
        [InlineData(Token128DatasetName, "ZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZZ")]
        public async Task EncryptAsync_InputEncodesToGreaterThanTokenSize_ThrowsException(string datasetName, string plainText)
        {
            var sut = _fixture.UbiqStructuredEncryptDecrypt;

            await Assert.ThrowsAsync<ArgumentException>(async () => await sut.EncryptAsync(datasetName, plainText));
        }
    }
}
