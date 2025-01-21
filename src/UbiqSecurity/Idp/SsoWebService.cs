using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UbiqSecurity.Idp
{
    internal class SsoWebService : ISsoWebService, IDisposable
    {
        private const string ApplicationJson = "application/json";
        private const string RestApiV3Root = "api/v3";

        private readonly string _baseUrl;

        private Lazy<HttpClient> _lazyHttpClient;

        internal SsoWebService(IUbiqCredentials ubiqCredentials, UbiqConfiguration ubiqConfiguration)
        {
            Credentials = ubiqCredentials;
            Configuration = ubiqConfiguration;

            if (!ubiqCredentials.Host.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                _baseUrl = $"https://{ubiqCredentials.Host}";
            }
            else
            {
                _baseUrl = ubiqCredentials.Host;
            }

            // thread-safe lazy init: 'BuildHttpClient' doesn't get called
            // until _lazyHttpClient.Value is accessed
            _lazyHttpClient = new Lazy<HttpClient>(BuildHttpClient);
        }

        public IUbiqCredentials Credentials { get; private set; }

        public UbiqConfiguration Configuration { get; private set; }

        public void Dispose()
        {
            if (_lazyHttpClient != null && _lazyHttpClient.IsValueCreated)
            {
                _lazyHttpClient.Value.Dispose();
                _lazyHttpClient = null;
            }
        }

        public async Task<SsoResponse> SsoAsync(SsoRequest request)
        {
            var url = $"{_baseUrl}/{Configuration.Idp.CustomerId}/{RestApiV3Root}/scim/sso";

            _lazyHttpClient.Value.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", request.AccessToken);

            var json = JsonConvert.SerializeObject(request);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _lazyHttpClient.Value.PostAsync(url, content).ConfigureAwait(false);

            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var jsonResponse = JsonConvert.DeserializeObject<SsoResponse>(body);

            return jsonResponse;
        }

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
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(ApplicationJson));
            return httpClient;
        }
    }
}
