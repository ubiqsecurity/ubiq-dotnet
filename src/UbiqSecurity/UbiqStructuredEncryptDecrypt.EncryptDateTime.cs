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
        public async Task<DateTime> EncryptAsync(string datasetName, DateTime plainDateTime)
        {
            return await EncryptAsync(datasetName, plainDateTime, null);
        }

        public async Task<DateTime> EncryptAsync(string datasetName, DateTime plainDateTime, byte[] tweak)
        {
            var dataset = await _datasetCache.GetAsync(datasetName);

            return await EncryptDateTimePipelineAsync(dataset, plainDateTime, tweak);
        }

        public async Task<IEnumerable<DateTime>> EncryptForSearchAsync(string datasetName, DateTime plainDateTime)
        {
            return await EncryptForSearchAsync(datasetName, plainDateTime, null);
        }

        public async Task<IEnumerable<DateTime>> EncryptForSearchAsync(string datasetName, DateTime plainDateTime, byte[] tweak)
        {
            await LoadAllKeysAsync(datasetName);

            var dataset = await _datasetCache.GetAsync(datasetName);
            var currentFfx = await _ffxCache.GetAsync(dataset, null);
            var results = new List<DateTime>();

            for (int keyNumber = 0; keyNumber <= currentFfx.KeyNumber; keyNumber++)
            {
                var cipherText = await EncryptDateTimePipelineAsync(dataset, plainDateTime, tweak, keyNumber);

                results.Add(cipherText);
            }

            return results;
        }

        private async Task<DateTime> EncryptDateTimePipelineAsync(FfsRecord dataset, DateTime plainDateTime, byte[] tweak, int? keyNumber = null)
        {
            if (dataset.DataType != "datetime")
            {
                throw new InvalidOperationException($"Dataset '{dataset.Name}' is not a 'datetime' DataType");
            }

            if (dataset.DataTypeConfig == null)
            {
                throw new InvalidOperationException($"Dataset '{dataset.Name}' is missing data_type_config");
            }

            var utcPlainDateTime = plainDateTime.ToUniversalTime();

            // track timespan between UTC and original datetime, so we can adjust the encrypted date/time by the same amount
            var utcOffsetTimeSpan = utcPlainDateTime.Subtract(plainDateTime);

            if (utcPlainDateTime > dataset.DataTypeConfig.MaxInputDateValue)
            {
                throw new ArgumentOutOfRangeException(nameof(plainDateTime), $"plainDateTime must be <= {dataset.DataTypeConfig.MaxInputDateValue}, {plainDateTime}");
            }

            if (utcPlainDateTime < dataset.DataTypeConfig.MinInputDateValue)
            {
                throw new ArgumentOutOfRangeException(nameof(plainDateTime), $"plainDateTime must be >= {dataset.DataTypeConfig.MinInputDateValue}");
            }

            // convert datetime to number of seconds from our epoch, ususually 1/1/1970 but its configurable
            var secondsToEpoch = (long)(utcPlainDateTime - dataset.DataTypeConfig.Epoch).TotalSeconds;

            // track if we are dealing w/ a negative sign, to ensure we re-add it after padding
            bool isNegative = secondsToEpoch < 0;

            // pad input to min_input_length w/ leading zeroes
            var plainText = Math.Abs(secondsToEpoch).ToString(CultureInfo.InvariantCulture);
            plainText = plainText.PadLeft(dataset.MinInputLength, '0');

            // re-add negative sign now that we are fully padded
            if (isNegative)
            {
                plainText = $"-{plainText}";
            }

            // encrypted output will contain baseX (usually base12, 0-9a-b) characters
            var cipherText = await EncryptPipelineAsync(dataset, _ffxCache, plainText, tweak, keyNumber);

            // convert baseX string to base10 int
            var encryptedSecondsToEpoch = IntegerHelper.Parse(cipherText, dataset.OutputCharacters.Length);

            // convert encrypted seconds back to a datetime by adding the seconds to our epoch
            var encryptedDateTime = dataset.DataTypeConfig.Epoch.AddSeconds(encryptedSecondsToEpoch);

            var localEncryptedDateTime = encryptedDateTime.Subtract(utcOffsetTimeSpan);

            return DateTime.SpecifyKind(localEncryptedDateTime, plainDateTime.Kind);
        }
    }
}
