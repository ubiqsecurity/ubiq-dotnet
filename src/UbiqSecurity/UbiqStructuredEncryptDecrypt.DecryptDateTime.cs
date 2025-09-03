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

            // convert datetime to number of seconds from the unix epoch (1/1/1970)
            var cipherSecondsToEpoch = new DateTimeOffset(cipherDateTime).ToUnixTimeSeconds();

            // convert from base 10 to base 12 and pad to 10 digits
            var cipherText = IntegerHelper.ToString(cipherSecondsToEpoch, 12);
            cipherText = cipherText.PadLeft(10, '0');

            // ciphertext is base 12, but plaintext will be base 10
            var plainText = await DecryptPipelineAsync(dataset, _ffxCache, cipherText, tweak);

            // convert base10 string to int
            var plainSecondsToEpoch = long.Parse(plainText, CultureInfo.InvariantCulture);

            // convert decrypted seconds back to a date time
            var plainDateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(plainSecondsToEpoch);

            return plainDateTimeOffset.UtcDateTime;
        }
    }
}
