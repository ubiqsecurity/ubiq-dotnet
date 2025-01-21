namespace UbiqSecurity.Tests.Fixtures
{
	public class UbiqEncryptDecryptFixture : IDisposable
	{
		public UbiqEncryptDecryptFixture()
		{
			UbiqCredentials = UbiqFactory.ReadCredentialsFromFile(string.Empty);
            UbiqEncrypt = new UbiqEncrypt(UbiqCredentials, 1);
            UbiqDecrypt = new UbiqDecrypt(UbiqCredentials);
        }

		public UbiqEncrypt UbiqEncrypt { get; private set; }

        public UbiqDecrypt UbiqDecrypt { get; private set; }

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
                UbiqEncrypt?.Dispose();
                UbiqDecrypt?.Dispose();
            }
		}
	}
}
