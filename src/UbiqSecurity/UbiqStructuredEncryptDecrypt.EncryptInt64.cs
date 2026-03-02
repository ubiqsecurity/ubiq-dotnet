using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UbiqSecurity.Internals.Structured.Helpers;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity
{
    // Int64 strongly typed Decrypt methods
    public partial class UbiqStructuredEncryptDecrypt
    {
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
                var cipherText = await EncryptIntegerPipelineAsync(dataset, plainInteger, tweak, keyNumber);

                results.Add(cipherText);
            }

            return results;
        }

        private async Task<long> EncryptIntegerPipelineAsync(FfsRecord dataset, long plainInteger, byte[] tweak, int? keyNumber = null)
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

            if (plainInteger > dataset.DataTypeConfig.MaxInputIntValue)
            {
                throw new ArgumentOutOfRangeException(nameof(plainInteger), $"Number must be <= {dataset.DataTypeConfig.MaxInputIntValue}");
            }

            if (plainInteger < dataset.DataTypeConfig.MinInputIntValue)
            {
                throw new ArgumentOutOfRangeException(nameof(plainInteger), $"Number must be >= {dataset.DataTypeConfig.MinInputIntValue}");
            }

            // convert to string and pad to min_input_length after the negative sign
            var plainText = plainInteger.ToString($"D{dataset.MinInputLength}", CultureInfo.InvariantCulture);

            // encrypted output will contain base14 characters (0-9a-d)
            var cipherText = await EncryptPipelineAsync(dataset, _ffxCache, plainText, tweak, keyNumber);

            // convert base14 string to base10 int
            var cipherInteger = IntegerHelper.Parse(cipherText, dataset.OutputCharacters.Length);

            return cipherInteger;
        }
    }
}
