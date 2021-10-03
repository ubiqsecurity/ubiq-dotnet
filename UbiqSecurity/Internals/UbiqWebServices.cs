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
using System.Collections.Generic;

namespace UbiqSecurity.Internals
{
	internal class UbiqWebServices : IDisposable
	{
		#region Private Data

		private const string _applicationJson = "application/json";
		private const string _restApiRoot = "api/v0";

		private readonly IUbiqCredentials _ubiqCredentials;
		private readonly string _baseUrl;

		private Lazy<HttpClient> _lazyHttpClient;

		#endregion

		#region Constructors

		internal UbiqWebServices(IUbiqCredentials ubiqCredentials)
		{
			_ubiqCredentials = ubiqCredentials;

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

		#endregion

		#region IDisposable

		public void Dispose()
		{
			if (_lazyHttpClient.IsValueCreated)
			{
				_lazyHttpClient.Value.Dispose();
				_lazyHttpClient = null;
			}
		}

		#endregion

		#region Methods

		internal async Task<DecryptionKeyResponse> GetDecryptionKeyAsync(byte[] encryptedDataKey)
		{
			var url = $"{_baseUrl}/{_restApiRoot}/decryption/key";

			// convert binary key bytes to Base64
			var base64DataKey = Convert.ToBase64String(encryptedDataKey);
			var jsonRequest = $"{{\"encrypted_data_key\": \"{base64DataKey}\"}}";

			try
			{
				var jsonResponse = await BuildAndHttpSendAsync(url, jsonRequest, HttpMethod.Post, HttpStatusCode.OK)
									.ConfigureAwait(false);

				// deserialize JSON response to POCO
				var decryptionKeyResponse = JsonConvert.DeserializeObject<DecryptionKeyResponse>(jsonResponse);

				// decrypt the server-provided decryption key
				decryptionKeyResponse.PostProcess(_ubiqCredentials.SecretCryptoAccessKey);

				return decryptionKeyResponse;
			}
			catch (Exception ex)
			{
#if DEBUG
				Debug.WriteLine($"{GetType().Name}.{nameof(GetDecryptionKeyAsync)} exception: {ex.Message}");
#endif
				return null;
			}
		}

		internal async Task<EncryptionKeyResponse> GetEncryptionKeyAsync(int uses)
		{
			var url = $"{_baseUrl}/{_restApiRoot}/encryption/key";

			var jsonRequest = $"{{\"uses\": {uses}}}";

			try
			{
				var jsonResponse = await BuildAndHttpSendAsync(url, jsonRequest, HttpMethod.Post, HttpStatusCode.Created)
									.ConfigureAwait(false);

				// deserialize JSON response to POCO
				var encryptionKeyResponse = JsonConvert.DeserializeObject<EncryptionKeyResponse>(jsonResponse);

				// decrypt the server-provided encryption key
				encryptionKeyResponse.PostProcess(_ubiqCredentials.SecretCryptoAccessKey);

				return encryptionKeyResponse;
			}
			catch (Exception ex)
			{
#if DEBUG
				Debug.WriteLine($"{GetType().Name}.{nameof(GetEncryptionKeyAsync)} exception: {ex.Message}");
#endif
				return null;
			}
		}

		internal async Task<FpeEncryptionKeyResponse> GetFpeEncryptionKeyAsync(string ffsName, int? keyNumber)
		{
			var key = UrlHelper.GenerateFpeUrl(ffsName, keyNumber, _ubiqCredentials);
			var url = $"{_baseUrl}/{_restApiRoot}/fpe/key?{key}";

			try
			{
				var jsonResponse = await BuildAndHttpSendAsync(url, null, HttpMethod.Get, HttpStatusCode.OK)
									.ConfigureAwait(false);

				// deserialize JSON response to POCO
				var encryptionKeyResponse = JsonConvert.DeserializeObject<FpeEncryptionKeyResponse>(jsonResponse);

				return encryptionKeyResponse;
			}
			catch (Exception ex)
			{
#if DEBUG
				Debug.WriteLine($"{GetType().Name}.{nameof(GetFpeEncryptionKeyAsync)} exception: {ex.Message}");
#endif
				return null;
			}
		}

		internal async Task<FfsConfigurationResponse> GetFfsConfigurationsync(string ffsName)
		{
			var key = UrlHelper.GenerateFfsUrl(ffsName, _ubiqCredentials);
			var url = $"{_baseUrl}/{_restApiRoot}/ffs?{key}";

			try
			{
				var jsonResponse = await BuildAndHttpSendAsync(url, null, HttpMethod.Get, HttpStatusCode.OK)
									.ConfigureAwait(false);

				// deserialize JSON response to POCO
				var ffsConfiguration = JsonConvert.DeserializeObject<FfsConfigurationResponse>(jsonResponse);
				return ffsConfiguration;
			}
			catch (Exception ex)
			{
#if DEBUG
				Debug.WriteLine($"{GetType().Name}.{nameof(GetFfsConfigurationsync)} exception: {ex.Message}");
#endif
				return null;
			}
		}

		internal async Task UpdateEncryptionKeyUsageAsync(int actual, int requested,
			string keyFingerprint, string encryptionSession)
		{
			var url = $"{_baseUrl}/{_restApiRoot}/encryption/key/{keyFingerprint}/{encryptionSession}";

			var jsonRequest = $"{{\"requested\": {requested}, \"actual\": {actual}}}";

			try
			{
				var jsonResponse = await BuildAndHttpSendAsync(url, jsonRequest, new HttpMethod("PATCH"), HttpStatusCode.NoContent)
									.ConfigureAwait(false);

				// expect empty response
			}
			catch (Exception ex)
			{
#if DEBUG
				Debug.WriteLine($"{GetType().Name}.{nameof(UpdateEncryptionKeyUsageAsync)} exception: {ex.Message}");
#endif
			}
		}

		internal async Task UpdateDecryptionKeyUsageAsync(int uses, string keyFingerprint, string encryptionSession)
		{
			var url = $"{_baseUrl}/{_restApiRoot}/decryption/key/{keyFingerprint}/{encryptionSession}";

			var jsonRequest = $"{{\"uses\": {uses}}}";

			try
			{
				var jsonResponse = await BuildAndHttpSendAsync(url, jsonRequest, new HttpMethod("PATCH"), HttpStatusCode.NoContent)
									.ConfigureAwait(false);

				// expect empty response
			}
			catch (Exception ex)
			{
#if DEBUG
				Debug.WriteLine($"{GetType().Name}.{nameof(UpdateDecryptionKeyUsageAsync)} exception: {ex.Message}");
#endif
			}
		}

		internal async Task<FpeBillingResponse> SendBillingAsync(IList<FpeTransactionRecord> bills)
		{
			var url = $"{_baseUrl}/{_restApiRoot}/fpe/billing/{UrlHelper.GenerateAccessKeyUrl(_ubiqCredentials)}";

			try
			{
				var body = JsonConvert.SerializeObject(bills);
				var jsonResponse = await BuildAndHttpSendAsync(url, body, HttpMethod.Post, HttpStatusCode.Created)
									.ConfigureAwait(false);

				var response = JsonConvert.DeserializeObject<FpeBillingResponse>(jsonResponse);
				return response;
			}
			catch (InvalidOperationException ex)
			{
#if DEBUG
				Debug.WriteLine($"{GetType().Name}.{nameof(SendBillingAsync)} -> url: {url}");
				Debug.WriteLine($"{GetType().Name}.{nameof(SendBillingAsync)} -> exception: {ex.Message}");
#endif
				var response = JsonConvert.DeserializeObject<FpeBillingResponse>(ex.Message);
				return response;
			}
		}

		#endregion

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

		private static HttpRequestMessage BuildSignedHttpRequest(HttpMethod httpMethod, string requestUrl,
			string jsonRequest, string publicAccessKey, string secretSigningKey)
		{
			if (httpMethod.Method != HttpMethod.Get.Method && string.IsNullOrEmpty(jsonRequest))
			{
				throw new ArgumentException(nameof(jsonRequest));
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

			if(httpMethod != HttpMethod.Get)
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

		private async Task<string> BuildAndHttpSendAsync(string url, string jsonRequest, HttpMethod method, HttpStatusCode successCode)
		{
			var signedHttpRequest = BuildSignedHttpRequest(method, url, jsonRequest,
								_ubiqCredentials.AccessKeyId,
								_ubiqCredentials.SecretSigningKey);
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
						throw new InvalidOperationException(responseString);
					}

					return responseString;
				}
			}
			catch (Exception e)
			{
#if DEBUG
				Debug.WriteLine($"HttpSendAsync Exception : HTTP Request -> {httpRequestMessage.Method}: {httpRequestMessage.RequestUri}");
				Debug.WriteLine($"HttpSendAsync Exception -> message: {e.Message}");
				Debug.WriteLine($"HttpSendAsync Response -> stackTrace: {e.StackTrace}");
#endif
			}
			finally
			{
				httpRequestMessage?.Dispose();
			}

			return null;
		}

		private static string UnixTimeAsString()
		{
			var utcNow = DateTime.UtcNow;
			long unixTime = ((DateTimeOffset)utcNow).ToUnixTimeSeconds();
			return unixTime.ToString();
		}

		// build (request-target) HTTP header field of the form:
		//  method path?query
		private static string BuildRequestTarget(string httpMethod, Uri requestUri)
		{
			// assemble the request-target field value
			return $"{httpMethod.ToLower()} {requestUri.PathAndQuery}";
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
				using (var sha512 = new SHA512Managed())
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

					signature.AppendFormat("keyId=\"{0}\"", publicAccessKey);
					signature.Append(", algorithm=\"hmac-sha512\"");
					signature.AppendFormat(", created={0}", unixTimeString);
					if (httpRequestMessage.Content != null)
					{
						signature.Append(", headers=\"(created) (request-target) content-length content-type date digest host\"");
					}
					else
					{
						signature.Append(", headers=\"(created) (request-target) date digest host\"");
					}

					signature.AppendFormat(", signature=\"{0}\"", Convert.ToBase64String(hashBytes));
					return signature.ToString();
				}
			}
		}

		private static void WriteHashableBytes(Stream stream, string name, string value)
		{
			// build Unicode string
			var hashableString = $"{name.ToLower()}: {value}\n";

			// convert to UTF-8 bytes
			var hashableBytes = Encoding.UTF8.GetBytes(hashableString);

			// write bytes to caller-provided Stream
			stream.Write(hashableBytes, 0, hashableBytes.Length);
		}

		#endregion
	}
}
