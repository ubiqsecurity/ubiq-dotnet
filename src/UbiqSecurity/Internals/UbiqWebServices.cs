using UbiqSecurity.Model;
using Newtonsoft.Json;
using System;
#if DEBUG
using System.Diagnostics;
#endif
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Globalization;

namespace UbiqSecurity.Internals
{
    internal class UbiqWebServices : IUbiqWebService, IDisposable
    {
        private const string _applicationJson = "application/json";
        private const string _restApiRoot = "api/v0";
        private const string _restApiV3Root = "api/v3";

        private readonly IUbiqCredentials _ubiqCredentials;
        private readonly UbiqConfiguration _ubiqConfiguration;

        private readonly string _baseUrl;

        private Lazy<HttpClient> _lazyHttpClient;

        internal UbiqWebServices(IUbiqCredentials ubiqCredentials, UbiqConfiguration ubiqConfiguration)
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
            _lazyHttpClient = new Lazy<HttpClient>(BuildHttpClient);
        }

        public void Dispose()
        {
            if (_lazyHttpClient != null && _lazyHttpClient.IsValueCreated)
            {
                _lazyHttpClient.Value.Dispose();
                _lazyHttpClient = null;
            }
        }

        public async Task<DecryptionKeyResponse> GetDecryptionKeyAsync(byte[] encryptedDataKey)
        {
            var url = $"{_baseUrl}/{_restApiRoot}/decryption/key";

            // convert binary key bytes to Base64
            var base64DataKey = Convert.ToBase64String(encryptedDataKey);

            var request = new DecryptionKeyRequest(base64DataKey);

            var jsonResponse = await BuildAndHttpSendAsync(url, request, HttpMethod.Post, HttpStatusCode.OK)
                                .ConfigureAwait(false);

            // deserialize JSON response to POCO
            var decryptionKeyResponse = JsonConvert.DeserializeObject<DecryptionKeyResponse>(jsonResponse);

            // unwrap data key
            decryptionKeyResponse.UnwrappedDataKey = PayloadEncryption.UnwrapDataKey(decryptionKeyResponse.EncryptedPrivateKey, decryptionKeyResponse.WrappedDataKey, _ubiqCredentials.SecretCryptoAccessKey);

            return decryptionKeyResponse;
        }

        public async Task<EncryptionKeyResponse> GetEncryptionKeyAsync(int uses)
        {
            var url = $"{_baseUrl}/{_restApiRoot}/encryption/key";

            var request = new EncryptionKeyRequest(uses);

            
            var jsonResponse = await BuildAndHttpSendAsync(url, request, HttpMethod.Post, HttpStatusCode.Created)
                                .ConfigureAwait(false);

            // deserialize JSON response to POCO
            var encryptionKeyResponse = JsonConvert.DeserializeObject<EncryptionKeyResponse>(jsonResponse);

            // unwrap data key
            encryptionKeyResponse.UnwrappedDataKey = PayloadEncryption.UnwrapDataKey(encryptionKeyResponse.EncryptedPrivateKey, encryptionKeyResponse.WrappedDataKey, _ubiqCredentials.SecretCryptoAccessKey);

            return encryptionKeyResponse;
        }

        public async Task<FfsRecord> GetFfsDefinitionAsync(string ffsName)
        {
            var key = UrlHelper.GenerateFfsUrl(ffsName, _ubiqCredentials);
            var url = $"{_baseUrl}/{_restApiRoot}/ffs?{key}";

            
            var jsonResponse = await BuildAndHttpSendAsync(url, HttpMethod.Get, HttpStatusCode.OK).ConfigureAwait(false);

            // deserialize JSON response to POCO
            var ffsRecord = JsonConvert.DeserializeObject<FfsRecord>(jsonResponse);
            return ffsRecord;
        }

        public async Task<FpeKeyResponse> GetFpeDecryptionKeyAsync(string ffsName, int keyNumber)
        {
            var key = UrlHelper.GenerateFpeUrlDecrypt(ffsName, keyNumber, _ubiqCredentials);
            var url = $"{_baseUrl}/{_restApiRoot}/fpe/key?{key}";

            var jsonResponse = await BuildAndHttpSendAsync(url, HttpMethod.Get, HttpStatusCode.OK).ConfigureAwait(false);

            // deserialize JSON response to POCO
            var encryptionKeyResponse = JsonConvert.DeserializeObject<FpeKeyResponse>(jsonResponse);

            return encryptionKeyResponse;
        }

        public async Task<DatasetAndKeysResponse> GetDatasetAndKeysAsync(string datasetName)
        {
            var key = UrlHelper.GenerateFpeUrlEncrypt(datasetName, _ubiqCredentials);
            var url = $"{_baseUrl}/{_restApiRoot}/fpe/def_keys?{key}";

            var jsonResponse = await BuildAndHttpSendAsync(url, HttpMethod.Get, HttpStatusCode.OK).ConfigureAwait(false);

            // deserialize JSON response to POCO
            var encryptionKeyResponse = JsonConvert.DeserializeObject<DatasetAndKeysResponse>(jsonResponse);

            return encryptionKeyResponse;
        }

        public async Task<FpeKeyResponse> GetFpeEncryptionKeyAsync(string ffsName)
        {
            var key = UrlHelper.GenerateFpeUrlEncrypt(ffsName, _ubiqCredentials);
            var url = $"{_baseUrl}/{_restApiRoot}/fpe/key?{key}";

            var jsonResponse = await BuildAndHttpSendAsync(url, HttpMethod.Get, HttpStatusCode.OK).ConfigureAwait(false);

            // deserialize JSON response to POCO
            var encryptionKeyResponse = JsonConvert.DeserializeObject<FpeKeyResponse>(jsonResponse);

            return encryptionKeyResponse;
        }

        public async Task<FpeBillingResponse> SendTrackingEventsAsync(TrackingEventsRequest trackingEventsRequest)
        {
            var url = $"{_baseUrl}/{_restApiV3Root}/tracking/events";

           
            var jsonResponse = await BuildAndHttpSendAsync(url, trackingEventsRequest, HttpMethod.Post, HttpStatusCode.OK)
                                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(jsonResponse))
            {
                return new FpeBillingResponse(200, string.Empty);
            }

            var response = JsonConvert.DeserializeObject<FpeBillingResponse>(jsonResponse);
            return response;
        }

        #region Private Methods

        private static HttpClient BuildHttpClient()
        {
            HttpClient httpClient = new HttpClient();

            // start clean
            httpClient.DefaultRequestHeaders.Clear();

            var assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
            httpClient.DefaultRequestHeaders.Add("User-Agent", $"ubiq-dotnet/{assemblyVersion}");

            // set KeepAlive false to clean up socket connections ASAP
            httpClient.DefaultRequestHeaders.Add("Connection", "close");

            // assume all responses are JSON messages
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_applicationJson));

            return httpClient;
        }

        private async Task<HttpRequestMessage> BuildSignedHttpRequest<T>(HttpMethod httpMethod, string requestUrl, T request, string publicAccessKey, string secretSigningKey)
        {
            var idpEnabled = _ubiqCredentials.IsIdp && _ubiqConfiguration.Idp != null;
            if (idpEnabled)
            {
                // ensure IDP has been initialized and refresh the token+cert if expired
                await _ubiqCredentials.CheckInitAndExpirationAsync(_ubiqConfiguration);

                // for any web request that supports a payload cert (e.g. get enc/dec keys), include it
                if (request is IPayloadEncryptionRequest payloadRequest)
                {
                    payloadRequest.PayloadCertificate = _ubiqCredentials.IdpPayloadCert;
                }
            }

            var jsonRequest = request == null ? null : JsonConvert.SerializeObject(request);

            if (httpMethod.Method != HttpMethod.Get.Method && string.IsNullOrEmpty(jsonRequest))
            {
                throw new ArgumentException("Invalid (null or empty) jsonRequest", nameof(request));
            }

            if (string.IsNullOrEmpty(jsonRequest))
            {
                jsonRequest = string.Empty;
            }

            var requestUri = new Uri(requestUrl);
            var content = new StringContent(jsonRequest, Encoding.UTF8, _applicationJson);

            var digestValue = BuildDigestValue(content, out int contentLength);

            // tricky: StringContent does not set Content-Length, so do that explicitly
            content.Headers.ContentLength = contentLength;

            var signedHttpRequest = new HttpRequestMessage(httpMethod, requestUri)
            {
                Headers =
                {
                    { HttpRequestHeader.Date.ToString(), DateTime.UtcNow.ToString("r") },      // ddd, dd MMM yyyy HH:mm:ss GMT
                    { HttpRequestHeader.Host.ToString(), requestUri.Host },
                    { "Digest", digestValue },
                },
            };

            if (httpMethod != HttpMethod.Get)
            {
                signedHttpRequest.Content = content;
            }

            var unixTimeString = UnixTimeAsString();
            var requestTarget = BuildRequestTarget(httpMethod.ToString(), requestUri);
            var signature = BuildSignature(signedHttpRequest,
                unixTimeString, requestTarget,
                publicAccessKey, secretSigningKey);

            signedHttpRequest.Headers.Add("Signature", signature);

            return signedHttpRequest;
        }

        private async Task<string> BuildAndHttpSendAsync<T>(string url, T request, HttpMethod method, HttpStatusCode successCode)
        {
            var signedHttpRequest = await BuildSignedHttpRequest(method, url, request, _ubiqCredentials.AccessKeyId, _ubiqCredentials.SecretSigningKey).ConfigureAwait(false);

            var jsonResponse = await HttpSendAsync(signedHttpRequest, successCode).ConfigureAwait(false);

            return jsonResponse;
        }

        private async Task<string> BuildAndHttpSendAsync(string url, HttpMethod method, HttpStatusCode successCode)
        {
            var signedHttpRequest = await BuildSignedHttpRequest(method, url, (object)null, _ubiqCredentials.AccessKeyId, _ubiqCredentials.SecretSigningKey).ConfigureAwait(false);

            var jsonResponse = await HttpSendAsync(signedHttpRequest, successCode).ConfigureAwait(false);

            return jsonResponse;
        }

        private async Task<string> HttpSendAsync(HttpRequestMessage httpRequestMessage, HttpStatusCode successCode)
        {
            if (_lazyHttpClient == null)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            try
            {
                // JIT-create and cache reusable HttpClient instance
                using (var httpResponse = await _lazyHttpClient.Value.SendAsync(httpRequestMessage).ConfigureAwait(false))
                {
                    var responseString = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (httpResponse.StatusCode != successCode)
                    {
                        var errorMessage = JsonConvert.SerializeObject(
                            new
                            {
                                status = httpResponse.StatusCode,
                                message = responseString ?? ""
                            }
                        );

                        throw new InvalidOperationException(errorMessage);
                    }

					return responseString;
				}
			}
			finally
			{
				httpRequestMessage?.Dispose();
			}
		}

        private static string UnixTimeAsString()
        {
            var utcNow = DateTime.UtcNow;
            long unixTime = ((DateTimeOffset)utcNow).ToUnixTimeSeconds();

            return unixTime.ToString(CultureInfo.InvariantCulture);
        }

        // build (request-target) HTTP header field of the form:
        //  method path?query
        private static string BuildRequestTarget(string httpMethod, Uri requestUri)
        {
            // assemble the request-target field value
            return $"{httpMethod.ToLowerInvariant()} {requestUri.PathAndQuery}";
        }

        private static string BuildDigestValue(HttpContent httpContent, out int contentLength)
        {
            contentLength = 0;

            using (var contentStream = new MemoryStream())
            { 

                httpContent.CopyToAsync(contentStream).Wait();
                contentStream.Position = 0;     // rewind

                // return content length, in bytes
                contentLength = (int)contentStream.Length;

                // calculate SHA-512 hash
                byte[] hashBytes = null;
                using (var sha512 = SHA512.Create())
                {
                    hashBytes = sha512.ComputeHash(contentStream);
                }

                // return SHA-512 result as Base64 string
                return $"SHA-512={Convert.ToBase64String(hashBytes)}";
            }
        }

        private static string BuildSignature(HttpRequestMessage httpRequestMessage,
            string unixTimeString,
            string requestTarget,
            string publicAccessKey,
            string secretSigningKey)
        {
            var secretSigningKeyBytes = Encoding.UTF8.GetBytes(secretSigningKey);

            using (var hashStream = new MemoryStream())
            {
                WriteHashableBytes(hashStream, "(created)", unixTimeString);
                WriteHashableBytes(hashStream, "(request-target)", requestTarget);
                if (httpRequestMessage.Content != null)
                {
                    WriteHashableBytes(hashStream, "Content-Length", httpRequestMessage.Content.Headers.GetValues("Content-Length").First());
                    WriteHashableBytes(hashStream, "Content-Type", httpRequestMessage.Content.Headers.GetValues("Content-Type").First());
                }
                WriteHashableBytes(hashStream, "Date", httpRequestMessage.Headers.GetValues("Date").First());
                WriteHashableBytes(hashStream, "Digest", httpRequestMessage.Headers.GetValues("Digest").First());
                WriteHashableBytes(hashStream, "Host", httpRequestMessage.Headers.GetValues("Host").First());

                hashStream.Position = 0;        // rewind

                using (var hmac = new HMACSHA512(secretSigningKeyBytes))
                {
                    // Compute the hash of the input data
                    var hashBytes = hmac.ComputeHash(hashStream);

                    // assemble final signature string
                    var signature = new StringBuilder();

                    signature.AppendFormat(CultureInfo.InvariantCulture, "keyId=\"{0}\"", publicAccessKey);
                    signature.Append(", algorithm=\"hmac-sha512\"");
                    signature.AppendFormat(CultureInfo.InvariantCulture, ", created={0}", unixTimeString);
                    if (httpRequestMessage.Content != null)
                    {
                        signature.Append(", headers=\"(created) (request-target) content-length content-type date digest host\"");
                    }
                    else
                    {
                        signature.Append(", headers=\"(created) (request-target) date digest host\"");
                    }

                    signature.AppendFormat(CultureInfo.InvariantCulture, ", signature=\"{0}\"", Convert.ToBase64String(hashBytes));
                    return signature.ToString();
                }
            }
        }

        private static void WriteHashableBytes(Stream stream, string name, string value)
        {
            // build Unicode string
            var hashableString = $"{name.ToLowerInvariant()}: {value}\n";

            // convert to UTF-8 bytes
            var hashableBytes = Encoding.UTF8.GetBytes(hashableString);

            // write bytes to caller-provided Stream
            stream.Write(hashableBytes, 0, hashableBytes.Length);
        }

        #endregion
    }
}
