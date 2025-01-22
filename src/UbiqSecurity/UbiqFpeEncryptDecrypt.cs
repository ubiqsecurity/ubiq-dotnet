using System;
using System.Threading.Tasks;

namespace UbiqSecurity
{
    public class UbiqFPEEncryptDecrypt : UbiqStructuredEncryptDecrypt
    {
        public UbiqFPEEncryptDecrypt(IUbiqCredentials ubiqCredentials)
            : base(ubiqCredentials, new UbiqConfiguration())
        {
        }

        public UbiqFPEEncryptDecrypt(IUbiqCredentials ubiqCredentials, UbiqConfiguration ubiqConfiguration)
            : base(ubiqCredentials, ubiqConfiguration)
        {
        }

        [Obsolete("Static DecryptAsync method is deprecated, please use equivalent instance method")]
        public static async Task<string> DecryptAsync(IUbiqCredentials ubiqCredentials, string inputText, string datasetName)
        {
            return await DecryptAsync(ubiqCredentials, inputText, datasetName, null);
        }

        [Obsolete("Static DecryptAsync method is deprecated, please use equivalent instance method")]
        public static async Task<string> DecryptAsync(IUbiqCredentials ubiqCredentials, string inputText, string datasetName, byte[] tweak)
        {
            var response = string.Empty;
            using (var ubiqEncrypt = new UbiqFPEEncryptDecrypt(ubiqCredentials))
            {
                response = await ubiqEncrypt.DecryptAsync(datasetName, inputText, tweak);
            }

            return response;
        }

        [Obsolete("Static EncryptAsync method is deprecated, please use equivalent instance method")]
        public static async Task<string> EncryptAsync(IUbiqCredentials ubiqCredentials, string inputText, string datasetName)
        {
            return await EncryptAsync(ubiqCredentials, inputText, datasetName, null);
        }

        [Obsolete("Static EncryptAsync method is deprecated, please use equivalent instance method")]
        public static async Task<string> EncryptAsync(IUbiqCredentials ubiqCredentials, string inputText, string datasetName, byte[] tweak)
        {
            var response = string.Empty;
            using (var ubiqEncrypt = new UbiqFPEEncryptDecrypt(ubiqCredentials))
            {
                response = await ubiqEncrypt.EncryptAsync(datasetName, inputText, tweak);
            }

            return response;
        }
    }
}
