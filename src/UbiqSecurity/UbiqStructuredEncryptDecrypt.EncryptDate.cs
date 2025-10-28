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
        public async Task<DateTime> EncryptDateAsync(string datasetName, DateTime plainDateTime)
        {
            return await EncryptDateAsync(datasetName, plainDateTime, null);
        }

        public async Task<DateTime> EncryptDateAsync(string datasetName, DateTime plainDateTime, byte[] tweak)
        {
            var dataset = await _datasetCache.GetAsync(datasetName);

            return await EncryptDatePipelineAsync(dataset, plainDateTime, tweak);
        }

        public async Task<IEnumerable<DateTime>> EncryptDateForSearchAsync(string datasetName, DateTime plainDateTime)
        {
            return await EncryptDateForSearchAsync(datasetName, plainDateTime, null);
        }

        public async Task<IEnumerable<DateTime>> EncryptDateForSearchAsync(string datasetName, DateTime plainDateTime, byte[] tweak)
        {
            await LoadAllKeysAsync(datasetName);

            var dataset = await _datasetCache.GetAsync(datasetName);
            var currentFfx = await _ffxCache.GetAsync(dataset, null);
            var results = new List<DateTime>();

            for (int keyNumber = 0; keyNumber <= currentFfx.KeyNumber; keyNumber++)
            {
                var cipherText = await EncryptDatePipelineAsync(dataset, plainDateTime, tweak);

                results.Add(cipherText);
            }

            return results;
        }

        private async Task<DateTime> EncryptDatePipelineAsync(FfsRecord dataset, DateTime plainDate, byte[] tweak)
        {
            if (dataset.DataType != "date")
            {
                throw new InvalidOperationException($"Dataset '{dataset.Name}' is not a 'date' DataType");
            }

            if (dataset.DataTypeConfig == null)
            {
                throw new InvalidOperationException($"Dataset '{dataset.Name}' is missing data_type_config");
            }

            var utcPlainDate = plainDate.ToUniversalTime().Date;

            if (utcPlainDate > dataset.DataTypeConfig.MaxInputDateValue.Date)
            {
                throw new ArgumentOutOfRangeException(nameof(plainDate), $"plainDate must be <= {dataset.DataTypeConfig.MaxInputDateValue.Date}");
            }

            if (utcPlainDate < dataset.DataTypeConfig.MinInputDateValue.Date)
            {
                throw new ArgumentOutOfRangeException(nameof(plainDate), $"plainDate must be >= {dataset.DataTypeConfig.MinInputDateValue.Date}");
            }

            // convert date to number of days from our epoch, ususally 1/1/0001 but its configurable
            var daysFromEpoch = (utcPlainDate - dataset.DataTypeConfig.Epoch.Date).Days;

            // track if we are dealing w/ a negative sign, to ensure we re-add it after padding
            bool isNegative = daysFromEpoch < 0;

            // pad input to min_input_length w/ leading zeroes
            var plainText = Math.Abs(daysFromEpoch).ToString(CultureInfo.InvariantCulture);
            plainText = plainText.PadLeft(dataset.MinInputLength, '0');

            // re-add negative sign now that we are fully padded
            if (isNegative)
            {
                plainText = $"-{plainText}";
            }

            // encrypted output will contain baseX characters, usually base 12 (e.g. 0-9a-b)
            var cipherText = await EncryptPipelineAsync(dataset, _ffxCache, plainText, tweak);

            // convert baseX string to base10 int
            var encryptedDaysFromEpoch = IntegerHelper.Parse(cipherText, dataset.OutputCharacters.Length);

            // convert encrypted days back to a date time by adding days to our epoch
            var encryptedDate = dataset.DataTypeConfig.Epoch.Date.AddDays(encryptedDaysFromEpoch);

            return encryptedDate;
        }
    }
}
