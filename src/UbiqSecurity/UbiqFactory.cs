using System;
using UbiqSecurity.Internals;

namespace UbiqSecurity
{
    [Obsolete("Use CryptographyBuilder instead")]
    public static class UbiqFactory
    {
        private const string DefaultHost = "api.ubiqsecurity.com";
        private const string DefaultProfile = "default";

        public static IUbiqCredentials CreateCredentials(
            string accessKeyId = null,
            string secretSigningKey = null,
            string secretCryptoAccessKey = null,
            string host = DefaultHost)
        {
            return new UbiqCredentials(accessKeyId, secretSigningKey, secretCryptoAccessKey, host);
        }

        public static IUbiqCredentials ReadCredentialsFromFile(
            string pathname,
            string profile = DefaultProfile)
        {
            return new UbiqCredentials(pathname, profile, DefaultHost);
        }

        public static IUbiqCredentials CreateIdpCredentials(string idpUsername, string idpPassword, string host = DefaultHost)
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
