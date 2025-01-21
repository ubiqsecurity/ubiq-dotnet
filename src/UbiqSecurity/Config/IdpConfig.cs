namespace UbiqSecurity.Config
{
    public enum IdpProvider
    {
        MicrosoftEntraId
    }

    public class IdpConfig
    {
        public IdpConfig()
        {
        }

        public IdpProvider Provider { get; set; }

        public string IdpTokenEndpointUrl { get; set; }

        public string IdpClientSecret { get; set; }

        public string IdpTenantId { get; set; }

        public string CustomerId { get; set; }
    }
}
