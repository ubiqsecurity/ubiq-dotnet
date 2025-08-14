namespace UbiqSecurity.Internals.WebService.Models
{
    internal interface IEncryptedPrivateKeyModel
    {
        string EncryptedPrivateKey { get; set; }

        string WrappedDataKey { get; set; }

        byte[] UnwrappedDataKey { get; set; }
    }
}
