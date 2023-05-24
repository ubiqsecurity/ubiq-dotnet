namespace UbiqSecurity.Internals
{
	internal class FfsKeyRecord
	{
		public string EncryptedPrivateKey { get; set; }

		public string WrappedDataKey { get; set; }

		public int KeyNumber { get; set; }
	}
}
