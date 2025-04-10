using UbiqSecurity.Tests.Fixtures;

namespace UbiqSecurity.Tests
{
    public class UbiqOktaStructuredEncryptDecryptTests : IClassFixture<UbiqOktaStructuredEncryptDecryptFixture>
    {
        private readonly UbiqOktaStructuredEncryptDecryptFixture _fixture;

        public UbiqOktaStructuredEncryptDecryptTests(UbiqOktaStructuredEncryptDecryptFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact(Skip = "For local runs only")]
        public async Task EncryptAsync_ValidInput_ReturnsExpectedCipherText()
        {
            var plainText = "123-45-6789";
            var dataset = "ALPHANUM_SSN";

            var sut = _fixture.UbiqStructuredEncryptDecrypt;

            var actualCipherText = await sut.EncryptAsync(dataset, plainText);

            var actualPlainText = await sut.DecryptAsync(dataset, actualCipherText);

            Assert.Equal(plainText, actualPlainText);
        }
    }
}
