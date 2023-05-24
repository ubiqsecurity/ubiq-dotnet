using System;
using System.Collections.Generic;
using System.IO;
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
					.WithNotParsed((errors) => HandleCommandLineErrors(errors));

				return 0;       // zero means success
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{nameof(UbiqConsole)} exception: {ex}");
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
			if (options.Simple == options.Piecewise)
			{
				throw new InvalidOperationException("simple or piecewise API option need to be specified but not both");
			}

			if (options.Encrypt == options.Decrypt)
			{
				throw new InvalidOperationException("Encryption or Decrytion have to be specified but not both");
			}

			if (!File.Exists(options.InputFile))
			{
				throw new InvalidOperationException($"Input file does not exist: {options.InputFile}");
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
					Console.WriteLine($"Required environment variables not set'");
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

			// check input file size - we already know it exists
			{
				long maxSimpleSize = 50 * 0x100000;     // 50MB
				var inputFileInfo = new FileInfo(options.InputFile);
				if (options.Simple && (inputFileInfo.Length > maxSimpleSize))
				{
					Console.WriteLine("NOTE: This is only for demonstration purposes and is designed to work on memory");
					Console.WriteLine("      constrained devices.  Therefore, this sample application will switch to");
					Console.WriteLine("      the piecewise APIs for files larger than {0} bytes in order to reduce", maxSimpleSize);
					Console.WriteLine("      excessive resource usages on resource constrained IoT devices");
					options.Simple = false;
					options.Piecewise = true;
				}
			}

			if (options.Simple)
			{
				if (options.Encrypt)
				{
					SimpleEncryptionAsync(options.InputFile, options.OutputFile, ubiqCredentials).Wait();
				}
				else
				{
					SimpleDecryptionAsync(options.InputFile, options.OutputFile, ubiqCredentials).Wait();
				}
			}
			else
			{
				if (options.Encrypt)
				{
					PiecewiseEncryptionAsync(options.InputFile, options.OutputFile, ubiqCredentials).Wait();
				}
				else
				{
					PiecewiseDecryptionAsync(options.InputFile, options.OutputFile, ubiqCredentials).Wait();
				}
			}
		}

		private static async Task SimpleEncryptionAsync(string inFile, string outFile, IUbiqCredentials ubiqCredentials)
		{
			var plainBytes = File.ReadAllBytes(inFile);

			using var encryptor = new UbiqEncrypt(ubiqCredentials, 1);

			var cipherBytes = await encryptor.EncryptAsync(plainBytes);
			
			File.WriteAllBytes(outFile, cipherBytes);
		}

		private static async Task SimpleDecryptionAsync(string inFile, string outFile, IUbiqCredentials ubiqCredentials)
		{
			var cipherBytes = File.ReadAllBytes(inFile);
			var plainBytes = await UbiqDecrypt.DecryptAsync(ubiqCredentials, cipherBytes);
			File.WriteAllBytes(outFile, plainBytes);
		}

		private static async Task PiecewiseEncryptionAsync(string inFile, string outFile, IUbiqCredentials ubiqCredentials)
		{
			using var plainStream = new FileStream(inFile, FileMode.Open);
			using var cipherStream = new FileStream(outFile, FileMode.Create);
			using var ubiqEncrypt = new UbiqEncrypt(ubiqCredentials, 1);
			
			var cipherBytes = await ubiqEncrypt.BeginAsync();
			cipherStream.Write(cipherBytes, 0, cipherBytes.Length);

			var plainBytes = new byte[0x20000];
			int bytesRead = 0;
			while ((bytesRead = plainStream.Read(plainBytes, 0, plainBytes.Length)) > 0)
			{
				cipherBytes = ubiqEncrypt.Update(plainBytes, 0, bytesRead);
				cipherStream.Write(cipherBytes, 0, cipherBytes.Length);
			}

			cipherBytes = ubiqEncrypt.End();
			cipherStream.Write(cipherBytes, 0, cipherBytes.Length);
		}

		private static async Task PiecewiseDecryptionAsync(string inFile, string outFile, IUbiqCredentials ubiqCredentials)
		{
			using var cipherStream = new FileStream(inFile, FileMode.Open);
			using var plainStream = new FileStream(outFile, FileMode.Create);
			using var ubiqDecrypt = new UbiqDecrypt(ubiqCredentials);

			var plainBytes = ubiqDecrypt.Begin();
			plainStream.Write(plainBytes, 0, plainBytes.Length);

			var cipherBytes = new byte[0x20000];
			int bytesRead = 0;
			while ((bytesRead = cipherStream.Read(cipherBytes, 0, cipherBytes.Length)) > 0)
			{
				plainBytes = await ubiqDecrypt.UpdateAsync(cipherBytes, 0, bytesRead);
				plainStream.Write(plainBytes, 0, plainBytes.Length);
			}

			plainBytes = ubiqDecrypt.End();
			plainStream.Write(plainBytes, 0, plainBytes.Length);
		}

		private static void HandleCommandLineErrors(IEnumerable<Error> errors)
		{
			int errorCount = 0;
			foreach (var error in errors)
			{
				if (error is not VersionRequestedError && error is not HelpRequestedError)
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
		[Option('e', "encrypt", Default = false, HelpText = "Encrypt the contents of the input file and write the results to output file")]
		public bool Encrypt { get; set; }

		[Option('d', "decrypt", Default = false, HelpText = "Decrypt the contents of the input file and write the results to output file")]
		public bool Decrypt { get; set; }

		[Option('s', "simple", Default = false, HelpText = "Use the simple encryption / decryption interfaces")]
		public bool Simple { get; set; }

		[Option('p', "piecewise", Default = false, HelpText = "Use the piecewise encryption / decryption interfaces")]
		public bool Piecewise { get; set; }

		[Option('i', "in", Required = true, HelpText = "Set input file name")]
		public string InputFile { get; set; }

		[Option('o', "out", Required = true, HelpText = "Set output file name")]
		public string OutputFile { get; set; }

		[Option('c', "creds", Required = false, HelpText = "Set the file name with the API credentials", Default = null)]
		public string Credentials { get; set; }

		[Option('P', "profile", Required = false, HelpText = "Identify the profile within the credentials file", Default = "default")]
		public string Profile { get; set; }

		[Option('V', "version", Required = false, HelpText = "Show program's version number and exit")]
		public bool Version { get; set; }
	}
}