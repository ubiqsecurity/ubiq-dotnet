namespace UbiqSecurity.Tests
{
    public class UbiqStructuredEncryptDecryptDateTimeTests
    {
        [Fact]
        public async Task EncryptAsync_ValidDateTime_ReturnsEncryptedDateTimeThatDecrypts()
        {
            var expectedDate = new DateTime(2182, 4, 27, 1, 34, 7, DateTimeKind.Utc);
            var plainDate = new DateTime(2001, 1, 10, 3, 4, 5, DateTimeKind.Utc);

            var sut = CryptographyBuilder
                            .Create()
                            .WithCredentialsFromDefaultFileLocation("local-datetime")
                            .BuildStructured();

            var cipherDate = await sut.EncryptAsync("DATETIME", plainDate);

            Assert.Equal(expectedDate, cipherDate);
        }

        [Fact]
        public async Task DecryptAsync_ValidDateTime_ReturnsPlainDateTime()
        {
            var expectedDate = new DateTime(2001, 1, 10, 3, 4, 5, DateTimeKind.Utc);
            var cipherDate = new DateTime(2182, 4, 27, 1, 34, 7, DateTimeKind.Utc);

            var sut = CryptographyBuilder
                            .Create()
                            .WithCredentialsFromDefaultFileLocation("local-datetime")
                            .BuildStructured();

            var decryptedDate = await sut.DecryptAsync("DATETIME", cipherDate);

            Assert.Equal(expectedDate, decryptedDate);
        }
    }
}
