using System.Runtime.InteropServices;
using UbiqSecurity.Internals;

namespace UbiqSecurity.ComCompatible
{
    /// <summary>
    /// Factory used to create Ubiq objects
    /// </summary>
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("CA305EFA-1225-4FF4-9609-130C9D57F800")]
    public class UbiqFactoryWrapper : IUbiqFactoryWrapper
    {
        public UbiqFactoryWrapper()
        {
        }

        /// <summary>
        /// Creates UbiqCredentialsWrapper using manually specified credentials
        /// </summary>
        /// <param name="accessKeyId"></param>
        /// <param name="secretSigningKey"></param>
        /// <param name="secretCryptoAccessKey"></param>
        /// <returns></returns>
        public UbiqCredentialsWrapper CreateCredentials(
            string accessKeyId,
            string secretSigningKey,
            string secretCryptoAccessKey)
        {
            return new UbiqCredentialsWrapper
            {
                AccessKeyId = accessKeyId,
                SecretCryptoAccessKey = secretCryptoAccessKey,
                SecretSigningKey = secretSigningKey,
            };
        }

        /// <summary>
        /// Creates UbiqCredentialsWrapper by loading credentials from a file
        /// </summary>
        /// <param name="pathname">Path to ubiq credentials file. If null/empty, will try to auto-find the file in common locations</param>
        /// <param name="profile">Name of profile in ubiq credentials file to use</param>
        /// <returns></returns>
        public UbiqCredentialsWrapper ReadCredentialsFromFile(string pathname, string profile)
        {
            var creds = UbiqCredentials.CreateFromFile(pathname, profile);
            creds.Validate();

            return new UbiqCredentialsWrapper
            {
                AccessKeyId = creds.AccessKeyId,
                SecretCryptoAccessKey = creds.SecretCryptoAccessKey,
                SecretSigningKey = creds.SecretSigningKey,
            };
        }

        public UbiqEncryptWrapper CreateEncrypt(UbiqCredentialsWrapper ubiqCredentials, int usesRequested)
        {
            var creds = new UbiqCredentials()
            {
                AccessKeyId = ubiqCredentials.AccessKeyId,
                SecretCryptoAccessKey = ubiqCredentials.SecretCryptoAccessKey,
                SecretSigningKey = ubiqCredentials.SecretSigningKey,
                Host = ubiqCredentials.Host,
                IdpPassword = ubiqCredentials.IdpPassword,
                IdpUsername = ubiqCredentials.IdpUsername,
            };

            return new UbiqEncryptWrapper(creds, usesRequested, new UbiqConfiguration());
        }

        public UbiqDecryptWrapper CreateDecrypt(UbiqCredentialsWrapper ubiqCredentials)
        {
            var creds = new UbiqCredentials()
            {
                AccessKeyId = ubiqCredentials.AccessKeyId,
                SecretCryptoAccessKey = ubiqCredentials.SecretCryptoAccessKey,
                SecretSigningKey = ubiqCredentials.SecretSigningKey,
                Host = ubiqCredentials.Host,
                IdpPassword = ubiqCredentials.IdpPassword,
                IdpUsername = ubiqCredentials.IdpUsername,
            };

            return new UbiqDecryptWrapper(creds, new UbiqConfiguration());
        }

        public UbiqFpeEncryptDecryptWrapper CreateFpeEncryptDecrypt(UbiqCredentialsWrapper ubiqCredentials)
        {
            var creds = new UbiqCredentials()
            {
                AccessKeyId = ubiqCredentials.AccessKeyId,
                SecretCryptoAccessKey = ubiqCredentials.SecretCryptoAccessKey,
                SecretSigningKey = ubiqCredentials.SecretSigningKey,
                Host = ubiqCredentials.Host,
                IdpPassword = ubiqCredentials.IdpPassword,
                IdpUsername = ubiqCredentials.IdpUsername,
            };

            return new UbiqFpeEncryptDecryptWrapper(creds, new UbiqConfiguration());
        }
    }
}
