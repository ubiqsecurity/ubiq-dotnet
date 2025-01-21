using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using UbiqSecurity.Config;

namespace UbiqSecurity.Idp
{
    internal class EntraIdOAuthWebServices : IOAuthWebService
    {
        private Lazy<HttpClient> _lazyHttpClient;
        private readonly IUbiqCredentials _ubiqCredentials;

        internal EntraIdOAuthWebServices(IUbiqCredentials ubiqCredentials)
        {
            // thread-safe lazy init: 'BuildHttpClient' doesn't get called
            // until _lazyHttpClient.Value is accessed
            _lazyHttpClient = new Lazy<HttpClient>(BuildHttpClient);
            _ubiqCredentials = ubiqCredentials;
        }


        public void Dispose()
        {
            if (_lazyHttpClient != null && _lazyHttpClient.IsValueCreated)
            {
                _lazyHttpClient.Value.Dispose();
                _lazyHttpClient = null;
            }
        }

        public async Task<OAuthLoginResponse> LoginAsync(IdpConfig idpConfig)
        {
            var url = $"{idpConfig.IdpTokenEndpointUrl}";

            // convert binary key bytes to Base64
            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", idpConfig.IdpTenantId),
                new KeyValuePair<string, string>("client_secret", idpConfig.IdpClientSecret),
                new KeyValuePair<string, string>("username", _ubiqCredentials.IdpUsername),
                new KeyValuePair<string, string>("password", _ubiqCredentials.IdpPassword),
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("scope", $"api://{idpConfig.IdpTenantId}/.default"),
            });

            var response = await _lazyHttpClient.Value.PostAsync(url, formContent).ConfigureAwait(false);

            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            var jsonResponse = JsonConvert.DeserializeObject<OAuthLoginResponse>(body);

            return jsonResponse;
        }

        private static HttpClient BuildHttpClient()
        {
            HttpClient httpClient = new HttpClient();

            // start clean
            httpClient.DefaultRequestHeaders.Clear();

            // set KeepAlive false to clean up socket connections ASAP
            httpClient.DefaultRequestHeaders.Add("Connection", "close");

            return httpClient;
        }
    }
}
