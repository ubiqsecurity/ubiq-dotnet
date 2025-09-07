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
        // +/- 8 digits
        private const long MaxInt32Input = 99999999;
        private const long MinInt32Input = -99999999;

        public async Task<int> EncryptAsync(string datasetName, int plainInteger)
        {
            return await EncryptAsync(datasetName, plainInteger, null);
        }

        public async Task<int> EncryptAsync(string datasetName, int plainInteger, byte[] tweak)
        {
            var dataset = await _datasetCache.GetAsync(datasetName);

            return await EncryptIntegerPipelineAsync(dataset, plainInteger, tweak);
        }

        public async Task<IEnumerable<int>> EncryptForSearchAsync(string datasetName, int plainInteger)
        {
            return await EncryptForSearchAsync(datasetName, plainInteger, null);
        }

        public async Task<IEnumerable<int>> EncryptForSearchAsync(string datasetName, int plainInteger, byte[] tweak)
        {
            await LoadAllKeysAsync(datasetName);

            var dataset = await _datasetCache.GetAsync(datasetName);
            var currentFfx = await _ffxCache.GetAsync(dataset, null);
            var results = new List<int>();

            for (int keyNumber = 0; keyNumber <= currentFfx.KeyNumber; keyNumber++)
            {
                var cipherText = await EncryptIntegerPipelineAsync(dataset, plainInteger, tweak);

                results.Add(cipherText);
            }

            return results;
        }

        private async Task<int> EncryptIntegerPipelineAsync(FfsRecord dataset, int plainInteger, byte[] tweak)
        {
            if (dataset.DataType != "integer")
            {
                throw new InvalidOperationException($"Dataset '{dataset.Name}' is not a 'integer' DataType");
            }

            if (dataset.DataSize != 32)
            {
                throw new InvalidOperationException($"Dataset '{dataset.Name}' does not have a 32-bit DataSize");
            }

            if (plainInteger > MaxInt32Input)
            {
                throw new ArgumentOutOfRangeException(nameof(plainInteger), $"Number must be < {MaxInt32Input}");
            }

            if (plainInteger < MinInt32Input)
            {
                throw new ArgumentOutOfRangeException(nameof(plainInteger), $"Number must be > {MinInt32Input}");
            }

            // convert to string and pad to 8 characters after the negative sign
            var plainText = plainInteger.ToString("D8", CultureInfo.InvariantCulture);

            // encrypted output will contain base14 characters (0-9a-d)
            var cipherText = await EncryptPipelineAsync(dataset, _ffxCache, plainText, tweak);

            // convert base14 string to base10 int
            var cipherInteger = IntegerHelper.Parse(cipherText, 14);

            return (int)cipherInteger;
        }
    }
}
