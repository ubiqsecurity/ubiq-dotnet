using UbiqSecurity.Tests.Fixtures;

namespace UbiqSecurity.Tests
{
    public class UbiqStructuredEncryptDecryptDateTests : IClassFixture<UbiqStructuredEncryptDecryptFixture>
    {
        private const string DateDatasetName = "date";

        private readonly UbiqStructuredEncryptDecryptFixture _fixture;

        public UbiqStructuredEncryptDecryptDateTests(UbiqStructuredEncryptDecryptFixture fixture)
        {
            _fixture = fixture;
        }

        public static IEnumerable<object[]> TestData => new List<object[]>
        {
            new object[] { new DateTime(0001, 1, 1, 0, 0, 0, DateTimeKind.Utc), null }, // min value
            new object[] { new DateTime(2738, 11, 28, 0, 0, 0, DateTimeKind.Utc), null }, // max value
            new object[] { new DateTime(1653, 2, 10, 0, 0, 0, DateTimeKind.Utc), null },
        };

        public static IEnumerable<object[]> OutOfRangeTestData => new List<object[]>
        {
            new object[] { new DateTime(2738, 11, 29, 0, 0, 1, DateTimeKind.Utc) }, // max value + 1 day
        };

        public static IEnumerable<object[]> NonUtcTestData => new List<object[]>
        {
            new object[] { new DateTime(2738, 11, 29, 0, 0, 0, DateTimeKind.Local) },
            new object[] { new DateTime(2738, 11, 29, 0, 0, 0, DateTimeKind.Unspecified) },
        };

        [Theory]
        [MemberData(nameof(TestData))]
        public async Task EncryptAsync_ValidDate_ReturnsEncryptedDateThatDecrypts(DateTime plainDate, DateTime? expectedCipherDate)
        {
            var sut = _fixture.UbiqStructuredEncryptDecrypt;

            var cipherDate = await sut.EncryptDateAsync(DateDatasetName, plainDate);

            if (expectedCipherDate.HasValue)
            {
                Assert.Equal(expectedCipherDate, cipherDate);
            }

            var decryptedDate = await sut.DecryptDateAsync(DateDatasetName, cipherDate);

            Assert.Equal(plainDate, decryptedDate);
        }

        [Theory]
        [MemberData(nameof(OutOfRangeTestData))]
        public async Task EncryptAsync_DateOutOfRange_ThrowsArgumentOutOfRangeException(DateTime plainDate)
        {
            var sut = _fixture.UbiqStructuredEncryptDecrypt;

            await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () => await sut.EncryptDateAsync(DateDatasetName, plainDate));
        }

        [Theory]
        [MemberData(nameof(NonUtcTestData))]
        public async Task EncryptAsync_DateNotUtc_ThrowsArgumentException(DateTime plainDate)
        {
            var sut = _fixture.UbiqStructuredEncryptDecrypt;

            await Assert.ThrowsAsync<ArgumentException>(async () => await sut.EncryptDateAsync(DateDatasetName, plainDate));
        }

        [Fact]
        public async Task EncryptDateForSearchAsync_ValidDataset_ReturnedArrayOfCiphersContainsExpectedCipher()
        {
            var sut = _fixture.UbiqStructuredEncryptDecrypt;
            var plainDate = new DateTime(1653, 2, 10, 0, 0, 0, DateTimeKind.Utc);

            // ensure we have multiple ciphers, they are all unique, and it contains the most recent ciphertext
            var allCiphers = await sut.EncryptDateForSearchAsync(DateDatasetName, plainDate);
            
            // currently DATE has max rotations of 1, so ciphers count will always be 1 until we allow customization of epoch
            // Assert.True(allCiphers.Count() > 1);
            Assert.Equal(allCiphers.Distinct().Count(), allCiphers.Count());

            // NOTE: this EncryptAsync call must come after EncryptForSearchAsync() call or this call will prime the dataset/key caches
            var mostRecentCipher = await sut.EncryptDateAsync(DateDatasetName, plainDate);
            Assert.Contains(mostRecentCipher, allCiphers);

            // ensure all ciphers can be decrypted to original value
            foreach (var cipher in allCiphers)
            {
                var decryptedDate = await sut.DecryptDateAsync(DateDatasetName, cipher);
                Assert.Equal(plainDate, decryptedDate);
            }
        }
    }
}
