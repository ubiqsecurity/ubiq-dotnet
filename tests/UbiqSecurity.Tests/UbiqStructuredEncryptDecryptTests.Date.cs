namespace UbiqSecurity.Tests
{
    public class UbiqStructuredEncryptDecryptDateTests
    {
        public static IEnumerable<object[]> TestData => new List<object[]>
        {
            new object[] { new DateTime(0001, 1, 1, 0, 0, 0, DateTimeKind.Utc), null }, // min value
            new object[] { new DateTime(2738, 11, 28, 0, 0, 0, DateTimeKind.Utc), null }, // max value
            new object[] { new DateTime(1653, 2, 10, 0, 0, 0, DateTimeKind.Utc), null },
        };

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task EncryptAsync_ValidDate_ReturnsEncryptedDateThatDecrypts(DateTime plainDate, DateTime? expectedCipherDate)
        {
            var sut = CryptographyBuilder
                            .Create()
                            .WithCredentialsFromDefaultFileLocation("local-datetime")
                            .BuildStructured();

            var cipherDate = await sut.EncryptDateAsync("DATE", plainDate);

            if (expectedCipherDate.HasValue)
            {
                Assert.Equal(expectedCipherDate, cipherDate);
            }

            var decryptedDate = await sut.DecryptDateAsync("DATE", cipherDate);

            Assert.Equal(plainDate, decryptedDate);
        }
    }
}
