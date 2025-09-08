namespace UbiqSecurity.Tests.Fixtures
{
#pragma warning disable 0618
    public class UbiqFPEEncryptDecryptFixture : IDisposable
    {
        public UbiqFPEEncryptDecryptFixture()
        {
            InitNormalApiKey();
        }

        public UbiqFPEEncryptDecrypt UbiqFPEEncryptDecrypt { get; private set; }

        public IUbiqCredentials UbiqCredentials { get; private set; }

        private void InitNormalApiKey()
        {
            UbiqCredentials = UbiqSecurity.Internals.UbiqCredentials.CreateFromFile(string.Empty, Environment.GetEnvironmentVariable("JsonTestProfile") ?? "default");
            UbiqFPEEncryptDecrypt = new UbiqFPEEncryptDecrypt(UbiqCredentials);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                UbiqFPEEncryptDecrypt?.Dispose();
            }
        }
    }
#pragma warning restore 0618
}
