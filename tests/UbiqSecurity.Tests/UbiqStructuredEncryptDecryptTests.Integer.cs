namespace UbiqSecurity.Tests
{
    public class UbiqStructuredEncryptDecryptIntegerTests
    {
        [Theory]
        [InlineData(151223)]
        [InlineData(0)]
        [InlineData(-12312)]
        [InlineData(99999999)] // max value
        [InlineData(-99999999)] // min value
        public async Task EncryptAsync_ValidInteger_DecryptsToOriginalValue(int plainInteger)
        {
            var sut = CryptographyBuilder
                            .Create()
                            .WithCredentialsFromDefaultFileLocation("local-datetime")
                            .BuildStructured();

            var cipherInteger = await sut.EncryptAsync("INTEGER", plainInteger);

            var decryptedInteger = await sut.DecryptAsync("INTEGER", cipherInteger);

            Assert.Equal(plainInteger, decryptedInteger);
        }
    }
}
