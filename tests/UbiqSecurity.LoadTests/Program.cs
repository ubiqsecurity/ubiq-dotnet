using System.Linq;
using CommandLine;

namespace UbiqSecurity.LoadTests
{
	internal class Program
	{
		static async Task<int> Main(string[] args)
		{
			try
			{
				(await Parser.Default.ParseArguments<CommandLineOptions>(args)
					.WithParsedAsync(async (options) => await RunCommand(options)))
					.WithNotParsed(errors => HandleCommandLineErrors(errors));

				return 0;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Exception: {ex.Message}");
				return 1;
			}
			finally
			{
				Console.WriteLine($"{nameof(LoadTests)}: done.");
			}
		}

		private static async Task RunCommand(CommandLineOptions options)
		{
            bool isDirectory = false;

			// input file
			if (string.IsNullOrEmpty(options.InputFileName))
			{
				throw new InvalidOperationException("input file name (-i) must be specified ");
			}

            if (Directory.Exists(options.InputFileName))
            {
                isDirectory = true;
            }

			if (!isDirectory && !File.Exists(options.InputFileName))
			{
				throw new InvalidOperationException($"input file {options.InputFileName} not found");
			}

			// credentials
			IUbiqCredentials ubiqCredentials;
			if (string.IsNullOrEmpty(options.Credentials))
			{
				// no file specified, so fall back to ENV vars and default host, if any
				ubiqCredentials = UbiqFactory.CreateCredentials(
					accessKeyId: null,
					secretSigningKey: null,
					secretCryptoAccessKey: null);
			}
			else
			{
				ubiqCredentials = UbiqFactory.ReadCredentialsFromFile(options.Credentials, options.Profile);
			}

			
			using var benchmark = new Benchmark(ubiqCredentials);
            var filePaths = isDirectory ? Directory.EnumerateFiles(options.InputFileName, "*.json") : new List<string> { options.InputFileName };
            var timings = await benchmark.RunAsync(filePaths);

			PrintTimings(timings);
		}

		private static void PrintTimings(Timings timings)
		{
			Console.WriteLine($"\tEncryption count: {timings.EncryptionTimes.Count}");
			var encryptions = timings.EncryptionTimes.GroupBy(x => x.Key);
			foreach (var encryption in encryptions)
			{
				Console.WriteLine($"\tEncryption {encryption.Key} average time: {encryption.SelectMany(x => x.Value).Average()} (ms)");
			}

			Console.WriteLine($"\tDecryption count: {timings.DecryptionTimes.Count}");
			Console.WriteLine($"\tDecryption average time: {timings.DecryptionTimes.Average()} (ms)");

            Console.WriteLine($"\tError Count: {timings.ErrorCount}");
            if (timings.ErrorCount > 0)
            {
                throw new Exception("Error Count > 0");
            }
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
	}

	internal class CommandLineOptions
	{
		[Option('i', HelpText = "Input file name")]
		public string InputFileName { get; set; }

		[Option('e', Default = null, HelpText = "Maximum allowed average encrypt time in microseconds. Not including first call to server")]
		public long? MaxAllowedAverageEncryptTime { get; set; }

		[Option('d', Default = null, HelpText = "Maximum allowed average encrypt time in microseconds. Not including first call to server")]
		public long? MaxAllowedAverageDecryptTime { get; set; }

		[Option('E', Default = null, HelpText = "Maximum allowed total encrypt time in microseconds. Not including first call to server")]
		public long? MaxAllowedTotalEncryptTime { get; set; }

		[Option('D', Default = null, HelpText = "Maximum allowed total encrypt time in microseconds. Not including first call to server")]
		public long? MaxAllowedTotalDecryptTime { get; set; }

		[Option('h', "help", Default = false, HelpText = "Print app parameter summary")]
		public bool Help { get; set; }

		[Option('c', "creds", Required = false, HelpText = "Set the file name with the API credentials", Default = null)]
		public string Credentials { get; set; }

		[Option('P', "profile", Required = false, HelpText = "Identify the profile within the credentials file", Default = "default")]
		public string Profile { get; set; }
	}
}
