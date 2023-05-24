namespace UbiqSecurity
{
	public interface IUbiqCredentials
	{
		string AccessKeyId { get; }

		string SecretSigningKey { get; }

		string SecretCryptoAccessKey { get; }

		string Host { get; }
	}
}
