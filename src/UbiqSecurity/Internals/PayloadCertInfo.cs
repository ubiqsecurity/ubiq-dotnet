using System;

namespace UbiqSecurity.Internals
{
    internal partial class PayloadEncryption
    {
        public struct PayloadCertInfo
        {
            public string CsrPem { get; set; }

            public string EncryptedPrivateKey { get; set; }

            public string ApiCert { get; set; }

            public DateTime? ApiCertExpiration { get; set; }
        }
    }
}
