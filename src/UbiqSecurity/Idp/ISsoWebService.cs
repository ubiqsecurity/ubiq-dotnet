using System.Threading.Tasks;

namespace UbiqSecurity.Idp
{
    internal interface ISsoWebService
    {
        IUbiqCredentials Credentials { get; }

        UbiqConfiguration Configuration { get; }

        Task<SsoResponse> SsoAsync(SsoRequest request);
    }
}
