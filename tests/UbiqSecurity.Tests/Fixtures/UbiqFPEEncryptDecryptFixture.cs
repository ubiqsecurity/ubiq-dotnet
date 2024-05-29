namespace UbiqSecurity.Tests.Fixtures
{
	public class UbiqFPEEncryptDecryptFixture : IDisposable
	{
		public UbiqFPEEncryptDecryptFixture()
		{
			UbiqCredentials = UbiqFactory.ReadCredentialsFromFile(string.Empty, Environment.GetEnvironmentVariable("JsonTestProfile") ?? "default");
			UbiqFPEEncryptDecrypt = new UbiqFPEEncryptDecrypt(UbiqCredentials);
		}

		public UbiqFPEEncryptDecrypt UbiqFPEEncryptDecrypt { get; private set; }

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
				UbiqFPEEncryptDecrypt?.Dispose();
			}
		}
	}
}
