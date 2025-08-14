using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using UbiqSecurity.Config;

namespace UbiqSecurity.Internals.Idp
{
    internal class SelfSignedOAuthWebService : IOAuthWebService
    {
        private const string JwtIssuer = "Ubiq";
        private const string JwtAudience = "";
        private const int JwtLifetimeMinutes = 10;

        public void Dispose()
        {
        }

        public Task<OAuthLoginResponse> LoginAsync(IdpConfig idpConfig)
        {
            var now = DateTime.UtcNow;
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, idpConfig.SelfSignIdentity),
                new Claim(JwtRegisteredClaimNames.Email, idpConfig.SelfSignIdentity),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64),
            };

            var privateKey = GetPrivateKey(idpConfig.SelfSignKey);

            var token = new JwtSecurityToken(
                   JwtIssuer,
                   JwtAudience,
                   claims,
                   now.AddMilliseconds(-30),
                   now.AddMinutes(JwtLifetimeMinutes),
                   new SigningCredentials(privateKey, SecurityAlgorithms.RsaSha256));

            var tokenHandler = new JwtSecurityTokenHandler();

            var result = new OAuthLoginResponse()
            {
                AccessToken = tokenHandler.WriteToken(token),
                ExpiresInSeconds = (int)TimeSpan.FromMinutes(JwtLifetimeMinutes).TotalSeconds,
            };

            return Task.FromResult(result);
        }

        private static RsaSecurityKey GetPrivateKey(string pem)
        {
            using (var reader = new StringReader(pem))
            {
                var pemReader = new PemReader(reader);

                var keyPair = (AsymmetricCipherKeyPair)pemReader.ReadObject();

                RSAParameters rsa = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)keyPair.Private);

                return new RsaSecurityKey(rsa);
            }
        }
    }
}
