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
        // dotnet's minimum date is 1/1/0001 :(
        private static readonly DateTime MinDate = DateTime.MinValue.Date;

        // MinDate + 999,999 days = 11/28/2738
        private static readonly DateTime MaxDate = MinDate.AddDays(999999);

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

            var utcPlainDate = plainDate.ToUniversalTime().Date;

            if (utcPlainDate > MaxDate)
            {
                throw new ArgumentOutOfRangeException(nameof(plainDate), $"DateTime must be before {MaxDate}");
            }

            if (utcPlainDate < MinDate)
            {
                throw new ArgumentOutOfRangeException(nameof(plainDate), $"DateTime must be after {MinDate}");
            }

            // convert date to number of days from our epoch (1/1/0001) aka DateTime.MinValue
            var daysFromEpoch = (utcPlainDate - DateTime.MinValue.Date).Days;

            // pad input to 6 characters w/ leading zeroes
            var plainText = daysFromEpoch.ToString(CultureInfo.InvariantCulture);
            plainText = plainText.PadLeft(6, '0');

            // encrypted output will contain base12 characters (0-9a-b)
            var cipherText = await EncryptPipelineAsync(dataset, _ffxCache, plainText, tweak);

            // convert base12 string to base10 int
            var encryptedDaysFromEpoch = IntegerHelper.Parse(cipherText, 12);

            // convert encrypted days back to a date time
            var encryptedDate = DateTime.MinValue.Date.AddDays(encryptedDaysFromEpoch);

            return encryptedDate;
        }
    }
}
