using System;
using UbiqSecurity.Config;

namespace UbiqSecurity.Idp
{
    internal static class OAuthWebServiceFactory
    {
        public static IOAuthWebService Create(IdpProvider idpProvider, IUbiqCredentials ubiqCredentials)
        {
            switch (idpProvider)
            {
                case IdpProvider.MicrosoftEntraId:
                    return new EntraIdOAuthWebService(ubiqCredentials);
                case IdpProvider.Okta:
                    return new OktaOAuthWebService(ubiqCredentials);
                default:
                    throw new NotImplementedException($"IdpProvider '{idpProvider}' not implemented");
            }
        }
    }
}
