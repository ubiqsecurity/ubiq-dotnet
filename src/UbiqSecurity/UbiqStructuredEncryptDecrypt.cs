using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UbiqSecurity.Internals;
using UbiqSecurity.Internals.Billing;
using UbiqSecurity.Internals.Cache;
using UbiqSecurity.Internals.Structured;
using UbiqSecurity.Internals.Structured.Pipeline;
using UbiqSecurity.Internals.WebService;
using UbiqSecurity.Internals.WebService.Models;

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
            var dataset = await _datasetCache.GetAsync(datasetName);

            return await DecryptPipelineAsync(dataset, _ffxCache, cipherText, tweak);
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
            var dataset = await _datasetCache.GetAsync(datasetName);

            return await EncryptPipelineAsync(dataset, _ffxCache, plainText, tweak);
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
                var cipherText = await EncryptPipelineAsync(dataset, _ffxCache, plainText, tweak);

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

        private async Task<string> EncryptPipelineAsync(FfsRecord dataset, IFfxCache ffxCache, string plainText, byte[] tweak)
        {
            var operationContext = new OperationContext()
            {
                CurrentValue = plainText,
                Dataset = dataset,
                IsEncrypt = true,
                FfxCache = ffxCache,
                KeyNumber = null,
                OriginalValue = plainText,
                UserSuppliedTweak = tweak,
            };

            var pipeline = new EncryptionPipeline(dataset);

            var result = await pipeline.InvokeAsync(operationContext);

            // create the billing record
            await _billingEvents.AddBillingEventAsync(_ubiqCredentials.AccessKeyId, dataset.Name, string.Empty, BillingAction.Encrypt, DatasetType.Structured, operationContext.KeyNumber ?? 0, 1);

            return result;
        }

        private async Task<string> DecryptPipelineAsync(FfsRecord dataset, IFfxCache ffxCache, string cipherText, byte[] tweak)
        {
            var operationContext = new OperationContext()
            {
                CurrentValue = cipherText,
                Dataset = dataset,
                IsEncrypt = false,
                FfxCache = ffxCache,
                KeyNumber = null,
                OriginalValue = cipherText,
                UserSuppliedTweak = tweak,
            };

            var pipeline = new DecryptionPipeline(dataset);

            var result = await pipeline.InvokeAsync(operationContext);

            // create the billing record
            await _billingEvents.AddBillingEventAsync(_ubiqCredentials.AccessKeyId, dataset.Name, string.Empty, BillingAction.Decrypt, DatasetType.Structured, operationContext.KeyNumber.Value, 1);

            return result;
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
                            ffs.InputCharacters),
                        keyNumber);
                    break;
                default:
                    throw new NotSupportedException($"Unsupported FPE Algorithm: {ffs.EncryptionAlgorithm}");
            }

            return context;
        }
    }
}
