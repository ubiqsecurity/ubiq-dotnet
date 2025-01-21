using System;
using System.Threading.Tasks;
using UbiqSecurity.Config;

namespace UbiqSecurity.Idp
{
    internal interface IOAuthWebService : IDisposable
    {
        Task<OAuthLoginResponse> LoginAsync(IdpConfig idpConfig);
    }
}
