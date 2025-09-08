namespace UbiqSecurity.Tests.Fixtures
{
    public class UbiqStructuredEncryptDecryptFixture : IDisposable
    {
        public UbiqStructuredEncryptDecryptFixture()
        {
            UbiqCredentials = UbiqSecurity.Internals.UbiqCredentials.CreateFromFile(string.Empty, Environment.GetEnvironmentVariable("JsonTestProfile") ?? "default");
            UbiqStructuredEncryptDecrypt = CryptographyBuilder.Create().WithCredentials(UbiqCredentials).BuildStructured();
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
