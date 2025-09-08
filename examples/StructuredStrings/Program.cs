using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommandLine;
using UbiqSecurity;

namespace StructuredStrings
{
    public class Program
    {
        private static async Task<int> Main(string[] args)
        {
            try
            {
                (await Parser.Default.ParseArguments<CommandLineOptions>(args)
                    .WithParsedAsync(async (options) => await RunCommand(options)))
                    .WithNotParsed((errors) => HandleCommandLineErrors(errors));

                return 0;       // zero means success
            }
            catch (Exception)
            {
                return 1;       // non-zero means failure
            }
            finally
            {
                Console.WriteLine($"{nameof(StructuredStrings)}: done.");
            }
        }

        private static async Task RunCommand(CommandLineOptions options)
        {
            try
            {
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

                var cryptoBuilder = new CryptographyBuilder();

                if (options.Credentials == null)
                {
                    cryptoBuilder.WithCredentialsFromEnvironmentVariables();
                }
                else
                {
                    try
                    {
                        // read credentials from caller-specified section of specified config file
                        cryptoBuilder.WithCredentialsFromFile(options.Credentials, options.Profile);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception caught while reading credentials:  {ex.Message}");
                        Console.WriteLine($"Check pathname of credentials file '{options.Credentials}' and chosen profile '{options.Profile}'");
                        return;
                    }
                }

                if (!string.IsNullOrEmpty(options.EncryptText))
                {
                    await EncryptionAsync(options, cryptoBuilder);
                }
                else
                {
                    await DecryptionAsync(options, cryptoBuilder);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e.Message}");
                Environment.Exit(1);
            }
        }

        private static async Task EncryptionAsync(CommandLineOptions options, CryptographyBuilder cryptoBuilder)
        {
            using var ubiqEncryptDecrypt = cryptoBuilder.BuildStructured();

            var cipherText = await ubiqEncryptDecrypt.EncryptAsync(options.FfsName, options.EncryptText);

            Console.WriteLine($"ENCRYPTED cipher= {cipherText}\n");

            return;
        }

        private static async Task DecryptionAsync(CommandLineOptions options, CryptographyBuilder cryptoBuilder)
        {
            using var ubiqEncryptDecrypt = cryptoBuilder.BuildStructured();

            var plainText = await ubiqEncryptDecrypt.DecryptAsync(options.FfsName, options.DecryptText);

            Console.WriteLine($"DECRYPTED plainText= {plainText}\n");

            return;
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
}
