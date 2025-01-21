namespace UbiqSecurity.Tests.Fixtures
{
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
            UbiqCredentials = UbiqFactory.ReadCredentialsFromFile(string.Empty, Environment.GetEnvironmentVariable("JsonTestProfile") ?? "default");
            UbiqFPEEncryptDecrypt = new UbiqFPEEncryptDecrypt(UbiqCredentials);
        }

        private void InitSsoApiKey()
        {
            UbiqCredentials = UbiqFactory.ReadCredentialsFromFile(string.Empty, "local-idp");

            var config = new UbiqConfiguration()
            {
                Idp = new Config.IdpConfig()
                {
                    CustomerId = "7ba81eb5-90bc-408d-8cb8-4ad9ce28e467",
                    IdpClientSecret = "rTh8Q~Z9fhRCC7.SI~z4lnxSnOIVljNcW-XiqdAT",
                    IdpTokenEndpointUrl = "https://login.microsoftonline.com/313d11d1-3fbe-4faf-91a9-3421c84c0928/oauth2/token",
                    IdpTenantId = "6be71235-6c0b-4555-ad8b-4c22113c46ef"
                }
            };

            UbiqFPEEncryptDecrypt = new UbiqFPEEncryptDecrypt(UbiqCredentials, config);
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
}
