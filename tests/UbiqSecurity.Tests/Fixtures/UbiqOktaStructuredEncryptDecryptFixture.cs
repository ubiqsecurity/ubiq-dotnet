using UbiqSecurity.Internals;

namespace UbiqSecurity.Tests.Fixtures
{
    public class UbiqOktaStructuredEncryptDecryptFixture : IDisposable
    {
        public UbiqOktaStructuredEncryptDecryptFixture()
        {
            UbiqCredentials = UbiqFactory.ReadCredentialsFromFile(string.Empty, "okta-idp");

            UbiqStructuredEncryptDecrypt = CryptographyBuilder
                                                .Create()
                                                .WithCredentials(UbiqCredentials)
                                                .WithConfig("./okta.json")
                                                .BuildStructured();
        }

        public UbiqStructuredEncryptDecrypt UbiqStructuredEncryptDecrypt { get; private set; }

        public IUbiqCredentials UbiqCredentials { get; private set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                UbiqStructuredEncryptDecrypt?.Dispose();
            }
        }
    }
}
