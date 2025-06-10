using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.X509;
using UbiqSecurity.Idp;
using static UbiqSecurity.Internals.PayloadEncryption;

namespace UbiqSecurity.Internals
{
    internal class UbiqCredentials : IUbiqCredentials
    {
        // Environment variable names
        private const string UBIQ_ACCESS_KEY_ID = nameof(UBIQ_ACCESS_KEY_ID);
        private const string UBIQ_SECRET_SIGNING_KEY = nameof(UBIQ_SECRET_SIGNING_KEY);
        private const string UBIQ_SECRET_CRYPTO_ACCESS_KEY = nameof(UBIQ_SECRET_CRYPTO_ACCESS_KEY);

        private bool _initialized;
        private PayloadCertInfo _payloadCertInfo;
        private OAuthLoginResponse _oauthLoginResponse;

        internal UbiqCredentials()
        {
        }

        internal UbiqCredentials(string accessKeyId, string secretSigningKey, string secretCryptoAccessKey, string host)
        {
            if (string.IsNullOrEmpty(accessKeyId))
            {
                accessKeyId = Environment.GetEnvironmentVariable(UBIQ_ACCESS_KEY_ID);
            }
            AccessKeyId = accessKeyId;

            if (string.IsNullOrEmpty(secretSigningKey))
            {
                secretSigningKey = Environment.GetEnvironmentVariable(UBIQ_SECRET_SIGNING_KEY);
            }
            SecretSigningKey = secretSigningKey;

            if (string.IsNullOrEmpty(secretCryptoAccessKey))
            {
                secretCryptoAccessKey = Environment.GetEnvironmentVariable(UBIQ_SECRET_CRYPTO_ACCESS_KEY);
            }
            SecretCryptoAccessKey = secretCryptoAccessKey;

            Host = host;
            ValidateCredentials();
        }

        internal UbiqCredentials(string pathname, string profile, string host)
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
                // credentials file not specified, so look for ~/.ubiq/credentials
                string homeDirectory = (Environment.OSVersion.Platform == PlatformID.Unix ||
                        Environment.OSVersion.Platform == PlatformID.MacOSX)
                    ? Environment.GetEnvironmentVariable("HOME")
                    : Environment.ExpandEnvironmentVariables("%HOMEDRIVE%%HOMEPATH%");

                if (Directory.Exists(homeDirectory))
                {
                    pathname = Path.Combine(homeDirectory, ".ubiq", "credentials");
                }
            }

            var configParser = new ConfigParser(pathname);
            AccessKeyId = configParser.GetValue(profile, ACCESS_KEY_ID) ?? configParser.GetValue(DEFAULT_SECTION, ACCESS_KEY_ID);
            SecretSigningKey = configParser.GetValue(profile, SECRET_SIGNING_KEY) ?? configParser.GetValue(DEFAULT_SECTION, SECRET_SIGNING_KEY);
            SecretCryptoAccessKey = configParser.GetValue(profile, SECRET_CRYPTO_ACCESS_KEY) ?? configParser.GetValue(DEFAULT_SECTION, SECRET_CRYPTO_ACCESS_KEY);
            Host = configParser.GetValue(profile, SERVER_KEY) ?? configParser.GetValue(DEFAULT_SECTION, SERVER_KEY) ?? host;
            IdpUsername = configParser.GetValue(profile, IDP_USERNAME_KEY) ?? configParser.GetValue(DEFAULT_SECTION, IDP_USERNAME_KEY);
            IdpPassword = configParser.GetValue(profile, IDP_PASSWORD_KEY) ?? configParser.GetValue(DEFAULT_SECTION, IDP_PASSWORD_KEY);

            ValidateCredentials();
        }

        public string Host { get; set; }

        public string AccessKeyId { get; set; }

        public string SecretSigningKey { get; set; }

        public string SecretCryptoAccessKey { get; set; }

        public string IdpUsername { get; set; }

        public string IdpPassword { get; set; }

        public bool IsIdp => !string.IsNullOrEmpty(IdpUsername) && !string.IsNullOrEmpty(IdpPassword);

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

        internal void ValidateCredentials()
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
    }
}
