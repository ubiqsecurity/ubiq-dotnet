using System;
using System.Threading.Tasks;
using UbiqSecurity.Config;

namespace UbiqSecurity.Internals.Idp
{
    internal interface IOAuthWebService : IDisposable
    {
        Task<OAuthLoginResponse> LoginAsync(IdpConfig idpConfig);
    }
}
