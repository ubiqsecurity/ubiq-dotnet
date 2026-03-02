using UbiqSecurity.Tests.Fixtures;

namespace UbiqSecurity.Tests
{
    public class UbiqStructuredEncryptDecryptDateTimeTests : IClassFixture<UbiqStructuredEncryptDecryptFixture>
    {
        private const string DateTimeDatasetName = "datetime";

        private readonly UbiqStructuredEncryptDecryptFixture _fixture;

        public UbiqStructuredEncryptDecryptDateTimeTests(UbiqStructuredEncryptDecryptFixture fixture)
        {
            _fixture = fixture;
        }

        public static IEnumerable<object[]> TestData => new List<object[]>
        {
            new object[] { new DateTime(2001, 1, 10, 3, 4, 5, DateTimeKind.Utc), null },
            new object[] { new DateTime(2001, 1, 10, 3, 4, 5, 6, DateTimeKind.Utc), null },     // milliseconds that get trimmed
            new object[] { new DateTime(2001, 1, 10, 3, 4, 5, DateTimeKind.Local), null },      // local timezone
            new object[] { new DateTime(1969, 12, 30, 15, 0, 0, DateTimeKind.Utc), null },      // slightly before epoch, small negative number
            new object[] { new DateTime(2286, 11, 20, 17, 46, 39, DateTimeKind.Utc), null },    // max value
            new object[] { new DateTime(1653, 2, 10, 6, 13, 21, DateTimeKind.Utc), null },      // min value
        };

        public static IEnumerable<object[]> OutOfRangeTestData => new List<object[]>
        {
            new object[] { new DateTime(2286, 11, 20, 17, 46, 40, DateTimeKind.Utc) },    // max value + 1 second
            new object[] { new DateTime(1653, 2, 10, 6, 13, 20, DateTimeKind.Utc) },      // min value - 1 second
        };

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task EncryptAsync_ValidDateTime_ReturnsEncryptedDateTimeThatDecrypts(DateTime plainDate, DateTime? expectedCipherDate)
        {
            var sut = _fixture.UbiqStructuredEncryptDecrypt;

            var cipherDate = await sut.EncryptAsync(DateTimeDatasetName, plainDate);

            Assert.Equal(plainDate.Kind, cipherDate.Kind);
            Assert.Equal(0, cipherDate.Millisecond);

            if (expectedCipherDate.HasValue)
            {
                Assert.Equal(expectedCipherDate, cipherDate);
            }

            var decryptedDate = await sut.DecryptAsync(DateTimeDatasetName, cipherDate);

            // encrypt/decrypt process discards milliseconds so we need to do the same when comparing to the original value
            var plainDateNoMilliseconds = plainDate.AddTicks(-(plainDate.Ticks % TimeSpan.TicksPerSecond));

            Assert.Equal(plainDateNoMilliseconds, decryptedDate);
        }

        [Theory]
        [MemberData(nameof(OutOfRangeTestData))]
        public async Task EncryptAsync_DateTimeOutOfRange_ThrowsArgumentOutOfRangeException(DateTime plainDate)
        {
            var sut = _fixture.UbiqStructuredEncryptDecrypt;

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await sut.EncryptAsync(DateTimeDatasetName, plainDate));
        }

        [Fact]
        public async Task EncryptForSearchAsync_ValidDataset_ReturnedArrayOfCiphersContainsExpectedCipher()
        {
            var sut = _fixture.UbiqStructuredEncryptDecrypt;
            var plainDate = new DateTime(2286, 11, 20, 17, 46, 39, DateTimeKind.Utc);

            // ensure we have multiple ciphers, they are all unique, and it contains the most recent ciphertext
            var allCiphers = await sut.EncryptForSearchAsync(DateTimeDatasetName, plainDate);
            Assert.True(allCiphers.Count() > 1);
            Assert.Equal(allCiphers.Distinct().Count(), allCiphers.Count());

            // NOTE: this EncryptAsync call must come after EncryptForSearchAsync() call or this call will prime the dataset/key caches
            var mostRecentCipher = await sut.EncryptAsync(DateTimeDatasetName, plainDate);
            Assert.Contains(mostRecentCipher, allCiphers);

            // ensure all ciphers can be decrypted to original value
            foreach (var cipher in allCiphers)
            {
                var decryptedDate = await sut.DecryptAsync(DateTimeDatasetName, cipher);
                Assert.Equal(plainDate, decryptedDate);
            }
        }
    }
}
