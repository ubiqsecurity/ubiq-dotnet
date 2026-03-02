using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UbiqSecurity.Internals.Structured.Helpers;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity
{
    // Int32 strongly typed Encrypt methods
    public partial class UbiqStructuredEncryptDecrypt
    {
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
                var cipherText = await EncryptIntegerPipelineAsync(dataset, plainInteger, tweak, keyNumber);

                results.Add(cipherText);
            }

            return results;
        }

        private async Task<int> EncryptIntegerPipelineAsync(FfsRecord dataset, int plainInteger, byte[] tweak, int? keyNumber = null)
        {
            if (dataset.DataType != "integer")
            {
                throw new InvalidOperationException($"Dataset '{dataset.Name}' is not a 'integer' DataType");
            }

            if (dataset.DataTypeConfig == null)
            {
                throw new InvalidOperationException($"Dataset '{dataset.Name}' is missing data_type_config");
            }

            if (dataset.DataTypeConfig.Size != 32)
            {
                throw new InvalidOperationException($"Dataset '{dataset.Name}' does not have a 32-bit DataSize");
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

            return (int)cipherInteger;
        }
    }
}
