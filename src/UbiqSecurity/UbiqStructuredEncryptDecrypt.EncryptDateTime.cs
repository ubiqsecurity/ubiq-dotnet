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
        // 9,999,999,999 seconds from epoch
        private static readonly DateTime MaxDateTime = new DateTime(2286, 11, 20, 17, 46, 39, DateTimeKind.Utc);

        // -9,999,999,999 seconds from epoch
        private static readonly DateTime MinDateTime = new DateTime(1653, 2, 10, 6, 13, 21, DateTimeKind.Utc);

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
                var cipherText = await EncryptDateTimePipelineAsync(dataset, plainDateTime, tweak);

                results.Add(cipherText);
            }

            return results;
        }

        private async Task<DateTime> EncryptDateTimePipelineAsync(FfsRecord dataset, DateTime plainDateTime, byte[] tweak)
        {
            if (dataset.DataType != "datetime")
            {
                throw new InvalidOperationException($"Dataset '{dataset.Name}' is not a 'datetime' DataType");
            }

            var utcPlainDateTime = plainDateTime.ToUniversalTime();

            if (utcPlainDateTime > MaxDateTime)
            {
                throw new ArgumentOutOfRangeException(nameof(plainDateTime), $"DateTime must be before {MaxDateTime}");
            }

            if (utcPlainDateTime < MinDateTime)
            {
                throw new ArgumentOutOfRangeException(nameof(plainDateTime), $"DateTime must be after {MinDateTime}");
            }

            // convert datetime to number of seconds from the unix epoch (1/1/1970)
            var secondsToEpoch = new DateTimeOffset(utcPlainDateTime).ToUnixTimeSeconds();

            // pad input to 10 characters w/ leading zeroes
            var plainText = secondsToEpoch.ToString(CultureInfo.InvariantCulture);
            plainText = plainText.PadLeft(10, '0');

            // encrypted output will contain base12 characters (0-9a-b)
            var cipherText = await EncryptPipelineAsync(dataset, _ffxCache, plainText, tweak);

            // convert base12 string to base10 int
            var encryptedSecondsToEpoch = IntegerHelper.Parse(cipherText, 12);

            // convert encrypted seconds back to a date time
            var encryptedDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(encryptedSecondsToEpoch);

            return encryptedDateTimeOffset.UtcDateTime;
        }
    }
}
