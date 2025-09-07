namespace UbiqSecurity.Tests
{
    public class UbiqStructuredEncryptDecryptInt64Tests
    {
        [Theory]
        [InlineData(151223)]
        [InlineData(0)]
        [InlineData(-12312)]
        [InlineData(9999999999999999)] // max value
        [InlineData(-9999999999999999)] // min value
        public async Task EncryptAsync_ValidInt64_DecryptsToOriginalValue(long plainInteger)
        {
            var sut = CryptographyBuilder
                            .Create()
                            .WithCredentialsFromDefaultFileLocation("local-datetime")
                            .BuildStructured();

            var cipherInteger = await sut.EncryptAsync("INT64", plainInteger);

            var decryptedInteger = await sut.DecryptAsync("INT64", cipherInteger);

            Assert.Equal(plainInteger, decryptedInteger);
        }

        [Theory]
        [InlineData(9999999999999999 + 1)] // max value
        [InlineData(-9999999999999999 - 1)] // min value
        public async Task EncryptAsync_Int64OutOfRange_ThrowsArgumentOutOfRangeException(long plainInteger)
        {
            var sut = CryptographyBuilder
                            .Create()
                            .WithCredentialsFromDefaultFileLocation("local-datetime")
                            .BuildStructured();

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await sut.EncryptAsync("INT64", plainInteger));
        }
    }
}
