using System.Threading.Tasks;

namespace UbiqSecurity.Internals.Idp
{
    internal interface ISsoWebService
    {
        IUbiqCredentials Credentials { get; }

        UbiqConfiguration Configuration { get; }

        Task<SsoResponse> SsoAsync(SsoRequest request);
    }
}
