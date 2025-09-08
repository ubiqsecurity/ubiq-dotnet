using System.Threading.Tasks;

namespace UbiqSecurity
{
    public interface IUbiqCredentials
    {
        string Host { get; set; }

        string AccessKeyId { get; set; }

        string SecretSigningKey { get; set; }

        string SecretCryptoAccessKey { get; set; }

        string IdpUsername { get; set; }

        string IdpPassword { get; set; }

        bool IsIdp { get; }

        string IdpPayloadCert { get; }

        Task CheckInitAndExpirationAsync(UbiqConfiguration ubiqConfiguration);

        void Validate();
    }
}
