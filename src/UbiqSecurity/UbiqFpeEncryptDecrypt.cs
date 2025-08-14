using System;
using System.Threading.Tasks;
using UbiqSecurity.Internals.Billing;
using UbiqSecurity.Internals.Cache;
using UbiqSecurity.Internals.WebService;

namespace UbiqSecurity
{
    [Obsolete("Use UbiqStructuredEncryptDecrypt via new CryptoBuilder().BuildStructured()")]
    public class UbiqFPEEncryptDecrypt : UbiqStructuredEncryptDecrypt
    {
        [Obsolete("Use CryptoBuilder .BuildStructured()")]
        public UbiqFPEEncryptDecrypt(IUbiqCredentials ubiqCredentials)
            : base(ubiqCredentials, new UbiqConfiguration())
        {
        }

        [Obsolete("Use CryptoBuilder .BuildStructured()")]
        public UbiqFPEEncryptDecrypt(IUbiqCredentials ubiqCredentials, UbiqConfiguration ubiqConfiguration)
            : base(ubiqCredentials, ubiqConfiguration)
        {
        }

        internal UbiqFPEEncryptDecrypt(IUbiqCredentials ubiqCredentials, IUbiqWebService webService, IBillingEventsManager billingEventsManager, IFfxCache ffxCache, IDatasetCache datasetCache)
            : base(ubiqCredentials, webService, billingEventsManager, ffxCache, datasetCache)
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
