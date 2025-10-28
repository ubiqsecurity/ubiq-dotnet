using System;
using System.Globalization;
using System.Threading.Tasks;
using UbiqSecurity.Internals.Structured.Helpers;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity
{
    public partial class UbiqStructuredEncryptDecrypt
    {
        public async Task<DateTime> DecryptDateAsync(string datasetName, DateTime cipherDateTime)
        {
            return await DecryptDateAsync(datasetName, cipherDateTime, null);
        }

        public async Task<DateTime> DecryptDateAsync(string datasetName, DateTime cipherDateTime, byte[] tweak)
        {
            var dataset = await _datasetCache.GetAsync(datasetName);

            return await DecryptDatePipelineAsync(dataset, cipherDateTime, tweak);
        }

        private async Task<DateTime> DecryptDatePipelineAsync(FfsRecord dataset, DateTime encryptedDate, byte[] tweak)
        {
            if (dataset.DataType != "date")
            {
                throw new InvalidOperationException($"Dataset '{dataset.Name}' is not a 'date' DataType");
            }

            if (dataset.DataTypeConfig == null)
            {
                throw new InvalidOperationException($"Dataset '{dataset.Name}' is missing data_type_config");
            }

            // convert datetime to number of days from our epoch (1/1/0001) aka DateTime.MinValue
            var encryptedDaysFromEpoch = (encryptedDate - dataset.DataTypeConfig.Epoch.Date).Days;

            bool isNegative = encryptedDaysFromEpoch < 0;

            // convert from base 10 to baseX
            var cipherText = IntegerHelper.ToString(Math.Abs(encryptedDaysFromEpoch), dataset.OutputCharacters.Length);

            // left pad to min_input_length
            cipherText = cipherText.PadLeft(dataset.MinInputLength, '0');

            // re-add negative sign, if needed
            if (isNegative)
            {
                cipherText = "-" + cipherText;
            }

            // ciphertext is base 12, but plaintext will be base 10
            var plainText = await DecryptPipelineAsync(dataset, _ffxCache, cipherText, tweak);

            // convert base10 string to int
            var plainDaysFromEpoch = long.Parse(plainText, CultureInfo.InvariantCulture);

            // convert decrypted seconds back to a date time
            var plainDateTime = dataset.DataTypeConfig.Epoch.Date.AddDays(plainDaysFromEpoch);

            return plainDateTime;
        }
    }
}
