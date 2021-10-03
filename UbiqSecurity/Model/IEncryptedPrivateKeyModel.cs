namespace UbiqSecurity.Model
{
	public interface IEncryptedPrivateKeyModel
	{
		string EncryptedPrivateKey { get; set; }

		string WrappedDataKey { get; set; }

		byte[] UnwrappedDataKey { get; set; }
	}
}
