using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UbiqSecurity.Internals.Structured.Helpers;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity
{
    public partial class UbiqStructuredEncryptDecrypt
    {
        // +/- 16 digits
        private const long MaxInt64Input = 9999999999999999;
        private const long MinInt64Input = -9999999999999999;

        public async Task<long> EncryptAsync(string datasetName, long plainInteger)
        {
            return await EncryptAsync(datasetName, plainInteger, null);
        }

        public async Task<long> EncryptAsync(string datasetName, long plainInteger, byte[] tweak)
        {
            var dataset = await _datasetCache.GetAsync(datasetName);

            return await EncryptIntegerPipelineAsync(dataset, plainInteger, tweak);
        }

        public async Task<IEnumerable<long>> EncryptForSearchAsync(string datasetName, long plainInteger)
        {
            return await EncryptForSearchAsync(datasetName, plainInteger, null);
        }

        public async Task<IEnumerable<long>> EncryptForSearchAsync(string datasetName, long plainInteger, byte[] tweak)
        {
            await LoadAllKeysAsync(datasetName);

            var dataset = await _datasetCache.GetAsync(datasetName);
            var currentFfx = await _ffxCache.GetAsync(dataset, null);
            var results = new List<long>();

            for (int keyNumber = 0; keyNumber <= currentFfx.KeyNumber; keyNumber++)
            {
                var cipherText = await EncryptIntegerPipelineAsync(dataset, plainInteger, tweak);

                results.Add(cipherText);
            }

            return results;
        }

        private async Task<long> EncryptIntegerPipelineAsync(FfsRecord dataset, long plainInteger, byte[] tweak)
        {
            if (dataset.DataType != "integer")
            {
                throw new InvalidOperationException($"Dataset '{dataset.Name}' is not a 'integer' DataType");
            }

            if (dataset.DataSize != 64)
            {
                throw new InvalidOperationException($"Dataset '{dataset.Name}' does not have a 64-bit DataSize");
            }

            if (plainInteger > MaxInt64Input)
            {
                throw new ArgumentOutOfRangeException(nameof(plainInteger), $"Number must be < {MaxInt64Input}");
            }

            if (plainInteger < MinInt64Input)
            {
                throw new ArgumentOutOfRangeException(nameof(plainInteger), $"Number must be > {MinInt64Input}");
            }

            // convert to string and pad to 8 characters after the negative sign
            var plainText = plainInteger.ToString("D16", CultureInfo.InvariantCulture);

            // encrypted output will contain base14 characters (0-9a-d)
            var cipherText = await EncryptPipelineAsync(dataset, _ffxCache, plainText, tweak);

            // convert base14 string to base10 int
            var cipherInteger = IntegerHelper.Parse(cipherText, 14);

            return cipherInteger;
        }
    }
}
