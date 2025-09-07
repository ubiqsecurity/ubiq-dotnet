using System;
using System.Globalization;
using System.Threading.Tasks;
using UbiqSecurity.Internals.Structured.Helpers;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity
{
    public partial class UbiqStructuredEncryptDecrypt
    {
        public async Task<long> DecryptAsync(string datasetName, long cipherInteger)
        {
            return await DecryptAsync(datasetName, cipherInteger, null);
        }

        public async Task<long> DecryptAsync(string datasetName, long cipherInteger, byte[] tweak)
        {
            var dataset = await _datasetCache.GetAsync(datasetName);

            return await DecryptIntegerPipelineAsync(dataset, cipherInteger, tweak);
        }

        private async Task<long> DecryptIntegerPipelineAsync(FfsRecord dataset, long cipherInteger, byte[] tweak)
        {
            if (dataset.DataType != "integer")
            {
                throw new InvalidOperationException($"Dataset '{dataset.Name}' is not a 'integer' DataType");
            }

            bool isNegative = cipherInteger < 0;

            // convert from base 10 to base 14
            var cipherText = IntegerHelper.ToString(Math.Abs(cipherInteger), 14);

            // left pad to 16 characters
            cipherText = cipherText.PadLeft(16, '0');

            // re-add negative sign, if needed
            if (isNegative)
            {
                cipherText = "-" + cipherText;
            }

            // ciphertext is base 14, but plaintext will be base 10
            var plainText = await DecryptPipelineAsync(dataset, _ffxCache, cipherText, tweak);

            var plainInteger = long.Parse(plainText, CultureInfo.InvariantCulture);

            return plainInteger;
        }
    }
}
