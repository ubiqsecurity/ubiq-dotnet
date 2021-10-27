using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UbiqSecurity;
using Org.BouncyCastle.OpenSsl;
using System.Text;

namespace UbiqSecurityTests
{
	[TestClass]
	public class UbiqFpeEncryptDecryptTests
	{
		[TestMethod]
		public async Task EncryptFPE_FFS_ALPHANUM_SSN_Success()
		{
			var credentials = UbiqFactory.ReadCredentialsFromFile("..\\..\\credentials", "default");

			byte[] tweakFF1 = new byte[0];

			var ffsName = "ALPHANUM_SSN";
			var original = "123-45-6789";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var cipher = await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1);
				var decrypted = await ubiqEncryptDecrypt.DecryptAsync(ffsName, cipher, tweakFF1);
				Assert.AreEqual(original, decrypted);
			}
		}

		[TestMethod]
		public async Task EncryptFPE_FFS_ALPHANUM_SSN_ValidPassthroughCharacters_Success()
		{
			var credentials = UbiqFactory.ReadCredentialsFromFile("..\\..\\credentials", "default");

			byte[] tweakFF1 = new byte[0];

			var ffsName = "ALPHANUM_SSN";
			var original = " 01&23-456-78-90";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var cipher = await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1);
				var decrypted = await ubiqEncryptDecrypt.DecryptAsync(ffsName, cipher, tweakFF1);

				Assert.AreEqual(original, decrypted);
			}
		}

		[TestMethod]
		public async Task EncryptFPE_FFS_ALPHANUM_SSN_InValidPassthroughCharacters_Fail()
		{
			var credentials = UbiqFactory.ReadCredentialsFromFile("..\\..\\credentials", "default");

			byte[] tweakFF1 = new byte[0];

			var ffsName = "ALPHANUM_SSN";
			var original = "1$23-45-6789";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var ex = await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(async () => await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1));
				Assert.IsTrue(ex.Message.Contains("invalid character found in the input"));
			}
		}

		[TestMethod]
		public async Task EncryptFPE_FFS_BIRTH_DATE_Success()
		{
			// TODO: Figure out how to handle credentials
			var credentials = UbiqFactory.ReadCredentialsFromFile("..\\..\\credentials", "default");

			byte[] tweakFF1 = new byte[0];

			var ffsName = "BIRTH_DATE";
			var original = "01-01-2020";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var cipher = await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1);
				var decrypted = await ubiqEncryptDecrypt.DecryptAsync(ffsName, cipher, tweakFF1);

				Assert.AreEqual(original, decrypted);
			}
		}

		[TestMethod]
		public async Task EncryptFPE_FFS_BIRTH_DATE_InValidPassthroughCharacters_Fail()
		{
			var credentials = UbiqFactory.ReadCredentialsFromFile("..\\..\\credentials", "default");

			byte[] tweakFF1 = new byte[0];

			var ffsName = "BIRTH_DATE";
			var original = "01/01/2020";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var ex = await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(async () => await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1));
				Assert.IsTrue(ex.Message.Contains("invalid character found in the input"));
			}
		}

		[TestMethod]
		public async Task EncryptFPE_FFS_SO_ALPHANUM_PIN_Success()
		{
			// TODO: Figure out how to handle credentials
			var credentials = UbiqFactory.ReadCredentialsFromFile("..\\..\\credentials", "default");

			byte[] tweakFF1 = new byte[0];

			var ffsName = "SO_ALPHANUM_PIN";
			var original = "ABCD";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var cipher = await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1);
				var decrypted = await ubiqEncryptDecrypt.DecryptAsync(ffsName, cipher, tweakFF1);

				Assert.AreEqual(original, decrypted);
			}
		}

		[TestMethod]
		public async Task EncryptFPE_FFS_SO_ALPHANUM_PIN_ALL_NUMBERS__Success()
		{
			// TODO: Figure out how to handle credentials
			var credentials = UbiqFactory.ReadCredentialsFromFile("..\\..\\credentials", "default");

			byte[] tweakFF1 = new byte[0];

			var ffsName = "SO_ALPHANUM_PIN";
			var original = "1234";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var cipher = await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1);
				var decrypted = await ubiqEncryptDecrypt.DecryptAsync(ffsName, cipher, tweakFF1);

				Assert.AreEqual(original, decrypted);
			}
		}

		[TestMethod]
		public async Task EncryptFPE_FFS_SO_ALPHANUM_PIN_ValidPassthroughCharacters_Success()
		{
			var credentials = UbiqFactory.ReadCredentialsFromFile("..\\..\\credentials", "default");

			byte[] tweakFF1 = new byte[0];

			var ffsName = "SO_ALPHANUM_PIN";
			var original = "AB^CD";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var cipher = await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1);
				var decrypted = await ubiqEncryptDecrypt.DecryptAsync(ffsName, cipher, tweakFF1);

				Assert.AreEqual(original, decrypted);
			}
		}

		[TestMethod]
		public async Task EncryptFPE_FFS_SO_ALPHANUM_PIN_InValidPassthroughCharacters_Fail()
		{
			var credentials = UbiqFactory.ReadCredentialsFromFile("..\\..\\credentials", "default");

			byte[] tweakFF1 = new byte[0];

			var ffsName = "SO_ALPHANUM_PIN";
			var original = "AB+CD";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var ex = await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(async () => await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1));
				Assert.IsTrue(ex.Message.Contains("invalid character found in the input"));
			}
		}

		[TestMethod]
		public async Task EncryptFPE_FFS_GENERIC_STRING_Success()
		{
			var credentials = UbiqFactory.ReadCredentialsFromFile("..\\..\\credentials", "default");

			byte[] tweakFF1 = {
				(byte)0x39, (byte)0x38, (byte)0x37, (byte)0x36,
				(byte)0x35, (byte)0x34, (byte)0x33, (byte)0x32,
				(byte)0x31, (byte)0x30, (byte)0x33, (byte)0x32,
				(byte)0x31, (byte)0x30, (byte)0x32,
			};

			var ffsName = "GENERIC_STRING";
			var original = "A STRING OF AT LEAST 15 UPPER CHARACTERS";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var cipher = await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1);
				var decrypted = await ubiqEncryptDecrypt.DecryptAsync(ffsName, cipher, tweakFF1);

				Assert.AreEqual(original, decrypted);
			}
		}

		[TestMethod]
		public async Task EncryptFPE_XPlatformValidation_Success()
		{
			var credentials = UbiqFactory.ReadCredentialsFromFile("..\\..\\credentials", "default");

			byte[] tweakFF1 = new byte[0];

			var ffsName = "ALPHANUM_SSN";
			var original = "123 456 789";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var cipher = await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1);
				//Debug.WriteLine($"encrypted: {cipher}");
				var decrypted = await ubiqEncryptDecrypt.DecryptAsync(ffsName, cipher, tweakFF1);
				//Debug.WriteLine($"decrypted: {decrypted}");
				Assert.AreEqual(original, decrypted);
			}
		}


		[TestMethod]
		public async Task EncryptFPE_InvalidFFS()
		{
			var credentials = UbiqFactory.ReadCredentialsFromFile("..\\..\\credentials", "default");

			byte[] tweakFF1 = new byte[0];

			var ffsName = "ERROR FFS";
			var original = "ABCDEFGHI";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var ex = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1));
				Assert.IsTrue(ex.Message.Contains("Value cannot be null."));
			}
		}


		[TestMethod]
		public async Task EncryptFPE_InvalidCredentials()
		{
			var credentials = UbiqFactory.CreateCredentials("a", "b", "c", "d");

			byte[] tweakFF1 = new byte[0];

			var ffsName = "ALPHANUM_SSN";
			var original = "ABCDEFGHI";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var ex = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1));
				Assert.IsTrue(ex.Message.Contains("Value cannot be null."));
			}
		}


		[TestMethod]
		public async Task EncryptFPE_Invalid_PT_CT()
		{
			var credentials = UbiqFactory.ReadCredentialsFromFile("..\\..\\credentials", "default");

			byte[] tweakFF1 = new byte[0];

			var ffsName = "SSN";
			var original = " 123456789$";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var ex = await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(async () => await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1));
				Assert.IsTrue(ex.Message.Contains("Specified argument was out of the range of valid values."));
			}
		}


		[TestMethod]
		public async Task EncryptFPE_Invalid_LEN_1()
		{
			var credentials = UbiqFactory.ReadCredentialsFromFile("..\\..\\credentials", "default");

			byte[] tweakFF1 = new byte[0];

			var ffsName = "SSN";
			var original = " 1234";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1));
				//Console.WriteLine("ex.Message is {0}", ex.Message);
				Assert.IsTrue(ex.Message.Contains("Input length does not match FFS parameters."));
			}
		}


		[TestMethod]
		public async Task EncryptFPE_Invalid_LEN_2()
		{
			var credentials = UbiqFactory.ReadCredentialsFromFile("..\\..\\credentials", "default");

			byte[] tweakFF1 = new byte[0];

			var ffsName = "SSN";
			var original = " 12345678901234567890";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var ex = await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1));
				Assert.IsTrue(ex.Message.Contains("Input length does not match FFS parameters."));
			}
		}

		[TestMethod]
		public async Task EncryptFPE_Invalid_specific_creds_1()
		{
			var credentials = UbiqFactory.ReadCredentialsFromFile("..\\..\\credentials", "default");
			credentials = UbiqFactory.CreateCredentials(credentials.AccessKeyId.Substring(0, 1),
															credentials.SecretSigningKey,
															credentials.SecretCryptoAccessKey,
															credentials.Host);

			byte[] tweakFF1 = new byte[0];

			var ffsName = "ALPHANUM_SSN";
			var original = " 123456789";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var ex = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1));
				Assert.IsTrue(ex.Message.Contains("Value cannot be null."));
			}
		}

		[TestMethod]
		public async Task EncryptFPE_Invalid_specific_creds_2()
		{
			var credentials = UbiqFactory.ReadCredentialsFromFile("..\\..\\credentials", "default");
			credentials = UbiqFactory.CreateCredentials(credentials.AccessKeyId,
															credentials.SecretSigningKey.Substring(0, 1),
															credentials.SecretCryptoAccessKey,
															credentials.Host);

			byte[] tweakFF1 = new byte[0];

			var ffsName = "ALPHANUM_SSN";
			var original = " 123456789";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var ex = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1));
				Assert.IsTrue(ex.Message.Contains("Value cannot be null."));
			}
		}


		[TestMethod]
		public async Task EncryptFPE_Invalid_specific_creds_3()
		{
			var credentials = UbiqFactory.ReadCredentialsFromFile("..\\..\\credentials", "default");
			credentials = UbiqFactory.CreateCredentials(credentials.AccessKeyId,
															credentials.SecretSigningKey,
															credentials.SecretCryptoAccessKey.Substring(0, 1),
															credentials.Host);

			byte[] tweakFF1 = new byte[0];

			var ffsName = "ALPHANUM_SSN";
			var original = " 123456789";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var ex = await Assert.ThrowsExceptionAsync<PemException>(async () => await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1));
				Assert.IsTrue(ex.Message.Contains("problem creating ENCRYPTED private key"));
			}
		}


		[TestMethod]
		public async Task EncryptFPE_Invalid_specific_creds_4()
		{
			var credentials = UbiqFactory.ReadCredentialsFromFile("..\\..\\credentials", "default");
			credentials = UbiqFactory.CreateCredentials(credentials.AccessKeyId,
															credentials.SecretSigningKey,
															credentials.SecretCryptoAccessKey,
															"pi.ubiqsecurity.com");

			byte[] tweakFF1 = new byte[0];

			var ffsName = "ALPHANUM_SSN";
			var original = " 123456789";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var ex = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1));
				Assert.IsTrue(ex.Message.Contains("Value cannot be null."));
			}
		}


		[TestMethod]
		public async Task EncryptFPE_Invalid_specific_creds_5()
		{
			var credentials = UbiqFactory.ReadCredentialsFromFile("..\\..\\credentials", "default");
			credentials = UbiqFactory.CreateCredentials(credentials.AccessKeyId,
															credentials.SecretSigningKey,
															credentials.SecretCryptoAccessKey,
															"ps://api.ubiqsecurity.com");

			byte[] tweakFF1 = new byte[0];

			var ffsName = "ALPHANUM_SSN";
			var original = " 123456789";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var ex = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1));
				Assert.IsTrue(ex.Message.Contains("Value cannot be null."));
			}
		}


		[TestMethod]
		public async Task EncryptFPE_Invalid_specific_creds_6()
		{
			var credentials = UbiqFactory.ReadCredentialsFromFile("..\\..\\credentials", "default");
			credentials = UbiqFactory.CreateCredentials(credentials.AccessKeyId,
															credentials.SecretSigningKey,
															credentials.SecretCryptoAccessKey,
															"https://google.com");

			byte[] tweakFF1 = new byte[0];

			var ffsName = "ALPHANUM_SSN";
			var original = " 123456789";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var ex = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1));
				Assert.IsTrue(ex.Message.Contains("Value cannot be null."));
			}
		}



		[TestMethod]
		public async Task EncryptFPE_Invalid_keynum()
		{
			var credentials = UbiqFactory.ReadCredentialsFromFile("..\\..\\credentials", "default");

			byte[] tweakFF1 = new byte[0];

			var ffsName = "SO_ALPHANUM_PIN";
			//var original = " 0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			var original = " 0123";
			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials))
			{
				var cipher = await ubiqEncryptDecrypt.EncryptAsync(ffsName, original, tweakFF1);
				StringBuilder newcipher = new StringBuilder(cipher);
				newcipher[0] = '}';
				var ex = await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await ubiqEncryptDecrypt.DecryptAsync(ffsName, newcipher.ToString(), tweakFF1));
				Assert.IsTrue(ex.Message.Contains(" "));
			}
		}



	}
}
