using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UbiqSecurity.Model;

namespace UbiqSecurity.Internals.WebService
{
    internal class UbiqWebService : IUbiqWebService, IDisposable
    {
        private const string JsonMimeType = "application/json";
        private const string ApiV0Root = "api/v0";
        private const string ApiV3Root = "api/v3";

        private readonly IUbiqCredentials _ubiqCredentials;
        private readonly UbiqConfiguration _ubiqConfiguration;

        private readonly string _baseUrl;

        private HttpClient _httpClient;

        internal UbiqWebService(IUbiqCredentials ubiqCredentials, UbiqConfiguration ubiqConfiguration)
        {
            _ubiqCredentials = ubiqCredentials;
            _ubiqConfiguration = ubiqConfiguration;

            if (!_ubiqCredentials.Host.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                _baseUrl = $"https://{_ubiqCredentials.Host}";
            }
            else
            {
                _baseUrl = _ubiqCredentials.Host;
            }

            // thread-safe lazy init: 'BuildHttpClient' doesn't get called
            // until _lazyHttpClient.Value is accessed
            _httpClient = BuildHttpClient(ubiqCredentials);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
            _httpClient = null;
        }

        public async Task<DecryptionKeyResponse> GetDecryptionKeyAsync(byte[] encryptedDataKey)
        {
            var url = $"{_baseUrl}/{ApiV0Root}/decryption/key";

            // convert binary key bytes to Base64
            var base64DataKey = Convert.ToBase64String(encryptedDataKey);

            var request = new DecryptionKeyRequest(base64DataKey);

            var decryptionKeyResponse = await HttpSendAsync<DecryptionKeyResponse, DecryptionKeyRequest>(url, HttpMethod.Post, request).ConfigureAwait(false);

            // unwrap data key
            decryptionKeyResponse.UnwrappedDataKey = PayloadEncryption.UnwrapDataKey(decryptionKeyResponse.EncryptedPrivateKey, decryptionKeyResponse.WrappedDataKey, _ubiqCredentials.SecretCryptoAccessKey);

            return decryptionKeyResponse;
        }

        public async Task<EncryptionKeyResponse> GetEncryptionKeyAsync(int uses)
        {
            var url = $"{_baseUrl}/{ApiV0Root}/encryption/key";

            var request = new EncryptionKeyRequest(uses);

            var encryptionKeyResponse = await HttpSendAsync<EncryptionKeyResponse, EncryptionKeyRequest>(url, HttpMethod.Post, request);

            // unwrap data key
            encryptionKeyResponse.UnwrappedDataKey = PayloadEncryption.UnwrapDataKey(encryptionKeyResponse.EncryptedPrivateKey, encryptionKeyResponse.WrappedDataKey, _ubiqCredentials.SecretCryptoAccessKey);

            return encryptionKeyResponse;
        }

        public async Task<FfsRecord> GetDatasetAsync(string datasetName)
        {
            var query = new NameValueCollection
            {
                { "papi", _ubiqCredentials.AccessKeyId },
                { "ffs_name", datasetName },
            };

            var url = $"{_baseUrl}/{ApiV0Root}/ffs?{query.ToQueryString()}";

            var ffsRecord = await HttpSendAsync<FfsRecord>(url, HttpMethod.Get).ConfigureAwait(false);

            return ffsRecord;
        }

        public async Task<FpeKeyResponse> GetFpeDecryptionKeyAsync(string datasetName, int keyNumber)
        {
            var query = new NameValueCollection
            {
                { "papi", _ubiqCredentials.AccessKeyId },
                { "ffs_name", datasetName },
                { "key_number", keyNumber.ToString(CultureInfo.InvariantCulture) },
            };

            var idpEnabled = _ubiqCredentials.IsIdp && _ubiqConfiguration.Idp != null;
            if (idpEnabled)
            {
                query.Add("payload_cert", Convert.ToBase64String(Encoding.UTF8.GetBytes(_ubiqCredentials.IdpPayloadCert)));
            }

            var url = $"{_baseUrl}/{ApiV0Root}/fpe/key?{query.ToQueryString()}";

            var encryptionKeyResponse = await HttpSendAsync<FpeKeyResponse>(url, HttpMethod.Get).ConfigureAwait(false);

            if (idpEnabled)
            {
                encryptionKeyResponse.EncryptedPrivateKey = ((UbiqCredentials)_ubiqCredentials).IdpPayloadCertInfo.EncryptedPrivateKey;
            }

            return encryptionKeyResponse;
        }

        public async Task<DatasetAndKeysResponse> GetDatasetAndKeysAsync(string datasetName)
        {
            var query = new NameValueCollection
            {
                { "papi", _ubiqCredentials.AccessKeyId },
                { "ffs_name", datasetName },
            };

            var idpEnabled = _ubiqCredentials.IsIdp && _ubiqConfiguration.Idp != null;
            if (idpEnabled)
            {
                query.Add("payload_cert", Convert.ToBase64String(Encoding.UTF8.GetBytes(_ubiqCredentials.IdpPayloadCert)));
            }

            var url = $"{_baseUrl}/{ApiV0Root}/fpe/def_keys?{query.ToQueryString()}";

            var encryptionKeyResponse = await HttpSendAsync<DatasetAndKeysResponse>(url, HttpMethod.Get).ConfigureAwait(false);

            if (idpEnabled)
            {
                foreach (var key in encryptionKeyResponse)
                {
                    key.Value.EncryptedPrivateKey = ((UbiqCredentials)_ubiqCredentials).IdpPayloadCertInfo.EncryptedPrivateKey;
                }
            }

            return encryptionKeyResponse;
        }

        public async Task<FpeKeyResponse> GetFpeEncryptionKeyAsync(string datasetName)
        {
            var query = new NameValueCollection
            {
                { "papi", _ubiqCredentials.AccessKeyId },
                { "ffs_name", datasetName },
            };

            var idpEnabled = _ubiqCredentials.IsIdp && _ubiqConfiguration.Idp != null;
            if (idpEnabled)
            {
                query.Add("payload_cert", Convert.ToBase64String(Encoding.UTF8.GetBytes(_ubiqCredentials.IdpPayloadCert)));
            }

            var url = $"{_baseUrl}/{ApiV0Root}/fpe/key?{query.ToQueryString()}";

            var encryptionKeyResponse = await HttpSendAsync<FpeKeyResponse>(url, HttpMethod.Get).ConfigureAwait(false);

            if (idpEnabled)
            { 
                encryptionKeyResponse.EncryptedPrivateKey = ((UbiqCredentials)_ubiqCredentials).IdpPayloadCertInfo.EncryptedPrivateKey;
            }

            return encryptionKeyResponse;
        }

        public async Task<FpeBillingResponse> SendTrackingEventsAsync(TrackingEventsRequest trackingEventsRequest)
        {
            var url = $"{_baseUrl}/{ApiV3Root}/tracking/events";

            var response = await HttpSendAsync<FpeBillingResponse, TrackingEventsRequest>(url, HttpMethod.Post, trackingEventsRequest).ConfigureAwait(false);

            return response ?? new FpeBillingResponse(200, string.Empty);
        }

        protected async Task<TResponse> HttpSendAsync<TResponse>(string url, HttpMethod method)
        {
            return await HttpSendAsync<TResponse, object>(url, method, null).ConfigureAwait(false);
        }

        protected async Task<TResponse> HttpSendAsync<TResponse, TRequest>(string url, HttpMethod method, TRequest request)
        {
            var idpEnabled = _ubiqCredentials.IsIdp && _ubiqConfiguration.Idp != null;
            if (idpEnabled)
            {
                // ensure IDP has been initialized and refresh the token+cert if expired
                await _ubiqCredentials.CheckInitAndExpirationAsync(_ubiqConfiguration);

                // for any web request that supports a payload cert (e.g. get enc/dec keys), include it
                if (request != null && request is IPayloadEncryptionRequest payloadRequest)
                {
                    payloadRequest.PayloadCertificate = _ubiqCredentials.IdpPayloadCert;
                }
            }

            HttpRequestMessage requestMessage = new HttpRequestMessage(method, url);

            if (request != null)
            { 
                var jsonRequest = JsonConvert.SerializeObject(request);
                requestMessage.Content = new StringContent(jsonRequest, Encoding.UTF8, JsonMimeType);
            }

            var responseMessage = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);

            var responseString = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

            if (!responseMessage.IsSuccessStatusCode)
            {
                var errorMessage = JsonConvert.SerializeObject(
                    new
                    {
                        status = responseMessage.StatusCode,
                        message = responseString ?? string.Empty,
                    }
                );

                throw new InvalidOperationException(errorMessage);
            }

            TResponse response = JsonConvert.DeserializeObject<TResponse>(responseString);

            return response;
        }

        private static HttpClient BuildHttpClient(IUbiqCredentials credentials)
        {
            HttpClient httpClient = new HttpClient(new HttpSignatureDelegatingHandler(credentials));

            // start clean
            httpClient.DefaultRequestHeaders.Clear();

            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            httpClient.DefaultRequestHeaders.Add("User-Agent", $"ubiq-dotnet/{assemblyVersion}");

            // set KeepAlive false to clean up socket connections ASAP
            httpClient.DefaultRequestHeaders.Add("Connection", "close");

            // assume all responses are JSON messages
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(JsonMimeType));

            return httpClient;
        }
    }
}
