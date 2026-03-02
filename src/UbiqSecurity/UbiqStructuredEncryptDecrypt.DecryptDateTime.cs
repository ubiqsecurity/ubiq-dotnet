using System;
using System.Globalization;
using System.Threading.Tasks;
using UbiqSecurity.Internals.Structured.Helpers;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity
{
    public partial class UbiqStructuredEncryptDecrypt
    {
        public async Task<DateTime> DecryptAsync(string datasetName, DateTime cipherDateTime)
        {
            return await DecryptAsync(datasetName, cipherDateTime, null);
        }

        public async Task<DateTime> DecryptAsync(string datasetName, DateTime cipherDateTime, byte[] tweak)
        {
            var dataset = await _datasetCache.GetAsync(datasetName);

            return await DecryptDateTimePipelineAsync(dataset, cipherDateTime, tweak);
        }

        private async Task<DateTime> DecryptDateTimePipelineAsync(FfsRecord dataset, DateTime cipherDateTime, byte[] tweak)
        {
            if (dataset.DataType != "datetime")
            {
                throw new InvalidOperationException($"Dataset '{dataset.Name}' is not a 'datetime' DataType");
            }

            if (dataset.DataTypeConfig == null)
            {
                throw new InvalidOperationException($"Dataset '{dataset.Name}' is missing data_type_config");
            }

            var utcCipherDateTime = cipherDateTime.ToUniversalTime();

            // track timespan between UTC and original datetime, so we can adjust the encrypted date/time by the same amount
            var utcOffsetTimeSpan = utcCipherDateTime.Subtract(cipherDateTime);

            // convert datetime to number of seconds from the epoch, usually 1/1/1970 but is configurable
            var cipherSecondsToEpoch = (long)(utcCipherDateTime - dataset.DataTypeConfig.Epoch).TotalSeconds;

            // preserve negative sign for later, after padding
            bool isNegative = cipherSecondsToEpoch < 0;

            // convert from base 10 to baseX
            var cipherText = IntegerHelper.ToString(Math.Abs(cipherSecondsToEpoch), dataset.OutputCharacters.Length);

            // left pad to input_min_length
            cipherText = cipherText.PadLeft(dataset.MinInputLength, '0');

            // re-add negative sign, if needed
            if (isNegative)
            {
                cipherText = "-" + cipherText;
            }

            // ciphertext is baseX, but plaintext will be base 10
            var plainText = await DecryptPipelineAsync(dataset, _ffxCache, cipherText, tweak);

            // convert base10 string to int
            var plainSecondsToEpoch = long.Parse(plainText, CultureInfo.InvariantCulture);

            // convert decrypted seconds back to a date time
            var plainDateTime = dataset.DataTypeConfig.Epoch.AddSeconds(plainSecondsToEpoch);

            var localPlainDateTime = plainDateTime.Subtract(utcOffsetTimeSpan);

            return DateTime.SpecifyKind(localPlainDateTime, cipherDateTime.Kind);
        }
    }
}
