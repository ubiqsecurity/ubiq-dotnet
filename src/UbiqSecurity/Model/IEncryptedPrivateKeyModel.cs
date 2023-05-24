namespace UbiqSecurity.Model
{
	internal interface IEncryptedPrivateKeyModel
	{
		string EncryptedPrivateKey { get; set; }

		string WrappedDataKey { get; set; }

		byte[] UnwrappedDataKey { get; set; }
	}
}
