namespace UbiqSecurity.Internals.WebService.Models
{
    internal interface IPayloadEncryptionRequest
    {
        string PayloadCertificate { get; set; }
    }
}
