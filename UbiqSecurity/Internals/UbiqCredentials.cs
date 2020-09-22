using System;
using System.IO;

namespace UbiqSecurity.Internals
{
    internal class UbiqCredentials : IUbiqCredentials
    {
        // Environment variable names
        private const string UBIQ_ACCESS_KEY_ID = nameof(UBIQ_ACCESS_KEY_ID);
        private const string UBIQ_SECRET_SIGNING_KEY = nameof(UBIQ_SECRET_SIGNING_KEY);
        private const string UBIQ_SECRET_CRYPTO_ACCESS_KEY = nameof(UBIQ_SECRET_CRYPTO_ACCESS_KEY);

        #region Constructors
        internal UbiqCredentials(string accessKeyId, string secretSigningKey, string secretCryptoAccessKey, string host)
        {
            if (string.IsNullOrEmpty(accessKeyId))
            {
                accessKeyId = Environment.GetEnvironmentVariable(UBIQ_ACCESS_KEY_ID);
            }
            AccessKeyId = accessKeyId;

            if (string.IsNullOrEmpty(secretSigningKey))
            {
                secretSigningKey = Environment.GetEnvironmentVariable(UBIQ_SECRET_SIGNING_KEY);
            }
            SecretSigningKey = secretSigningKey;

            if (string.IsNullOrEmpty(secretCryptoAccessKey))
            {
                secretCryptoAccessKey = Environment.GetEnvironmentVariable(UBIQ_SECRET_CRYPTO_ACCESS_KEY);
            }
            SecretCryptoAccessKey = secretCryptoAccessKey;

            Host = host;
        }

        internal UbiqCredentials(string pathname, string profile, string host)
        {
            const string DEFAULT_SECTION = "default";
            const string ACCESS_KEY_ID = "access_key_id";
            const string SECRET_SIGNING_KEY = "secret_signing_key";
            const string SECRET_CRYPTO_ACCESS_KEY = "secret_crypto_access_key";
            const string SERVER_KEY = "server";

            if (string.IsNullOrEmpty(pathname))
            {
                // credentials file not specified, so look for ~/.ubiq/credentials
                string homeDirectory = (Environment.OSVersion.Platform == PlatformID.Unix ||
                        Environment.OSVersion.Platform == PlatformID.MacOSX)
                    ? Environment.GetEnvironmentVariable("HOME")
                    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

                if (Directory.Exists(homeDirectory))
                {
                    pathname = Path.Combine(homeDirectory, ".ubiq", "credentials");
                }
            }

            var configParser = new ConfigParser(pathname);
            AccessKeyId = configParser.GetValue(profile, ACCESS_KEY_ID) ?? configParser.GetValue(DEFAULT_SECTION, ACCESS_KEY_ID);
            SecretSigningKey = configParser.GetValue(profile, SECRET_SIGNING_KEY) ?? configParser.GetValue(DEFAULT_SECTION, SECRET_SIGNING_KEY);
            SecretCryptoAccessKey = configParser.GetValue(profile, SECRET_CRYPTO_ACCESS_KEY) ?? configParser.GetValue(DEFAULT_SECTION, SECRET_CRYPTO_ACCESS_KEY);
            Host = configParser.GetValue(profile, SERVER_KEY) ?? configParser.GetValue(DEFAULT_SECTION, SERVER_KEY) ?? host;
        }
        #endregion

        #region IUbiqCredentials
        public string AccessKeyId { get; }

        public string SecretSigningKey { get; }

        public string SecretCryptoAccessKey { get; }

        public string Host { get; }
        #endregion
    }
}
