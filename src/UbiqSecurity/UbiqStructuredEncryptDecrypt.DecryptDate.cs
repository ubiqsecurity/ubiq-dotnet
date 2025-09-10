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

            // convert datetime to number of days from our epoch (1/1/0001) aka DateTime.MinValue
            var encryptedDaysFromEpoch = (encryptedDate - MinDate).Days;

            // convert from base 10 to base 12
            var cipherText = IntegerHelper.ToString(encryptedDaysFromEpoch, 12);

            // left pad to 6 characters
            cipherText = cipherText.PadLeft(6, '0');

            // ciphertext is base 12, but plaintext will be base 10
            var plainText = await DecryptPipelineAsync(dataset, _ffxCache, cipherText, tweak);

            // convert base10 string to int
            var plainDaysFromEpoch = long.Parse(plainText, CultureInfo.InvariantCulture);

            // convert decrypted seconds back to a date time
            var plainDateTime = DateTime.MinValue.Date.AddDays(plainDaysFromEpoch);

            return plainDateTime;
        }
    }
}
