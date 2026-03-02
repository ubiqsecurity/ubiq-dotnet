using System;
using System.Globalization;
using System.Threading.Tasks;
using UbiqSecurity.Internals.Structured.Helpers;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity
{
    // Int64 strongly typed Decrypt methods
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

            if (dataset.DataTypeConfig == null)
            {
                throw new InvalidOperationException($"Dataset '{dataset.Name}' is missing data_type_config");
            }

            if (dataset.DataTypeConfig.Size != 64)
            {
                throw new InvalidOperationException($"Dataset '{dataset.Name}' does not have a 64-bit DataSize");
            }

            bool isNegative = cipherInteger < 0;

            // convert from base 10 to base 14
            var cipherText = IntegerHelper.ToString(Math.Abs(cipherInteger), dataset.OutputCharacters.Length);

            // left pad to min_input_length
            cipherText = cipherText.PadLeft(dataset.MinInputLength, '0');

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
