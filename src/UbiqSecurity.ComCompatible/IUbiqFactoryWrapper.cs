using System.Runtime.InteropServices;

namespace UbiqSecurity.ComCompatible
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    [Guid("15689EF5-17A1-4A72-A7F3-4C9F61E04484")]
    public interface IUbiqFactoryWrapper
    {
        private const string DEFAULT_PROFILE = "default";

        public UbiqCredentialsWrapper CreateCredentials(string accessKeyId, string secretSigningKey, string secretCryptoAccessKey);

        public UbiqCredentialsWrapper ReadCredentialsFromFile(string pathname, string profile = DEFAULT_PROFILE);

        public UbiqEncryptWrapper CreateEncrypt(UbiqCredentialsWrapper ubiqCredentials, int usesRequested);

        public UbiqDecryptWrapper CreateDecrypt(UbiqCredentialsWrapper ubiqCredentials);

        public UbiqFpeEncryptDecryptWrapper CreateFpeEncryptDecrypt(UbiqCredentialsWrapper ubiqCredentials);
    }
}
