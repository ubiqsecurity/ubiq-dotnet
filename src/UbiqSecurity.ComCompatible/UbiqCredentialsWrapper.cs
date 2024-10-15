using System.Runtime.InteropServices;

namespace UbiqSecurity
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("14C934CF-32B6-4829-A65B-8DC4124CD7D8")]
    public class UbiqCredentialsWrapper : IUbiqCredentialsWrapper
    {
        public UbiqCredentialsWrapper() { }

        public string AccessKeyId { get; set; }

        public string SecretSigningKey { get; set; }

        public string SecretCryptoAccessKey { get; set; }

        public string Host { get; set; }
    }
}
