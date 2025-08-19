using System;
using System.IO;
using Newtonsoft.Json;
using UbiqSecurity.Internals;
using UbiqSecurity.Internals.Billing;
using UbiqSecurity.Internals.Cache;
using UbiqSecurity.Internals.WebService;

namespace UbiqSecurity
{
    public class CryptographyBuilder
    {
        private UbiqConfiguration _configuration;
        private IUbiqCredentials _credentials;

        public CryptographyBuilder()
        {
            _configuration = new UbiqConfiguration();
        }

        public static CryptographyBuilder Create()
        {
            return new CryptographyBuilder();
        }

        public CryptographyBuilder WithCredentials(IUbiqCredentials credentials)
        {
            _credentials = credentials;

            return this;
        }

        [Obsolete("Use WithCredentialsFromFile() instead")]
        public CryptographyBuilder WithCredentials(string pathToCredentialsFile, string profile = "default")
        {
            return WithCredentialsFromFile(pathToCredentialsFile, profile);
        }

        public CryptographyBuilder WithCredentialsFromEnvironmentVariables()
        {
            _credentials = UbiqCredentials.CreateFromEnvironmentVariables();

            return this;
        }

        public CryptographyBuilder WithCredentialsFromDefaultFileLocation(string profile = "default")
        {
            return WithCredentialsFromFile(null, profile);
        }

        public CryptographyBuilder WithCredentialsFromFile(string pathToCredentialsFile, string profile = "default")
        {
            _credentials = UbiqCredentials.CreateFromFile(pathToCredentialsFile, profile);

            return this;
        }

        public CryptographyBuilder WithCredentials(Action<IUbiqCredentials> credentialsAction)
        {
            _credentials = _credentials ?? new UbiqCredentials();

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

        [Obsolete("Use WithConfigFromFile(string jsonPath) instead")]
        public CryptographyBuilder WithConfig(string jsonPath)
        {
            return WithConfigFromFile(jsonPath);
        }

        public CryptographyBuilder WithConfigFromDefaultFileLocation()
        {
            var filePath = UbiqCredentials.GetDefaultFileLocation("config.json");

            return WithConfigFromFile(filePath);
        }

        public CryptographyBuilder WithConfigFromFile(string jsonPath)
        {
            var json = File.ReadAllText(jsonPath);

            var configuration = JsonConvert.DeserializeObject<UbiqConfiguration>(json);

            _configuration = configuration;

            return this;
        }

        public UbiqStructuredEncryptDecrypt BuildStructured()
        {
            _credentials = _credentials ?? UbiqCredentials.CreateFromFile(string.Empty);
            _credentials.Validate();

            var webService = new UbiqWebService(_credentials, _configuration);
            var billingEventsManager = new BillingEventsManager(_configuration, webService);
            var ffxCache = new FfxCache(webService, _configuration, _credentials);
            var datasetCache = new DatasetCache(webService, _configuration);

            AsyncHelper.RunSync(() => _credentials.CheckInitAndExpirationAsync(_configuration));

            return new UbiqStructuredEncryptDecrypt(_credentials, webService, billingEventsManager, ffxCache, datasetCache);
        }

        public UbiqEncrypt BuildUnstructuredEncrypt()
        {
            _credentials = _credentials ?? UbiqCredentials.CreateFromFile(string.Empty);
            _credentials.Validate();

            var webService = new UbiqWebService(_credentials, _configuration);
            var billingEventsManager = new BillingEventsManager(_configuration, webService);

            return new UbiqEncrypt(_credentials, 1, webService, billingEventsManager);
        }

        public UbiqDecrypt BuildUnstructuredDecrypt()
        {
            _credentials = _credentials ?? UbiqCredentials.CreateFromFile(string.Empty);
            _credentials.Validate();

            var webService = new UbiqWebService(_credentials, _configuration);
            var billingEventsManager = new BillingEventsManager(_configuration, webService);
            var unstructuredCache = new UnstructuredKeyCache(webService, _configuration, _credentials);

            return new UbiqDecrypt(_credentials, webService, billingEventsManager, unstructuredCache);
        }
    }
}
