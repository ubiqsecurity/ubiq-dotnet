using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UbiqSecurity.Billing;
using UbiqSecurity.Cache;
using UbiqSecurity.Internals;
using UbiqSecurity.Internals.WebService;

namespace UbiqSecurity
{
    public class CryptographyBuilder
    {
        private UbiqConfiguration _configuration;
        private IUbiqCredentials _credentials;

        public static CryptographyBuilder Create()
        {
            return new CryptographyBuilder();
        }

        public CryptographyBuilder()
        {
            _configuration = new UbiqConfiguration();
            _credentials = UbiqFactory.ReadCredentialsFromFile(string.Empty);
        }

        public CryptographyBuilder WithCredentials(IUbiqCredentials credentials)
        {
            _credentials = credentials;

            return this;
        }

        public CryptographyBuilder WithCredentials(string pathToCredentialsFile, string profile = "default")
        {
            _credentials = UbiqFactory.ReadCredentialsFromFile(pathToCredentialsFile, profile);

            return this;
        }

        public CryptographyBuilder WithCredentials(Action<IUbiqCredentials> credentialsAction)
        {
            credentialsAction(_credentials);
            return this;
        }

        public CryptographyBuilder WithConfig(UbiqConfiguration configuration)
        {
            _configuration = configuration;
            return this;
        }

        public CryptographyBuilder WithConfig(Action<UbiqConfiguration> configAction)
        {
            configAction(_configuration);
            return this;
        }

        public CryptographyBuilder WithConfig(string jsonPath)
        {
            var json = System.IO.File.ReadAllText(jsonPath);

            var configuration = JsonConvert.DeserializeObject<UbiqConfiguration>(json);

            _configuration = configuration;

            return this;
        }

        public UbiqStructuredEncryptDecrypt BuildStructured()
        {
            var webService = new UbiqWebService(_credentials, _configuration);
            var billingEventsManager = new BillingEventsManager(_configuration, webService);
            var ffxCache = new FfxCache(webService, _configuration, _credentials);
            var datasetCache = new DatasetCache(webService, _configuration);

            AsyncHelper.RunSync(() => _credentials.CheckInitAndExpirationAsync(_configuration));

            return new UbiqStructuredEncryptDecrypt(_credentials, webService, billingEventsManager, ffxCache, datasetCache);
        }

        public UbiqEncrypt BuildUnstructuredEncrypt()
        {
            var webService = new UbiqWebService(_credentials, _configuration);
            var billingEventsManager = new BillingEventsManager(_configuration, webService);

            return new UbiqEncrypt(_credentials, 1, webService, billingEventsManager);
        }

        public UbiqDecrypt BuildUnstructuredDecrypt()
        {
            var webService = new UbiqWebService(_credentials, _configuration);
            var billingEventsManager = new BillingEventsManager(_configuration, webService);
            var unstructuredCache = new UnstructuredKeyCache(webService, _configuration, _credentials);

            return new UbiqDecrypt(_credentials, webService, billingEventsManager, unstructuredCache);
        }
    }
}
