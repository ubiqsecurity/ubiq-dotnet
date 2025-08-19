using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.X509;
using UbiqSecurity.Internals.Idp;
using static UbiqSecurity.Internals.PayloadEncryption;

namespace UbiqSecurity.Internals
{
    internal class UbiqCredentials : IUbiqCredentials
    {
        public const string DefaultHost = "api.ubiqsecurity.com";
        public const string DefaultProfile = "default";

        // Environment variable names
        public const string AccessKeyIdEnvironmentVariable = "UBIQ_ACCESS_KEY_ID";
        public const string SigningKeyEnvironmentVariable = "UBIQ_SECRET_SIGNING_KEY";
        public const string CryptoEnvironmentVariable = "UBIQ_SECRET_CRYPTO_ACCESS_KEY";
        public const string HostEnvironmentVariable = "UBIQ_SERVER";

        private bool _initialized;
        private PayloadCertInfo _payloadCertInfo;
        private OAuthLoginResponse _oauthLoginResponse;

        internal UbiqCredentials()
        {
        }

        [Obsolete("Use CryptographyBuilder instead")]
        internal UbiqCredentials(string accessKeyId, string secretSigningKey, string secretCryptoAccessKey, string host)
        {
            if (string.IsNullOrEmpty(accessKeyId))
            {
                accessKeyId = Environment.GetEnvironmentVariable(AccessKeyIdEnvironmentVariable);
            }

            AccessKeyId = accessKeyId;

            if (string.IsNullOrEmpty(secretSigningKey))
            {
                secretSigningKey = Environment.GetEnvironmentVariable(SigningKeyEnvironmentVariable);
            }

            SecretSigningKey = secretSigningKey;

            if (string.IsNullOrEmpty(secretCryptoAccessKey))
            {
                secretCryptoAccessKey = Environment.GetEnvironmentVariable(CryptoEnvironmentVariable);
            }

            SecretCryptoAccessKey = secretCryptoAccessKey;

            Host = host;
            Validate();
        }

        [Obsolete("Use CryptographyBuilder instead")]
        internal UbiqCredentials(string pathname, string profile, string host)
        {
            LoadFromFile(pathname, profile);

            Host = Host ?? host;

            Validate();
        }

        public string Host { get; set; }

        public string AccessKeyId { get; set; }

        public string SecretSigningKey { get; set; }

        public string SecretCryptoAccessKey { get; set; }

        public string IdpUsername { get; set; }

        public string IdpPassword { get; set; }

        public bool IsIdp => !string.IsNullOrEmpty(IdpUsername);

        public string IdpPayloadCert => _payloadCertInfo.ApiCert;

        internal PayloadCertInfo IdpPayloadCertInfo => _payloadCertInfo;

        public async Task CheckInitAndExpirationAsync(UbiqConfiguration ubiqConfiguration)
        {
            if (!IsIdp)
            {
                return;
            }

            if (!_initialized)
            {
                await InitAsync(ubiqConfiguration);
                return;
            }

            if (_payloadCertInfo.ApiCertExpiration <= DateTime.Now)
            {
                await DoSsoAsync(ubiqConfiguration);
            }
        }

        public void Validate()
        {
            if (string.IsNullOrEmpty(Host))
            {
                throw new InvalidOperationException($"Credentials data is incomplete (Host)");
            }

            if (IsIdp)
            {
                return;
            }

            if (string.IsNullOrEmpty(AccessKeyId) || string.IsNullOrEmpty(SecretSigningKey) || string.IsNullOrEmpty(SecretCryptoAccessKey))
            {
                throw new InvalidOperationException($"Credentials data is incomplete");
            }
        }

        internal static string GetDefaultFileLocation(string file = "credentials")
        {
            // credentials file not specified, so look for ~/.ubiq/credentials
            string homeDirectory = (Environment.OSVersion.Platform == PlatformID.Unix ||
                    Environment.OSVersion.Platform == PlatformID.MacOSX)
                ? Environment.GetEnvironmentVariable("HOME")
                : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

            if (!Directory.Exists(homeDirectory))
            {
                return null;
            }

            var pathname = Path.Combine(homeDirectory, ".ubiq", file);

            return pathname;
        }

        internal static UbiqCredentials CreateFromFile(string path, string profile = DefaultProfile, string host = DefaultHost)
        {
            var credentials = new UbiqCredentials();
            credentials.LoadFromFile(path, profile, host);
            return credentials;
        }

        internal static UbiqCredentials CreateFromEnvironmentVariables()
        {
            return new UbiqCredentials()
            {
                AccessKeyId = Environment.GetEnvironmentVariable(AccessKeyIdEnvironmentVariable),
                SecretSigningKey = Environment.GetEnvironmentVariable(SigningKeyEnvironmentVariable),
                SecretCryptoAccessKey = Environment.GetEnvironmentVariable(CryptoEnvironmentVariable),
                Host = Environment.GetEnvironmentVariable(HostEnvironmentVariable),
            };
        }

        internal async Task InitAsync(UbiqConfiguration ubiqConfiguration)
        {
            if (!IsIdp || _initialized)
            {
                _initialized = true;
                return;
            }

            if (ubiqConfiguration.Idp == null)
            {
                throw new ArgumentException("ubiqConfiguration.Idp is null", nameof(ubiqConfiguration));
            }

            // generate random crypto access key
            SecretCryptoAccessKey = PayloadEncryption.GenerateRandomCryptoAccessKey();

            // generate csr + encrypted private key
            _payloadCertInfo = PayloadEncryption.GenerateCsr(SecretCryptoAccessKey);

            // get auth token from IDP and exchange token+csr for apikey+payloadcert
            await DoSsoAsync(ubiqConfiguration);

            _initialized = true;
        }

        internal async Task DoSsoAsync(UbiqConfiguration ubiqConfiguration)
        {
            using (var oauthService = OAuthWebServiceFactory.Create(ubiqConfiguration.Idp.Provider, this))
            {
                _oauthLoginResponse = await oauthService.LoginAsync(ubiqConfiguration.Idp);

                if (_oauthLoginResponse == null)
                {
                    throw new InvalidOperationException("No oauth response");
                }

                if (string.IsNullOrEmpty(_oauthLoginResponse.AccessToken))
                {
                    throw new InvalidOperationException("OAuthResponse AccessToken is invalid");
                }

                using (var ssoService = new SsoWebService(this, ubiqConfiguration))
                {
                    var ssoRequest = new SsoRequest
                    {
                        AccessToken = _oauthLoginResponse.AccessToken,
                        Csr = _payloadCertInfo.CsrPem,
                    };

                    var ssoResponse = await ssoService.SsoAsync(ssoRequest);

                    if (string.IsNullOrEmpty(ssoResponse.ApiCert))
                    {
                        throw new InvalidOperationException("PayloadCertInfo is invalid");
                    }

                    _payloadCertInfo.ApiCert = ssoResponse.ApiCert;

                    // set cert expiration to 1 minute before actual expiration to avoid some edge cases
                    var parser = new X509CertificateParser();
                    var cert = parser.ReadCertificate(Encoding.UTF8.GetBytes(_payloadCertInfo.ApiCert));
                    _payloadCertInfo.ApiCertExpiration = cert.NotAfter.AddMinutes(-1);

                    AccessKeyId = ssoResponse.PublicValue;
                    SecretSigningKey = ssoResponse.SigningValue;
                }
            }
        }

        internal void LoadFromFile(string pathname, string profile = DefaultProfile, string host = DefaultHost)
        {
            const string DEFAULT_SECTION = "default";
            const string SERVER_KEY = "server";
            const string ACCESS_KEY_ID = "access_key_id";
            const string SECRET_SIGNING_KEY = "secret_signing_key";
            const string SECRET_CRYPTO_ACCESS_KEY = "secret_crypto_access_key";
            const string IDP_USERNAME_KEY = "idp_username";
            const string IDP_PASSWORD_KEY = "idp_password";

            if (string.IsNullOrEmpty(pathname))
            {
                pathname = GetDefaultFileLocation();
            }

            if (string.IsNullOrWhiteSpace(pathname))
            {
                return;
            }

            var configParser = new ConfigParser(pathname);
            AccessKeyId = configParser.GetValue(profile, ACCESS_KEY_ID) ?? configParser.GetValue(DEFAULT_SECTION, ACCESS_KEY_ID);
            SecretSigningKey = configParser.GetValue(profile, SECRET_SIGNING_KEY) ?? configParser.GetValue(DEFAULT_SECTION, SECRET_SIGNING_KEY);
            SecretCryptoAccessKey = configParser.GetValue(profile, SECRET_CRYPTO_ACCESS_KEY) ?? configParser.GetValue(DEFAULT_SECTION, SECRET_CRYPTO_ACCESS_KEY);
            Host = configParser.GetValue(profile, SERVER_KEY) ?? configParser.GetValue(DEFAULT_SECTION, SERVER_KEY) ?? host;
            IdpUsername = configParser.GetValue(profile, IDP_USERNAME_KEY) ?? configParser.GetValue(DEFAULT_SECTION, IDP_USERNAME_KEY);
            IdpPassword = configParser.GetValue(profile, IDP_PASSWORD_KEY) ?? configParser.GetValue(DEFAULT_SECTION, IDP_PASSWORD_KEY);
        }
    }
}
