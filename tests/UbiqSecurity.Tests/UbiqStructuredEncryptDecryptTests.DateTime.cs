namespace UbiqSecurity.Tests
{
    public class UbiqStructuredEncryptDecryptDateTimeTests
    {
        public static IEnumerable<object[]> TestData => new List<object[]>
        {
            new object[] { new DateTime(2001, 1, 10, 3, 4, 5, DateTimeKind.Utc), null },
            new object[] { new DateTime(1969, 12, 30, 15, 0, 0, DateTimeKind.Utc), null },      // slightly before epoch, small negative number
            new object[] { new DateTime(2286, 11, 20, 17, 46, 39, DateTimeKind.Utc), null },    // max value
            new object[] { new DateTime(1653, 2, 10, 6, 13, 21, DateTimeKind.Utc), null },      // min value
        };

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task EncryptAsync_ValidDateTime_ReturnsEncryptedDateTimeThatDecrypts(DateTime plainDate, DateTime? expectedCipherDate)
        {
            var sut = CryptographyBuilder
                            .Create()
                            .WithCredentialsFromDefaultFileLocation("local-datetime")
                            .BuildStructured();

            var cipherDate = await sut.EncryptAsync("DATETIME", plainDate);

            if (expectedCipherDate.HasValue)
            {
                Assert.Equal(expectedCipherDate, cipherDate);
            }

            var decryptedDate = await sut.DecryptAsync("DATETIME", cipherDate);

            Assert.Equal(plainDate, decryptedDate);
        }
    }
}
