using UbiqSecurity.Internals;

namespace UbiqSecurity
{
    public static class UbiqFactory
    {
        private const string DEFAULT_UBIQ_HOST = "api.ubiqsecurity.com";
        private const string DEFAULT_PROFILE = "default";

        public static IUbiqCredentials CreateCredentials(
            string accessKeyId = null,
            string secretSigningKey = null,
            string secretCryptoAccessKey = null,
            string host = DEFAULT_UBIQ_HOST)
        {
            return new UbiqCredentials(accessKeyId, secretSigningKey, secretCryptoAccessKey, host);
        }

        public static IUbiqCredentials ReadCredentialsFromFile(
            string pathname, 
            string profile = DEFAULT_PROFILE)
        {
            return new UbiqCredentials(pathname, profile, DEFAULT_UBIQ_HOST);
        }

        public static IUbiqCredentials CreateIdpCredentials(string idpUsername, string idpPassword, string host = DEFAULT_UBIQ_HOST)
        {
            return new UbiqCredentials()
            {
                Host = host,
                IdpUsername = idpUsername,
                IdpPassword = idpPassword,
            };
        }
    }
}
