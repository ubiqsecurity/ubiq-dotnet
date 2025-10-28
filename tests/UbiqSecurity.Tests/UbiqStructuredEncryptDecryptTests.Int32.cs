namespace UbiqSecurity.Tests
{
    public class UbiqStructuredEncryptDecryptInt32Tests
    {
        public const string Int32DatasetName = "INT32";
        public const string ProfileName = "local-datetime"; // TODO: change profile name back before MERGE

        [Theory]
        [InlineData(151223)]
        [InlineData(0)]
        [InlineData(-12312)]
        [InlineData(99999999)] // max value
        [InlineData(-99999999)] // min value
        public async Task EncryptAsync_ValidInt32_DecryptsToOriginalValue(int plainInteger)
        {
            var sut = CryptographyBuilder
                            .Create()
                            .WithCredentialsFromDefaultFileLocation(ProfileName) 
                            .BuildStructured();

            var cipherInteger = await sut.EncryptAsync(Int32DatasetName, plainInteger);

            var decryptedInteger = await sut.DecryptAsync(Int32DatasetName, cipherInteger);

            Assert.Equal(plainInteger, decryptedInteger);
        }

        [Theory]
        [InlineData(99999999 + 1)] // max value
        [InlineData(-99999999 - 1)] // min value
        public async Task EncryptAsync_Int32OutOfRange_ThrowsArgumentOutOfRangeException(int plainInteger)
        {
            var sut = CryptographyBuilder
                            .Create()
                            .WithCredentialsFromDefaultFileLocation(ProfileName)
                            .BuildStructured();

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await sut.EncryptAsync(Int32DatasetName, plainInteger));
        }
    }
}
