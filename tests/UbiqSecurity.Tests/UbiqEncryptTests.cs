using UbiqSecurity.Internals;

namespace UbiqSecurity.Tests
{
	public class UbiqEncryptTests
	{
		[Fact]
		public async Task EncryptAsync_ValidInput_ReturnsEncryptedBytes()
		{
			var credentials = UbiqCredentials.CreateFromFile(string.Empty, "ubiq-dotnet");

			byte[] originalBytes = await File.ReadAllBytesAsync("UbiqSecurity.Tests.dll");

			using var encryptor = new UbiqEncrypt(credentials, 1);

			encryptor.AddReportingUserDefinedMetadata("{ \"encryption_wrapper\" : true }");

			var cipherBytes = await encryptor.EncryptAsync(originalBytes);

			Assert.NotEqual(originalBytes, cipherBytes);

			using var decryptor = new UbiqDecrypt(credentials);
			var plainBytes = await decryptor.DecryptAsync(cipherBytes);

			Assert.Equal(originalBytes, plainBytes);
		}
	}
}
