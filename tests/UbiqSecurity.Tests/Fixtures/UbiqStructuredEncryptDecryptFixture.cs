namespace UbiqSecurity.Tests.Fixtures
{
    public class UbiqStructuredEncryptDecryptFixture : IDisposable
	{
		public UbiqStructuredEncryptDecryptFixture()
		{
            InitNormalApiKey();
		}

		public UbiqStructuredEncryptDecrypt UbiqStructuredEncryptDecrypt { get; private set; }

		public IUbiqCredentials UbiqCredentials { get; private set; }

        private void InitNormalApiKey()
        {
            UbiqCredentials = UbiqFactory.ReadCredentialsFromFile(string.Empty, Environment.GetEnvironmentVariable("JsonTestProfile") ?? "default");
            UbiqStructuredEncryptDecrypt = new UbiqStructuredEncryptDecrypt(UbiqCredentials);
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
                UbiqStructuredEncryptDecrypt?.Dispose();
			}
		}
	}
}
