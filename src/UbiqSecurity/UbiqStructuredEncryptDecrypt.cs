using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UbiqSecurity.Billing;
using UbiqSecurity.Cache;
using UbiqSecurity.Fpe;
using UbiqSecurity.Internals;
using UbiqSecurity.Internals.WebService;
using UbiqSecurity.Model;

namespace UbiqSecurity
{
    public class UbiqStructuredEncryptDecrypt : IDisposable
    {
        private readonly IUbiqCredentials _ubiqCredentials;

        private readonly IBillingEventsManager _billingEvents;

        private IUbiqWebService _ubiqWebService; // null when disposed
        private IFfxCache _ffxCache;
        private IDatasetCache _datasetCache;

        [Obsolete("Use CryptoBuilder .BuildStructured()")]
        public UbiqStructuredEncryptDecrypt(IUbiqCredentials ubiqCredentials)
            : this(ubiqCredentials, new UbiqConfiguration())
        {
        }

        [Obsolete("Use CryptoBuilder .BuildStructured()")]
        public UbiqStructuredEncryptDecrypt(IUbiqCredentials ubiqCredentials, UbiqConfiguration ubiqConfiguration)
        {
            _ubiqCredentials = ubiqCredentials;

            _ubiqWebService = new UbiqWebService(ubiqCredentials, ubiqConfiguration);
            _billingEvents = new BillingEventsManager(ubiqConfiguration, _ubiqWebService);
            _ffxCache = new FfxCache(_ubiqWebService, ubiqConfiguration, ubiqCredentials);
            _datasetCache = new DatasetCache(_ubiqWebService, ubiqConfiguration);
        }

        internal UbiqStructuredEncryptDecrypt(IUbiqCredentials ubiqCredentials, IUbiqWebService webService, IBillingEventsManager billingEventsManager, IFfxCache ffxCache, IDatasetCache datasetCache)
        {
            _ubiqCredentials = ubiqCredentials;
            _billingEvents = billingEventsManager;
            _ubiqWebService = webService;
            _ffxCache = ffxCache;
            _datasetCache = datasetCache;
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void ClearCache()
        {
            _ffxCache?.Clear();
            _datasetCache?.Clear();
        }

        public void AddReportingUserDefinedMetadata(string jsonString)
        {
            _billingEvents.AddUserDefinedMetadata(jsonString);
        }

        public async Task<byte[]> DecryptAsync(string datasetName, byte[] cipherBytes)
        {
            return await DecryptAsync(datasetName, cipherBytes, null);
        }

        public async Task<byte[]> DecryptAsync(string datasetName, byte[] cipherBytes, byte[] tweak)
        {
            var input = Encoding.ASCII.GetString(cipherBytes);

            var result = await DecryptAsync(datasetName, input, tweak);

            return Encoding.ASCII.GetBytes(result);
        }

        public async Task<string> DecryptAsync(string datasetName, string cipherText)
        {
            return await DecryptAsync(datasetName, cipherText, null);
        }

        public async Task<string> DecryptAsync(string datasetName, string cipherText, byte[] tweak)
        {
            try
            {
                var dataset = await _datasetCache.GetAsync(datasetName);

                var parsedInput = FpeParser.Parse(cipherText, dataset, false);

                // P20-1357 verify input length
                if (parsedInput.Trimmed.Length < dataset.MinInputLength || parsedInput.Trimmed.Length > dataset.MaxInputLength)
                {
                    throw new ArgumentException("Input length does not match FFS parameters.");
                }

                var keyNumber = parsedInput.DecodeKeyNumber(dataset, 0);

                var ffx = await _ffxCache.GetAsync(dataset, keyNumber);

                var convertedRadixCipherText = parsedInput.Trimmed.ConvertRadix(dataset.OutputCharacters, dataset.InputCharacters);

                var plainText = ffx.Cipher(dataset.EncryptionAlgorithm, convertedRadixCipherText, tweak, false);

                var formattedPlainText = MergeToFormattedOutput(parsedInput, plainText, dataset.InputCharacters);

                // create the billing record
                await _billingEvents.AddBillingEventAsync(_ubiqCredentials.AccessKeyId, dataset.Name, string.Empty, BillingAction.Decrypt, DatasetType.Structured, keyNumber, 1);

                return formattedPlainText;
            }
            catch
            {
                throw;
            }
        }

        public async Task<byte[]> EncryptAsync(string datasetName, byte[] plainBytes)
        {
            return await EncryptAsync(datasetName, plainBytes, null);
        }

        public async Task<byte[]> EncryptAsync(string datasetName, byte[] plainBytes, byte[] tweak)
        {
            var input = Encoding.ASCII.GetString(plainBytes);

            var result = await EncryptAsync(datasetName, input, tweak);

            return Encoding.ASCII.GetBytes(result);

        }

        public async Task<string> EncryptAsync(string datasetName, string plainText)
        {
            return await EncryptAsync(datasetName, plainText, null);
        }

        public async Task<string> EncryptAsync(string datasetName, string plainText, byte[] tweak)
        {
            try
            {
                var dataset = await _datasetCache.GetAsync(datasetName);

                var ffx = await _ffxCache.GetAsync(dataset, null);

                return await EncryptAsync(dataset, ffx, plainText, tweak);
            }
            catch
            {
                throw;
            }
        }

        private async Task<string> EncryptAsync(FfsRecord dataset, FfxContext ffx, string plainText, byte[] tweak)
        {
            var parsedInput = FpeParser.Parse(plainText, dataset, true);

            // P20-1357 verify input length
            if (parsedInput.Trimmed.Length < dataset.MinInputLength || parsedInput.Trimmed.Length > dataset.MaxInputLength)
            {
                throw new ArgumentException("Input length does not match FFS parameters.");
            }

            // encrypt using desired FPE algorithm
            string cipherText = ffx.Cipher(dataset.EncryptionAlgorithm, parsedInput.Trimmed, tweak, true);

            // convert to desired output character set
            string radixConvertedCipherText = cipherText.ConvertRadix(dataset.InputCharacters, dataset.OutputCharacters);

            // encode keynumber into text
            string encodedValue = EncodeKeyNumber(dataset, ffx.KeyNumber, radixConvertedCipherText);

            // final formatting
            string formattedValue = MergeToFormattedOutput(parsedInput, encodedValue, dataset.OutputCharacters);

            // create the billing record
            await _billingEvents.AddBillingEventAsync(_ubiqCredentials.AccessKeyId, dataset.Name, string.Empty, BillingAction.Encrypt, DatasetType.Structured, ffx.KeyNumber, 1);

            return formattedValue;
        }

        public async Task<IEnumerable<string>> EncryptForSearchAsync(string datasetName, string plainText)
        {
            return await EncryptForSearchAsync(datasetName, plainText, null);
        }

        public async Task<IEnumerable<string>> EncryptForSearchAsync(string datasetName, string plainText, byte[] tweak)
        {
            await LoadAllKeysAsync(datasetName);

            var dataset = await _datasetCache.GetAsync(datasetName);
            var currentFfx = await _ffxCache.GetAsync(dataset, null);
            var results = new List<string>();

            for (int keyNumber = 0; keyNumber <= currentFfx.KeyNumber; keyNumber++)
            {
                var ffx = await _ffxCache.GetAsync(dataset, keyNumber);

                var cipherText = await EncryptAsync(dataset, ffx, plainText, tweak);

                results.Add(cipherText);
            }

            return results;
        }

        public string GetCopyOfUsage() => _billingEvents.GetSerializedEvents();

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _billingEvents?.Dispose();

                if (_ffxCache != null)
                {
                    _ffxCache.Dispose();
                    _ffxCache = null;
                }

                if (_datasetCache != null)
                {
                    _datasetCache.Dispose();
                    _datasetCache = null;
                }

                if (_ubiqWebService != null)
                {
                    _ubiqWebService.Dispose();
                    _ubiqWebService = null;
                }
            }
        }

        private async Task LoadAllKeysAsync(string datasetName)
        {
            var datasetsWithKeys = await _ubiqWebService.GetDatasetAndKeysAsync(datasetName);

            // cache datasets and ffx contexts if they don't already exist
            foreach (var key in datasetsWithKeys.Keys)
            {
                var datasetWithKeys = datasetsWithKeys[key];

                _datasetCache.TryAdd(datasetWithKeys.Dataset);

                foreach (var datasetKey in datasetWithKeys.Keys.Select((value, i) => new { Value = value, Index = i }))
                {
                    var context = GetFfsContext(datasetWithKeys.Dataset, datasetWithKeys.EncryptedPrivateKey, datasetKey.Value, datasetKey.Index);

                    var keyId = new FfsKeyId { FfsRecord = datasetWithKeys.Dataset, KeyNumber = datasetKey.Index };

                    _ffxCache.TryAdd(keyId, context);
                }
            }
        }

        private FfxContext GetFfsContext(FfsRecord ffs, string encryptedPrivateKey, string wrappedDataKey, int keyNumber)
        {
            FfxContext context = new FfxContext();

            byte[] tweak = Array.Empty<byte>();
            if (ffs.TweakSource == "constant")
            {
                tweak = Convert.FromBase64String(ffs.Tweak);
            }

            byte[] key = PayloadEncryption.UnwrapDataKey(encryptedPrivateKey, wrappedDataKey, _ubiqCredentials.SecretCryptoAccessKey);
            switch (ffs.EncryptionAlgorithm)
            {
                case "FF1":
                    context.SetFF1(
                        new FF1(
                            key,
                            tweak,
                            ffs.TweakMinLength,
                            ffs.TweakMaxLength,
                            ffs.InputCharacters.Length,
                            ffs.InputCharacters
                        ),
                        keyNumber
                    );
                    break;
                default:
                    throw new NotSupportedException($"Unsupported FPE Algorithm: {ffs.EncryptionAlgorithm}");
            }

            return context;
        }

        private static string EncodeKeyNumber(FfsRecord dataset, int keyNumber, string str)
        {
            var charBuf = str[0];

            var ctValue = dataset.OutputCharacters.IndexOf(charBuf);

            ctValue += keyNumber << (int)dataset.MsbEncodingBits.Value;

            var ch = dataset.OutputCharacters[ctValue];

            return str.ReplaceCharacter(ch, 0);
        }

        /// <summary>
        /// Merges the given string into the  "formatted_dest" pattern using the 
        /// set of provided characters. 
        /// </summary>
        /// <param name="formattedDestination">Formatted destination string</param>
        /// <param name="convertedToRadix">String to be placed in the formattedDestination</param>
        /// <param name="characterSet">Set of characters to use in the final formattedDestination</param>
        /// <returns></returns>
        private static string MergeToFormattedOutput(FpeParseModel input, string convertedToRadix, string characterSet)
        {
            string ret = input.StringTemplate;

            var d = ret.Length - 1;
            var s = convertedToRadix.Length - 1;

            while (s >= 0 && d >= 0)
            {
                // Find the first available destination character
                while (d >= 0 && ret[d] != characterSet[0])
                {
                    d--;
                }

                // Copy the encrypted text into the formatted output string
                if (d >= 0)
                {
                    ret = ret.ReplaceCharacter(convertedToRadix[s], d);
                }

                s--;
                d--;
            }

            return $"{input.Prefix}{ret}{input.Suffix}";
        }
    }
}

