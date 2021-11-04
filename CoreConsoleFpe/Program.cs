using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using UbiqSecurity;

namespace UbiqConsole
{
	class Program
	{
		static int Main(string[] args)
		{
			try
			{
				Parser.Default.ParseArguments<CommandLineOptions>(args)
					.WithParsed(options => RunCommand(options))
					.WithNotParsed(errors => HandleCommandLineErrors(errors));

				return 0;       // zero means success
			}
			catch (Exception ex)
			{
				return 1;       // non-zero means failure
			}
			finally
			{
				Console.WriteLine($"{nameof(UbiqConsole)}: done.");
			}
		}

		#region Private Methods
		private static void RunCommand(CommandLineOptions options)
		{
		try
		{
				if (options.Simple == options.Bulk)
				{
					throw new InvalidOperationException("simple or bulk API option need to be specified but not both");
				}

				if (string.IsNullOrEmpty(options.EncryptText) && string.IsNullOrEmpty(options.DecryptText))
				{
					throw new InvalidOperationException("Encryption or Decryption must be specified");
				}

				if (!string.IsNullOrEmpty(options.EncryptText) && !string.IsNullOrEmpty(options.DecryptText))
				{
					throw new InvalidOperationException("Encryption text or Decrytion text have to be specified but not both");
				}

				if (string.IsNullOrEmpty(options.FfsName))
				{
					throw new InvalidOperationException("ffsname must be specified");
				}

				IUbiqCredentials ubiqCredentials;
				if (options.Credentials == null)
				{
					try
					{
						// no file specified, so fall back to ENV vars and default host, if any
						ubiqCredentials = UbiqFactory.CreateCredentials(
							accessKeyId: null,
							secretSigningKey: null,
							secretCryptoAccessKey: null,
							host: null);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Exception caught while setting credentials:  {ex.Message}");
						Console.WriteLine($"Required environment variables not set");
						return;
					}
				}
				else
				{
					try
					{
						// read credentials from caller-specified section of specified config file
						ubiqCredentials = UbiqFactory.ReadCredentialsFromFile(options.Credentials, options.Profile);
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Exception caught while reading credentials:  {ex.Message}");
						Console.WriteLine($"Check pathname of credentials file '{options.Credentials}' and chosen profile '{options.Profile}'");
						return;
					}
				}

				if (options.Simple)
				{
					if (!string.IsNullOrEmpty(options.EncryptText))
					{
						SimpleEncryptionAsync(options, ubiqCredentials).GetAwaiter().GetResult();
					}
					else
					{
						SimpleDecryptionAsync(options, ubiqCredentials).GetAwaiter().GetResult();
					}
				}
				else
				{
					if (!string.IsNullOrEmpty(options.EncryptText))
					{
						BulkEncryptionAsync(options, ubiqCredentials).GetAwaiter().GetResult();
					}
					else
					{
						BulkDecryptionAsync(options, ubiqCredentials).GetAwaiter().GetResult();
					}
				}
			}
			catch(Exception e)
			{
				Console.WriteLine($"Exception: {e.Message}");
				Environment.Exit(1);
			}
		}

		private static async Task SimpleEncryptionAsync(CommandLineOptions options, IUbiqCredentials ubiqCredentials)
		{
			// default tweak in case the FFS model allows for external tweak insertion          
			byte[] tweakFF1 = {};

			var cipherText = await UbiqFPEEncryptDecrypt.EncryptAsync(ubiqCredentials, options.EncryptText, options.FfsName, tweakFF1);

			Console.WriteLine($"ENCRYPTED cipher= {cipherText}\n");
			return;
		}

		private static async Task SimpleDecryptionAsync(CommandLineOptions options, IUbiqCredentials ubiqCredentials)
		{
			// default tweak in case the FFS model allows for external tweak insertion          
			byte[] tweakFF1 = {};

			var plainText = await UbiqFPEEncryptDecrypt.DecryptAsync(ubiqCredentials, options.DecryptText, options.FfsName, tweakFF1);

			Console.WriteLine($"DECRYPTED plainText= {plainText}\n");
			return;
		}

		private static async Task BulkEncryptionAsync(CommandLineOptions options, IUbiqCredentials ubiqCredentials)
		{
			// default tweak in case the FFS model allows for external tweak insertion          
			byte[] tweakFF1 = {};

			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(ubiqCredentials))
			{
				var cipherText = await ubiqEncryptDecrypt.EncryptAsync(options.FfsName, options.EncryptText, tweakFF1);
				Console.WriteLine($"ENCRYPTED cipher= {cipherText}\n");
			}

			return;
		}

		private static async Task BulkDecryptionAsync(CommandLineOptions options, IUbiqCredentials ubiqCredentials)
		{
			// default tweak in case the FFS model allows for external tweak insertion          
			byte[] tweakFF1 = {};

			using (var ubiqEncryptDecrypt = new UbiqFPEEncryptDecrypt(ubiqCredentials))
			{
				var plainText = await ubiqEncryptDecrypt.DecryptAsync(options.FfsName, options.DecryptText, tweakFF1);
				Console.WriteLine($"DECRYPTED plainText= {plainText}\n");
			}

			return;
		}

		private static void HandleCommandLineErrors(IEnumerable<Error> errors)
		{
			int errorCount = 0;
			foreach (var error in errors)
			{
				if (!(error is VersionRequestedError) &&
					!(error is HelpRequestedError))
				{
					Console.WriteLine($"invalid command line: {error.Tag}");
					errorCount++;
				}
			}

			if (errorCount > 0)
			{
				throw new ArgumentException("one or more command line arguments missing or invalid");
			}
		}
		#endregion
	}

	internal class CommandLineOptions
	{
		[Option('e', "encrypttext", HelpText = "Set the field text value to encrypt and will return the encrypted cipher text")]
		public string EncryptText { get; set; }

		[Option('d', "decrypttext", HelpText = "Set the cipher text value to decrypt and will return the decrypted text")]
		public string DecryptText { get; set; }

		[Option('s', "simple", Default = false, HelpText = "Use the simple encryption / decryption interfaces")]
		public bool Simple { get; set; }

		[Option('b', "bulk", Default = false, HelpText = "Use the bulk encryption / decryption interfaces")]
		public bool Bulk { get; set; }

		[Option('n', "ffsname", HelpText = "Set the ffs name, for example SSN")]
		public string FfsName { get; set; }

		[Option('h', "help", Default = false, HelpText = "Print app parameter summary")]
		public bool Help { get; set; }

		[Option('c', "creds", Required = false, HelpText = "Set the file name with the API credentials", Default = null)]
		public string Credentials { get; set; }

		[Option('P', "profile", Required = false, HelpText = "Identify the profile within the credentials file", Default = "default")]
		public string Profile { get; set; }

		[Option('V', "version", Required = false, HelpText = "Show program's version number and exit")]
		public bool Version { get; set; }
	}
}
