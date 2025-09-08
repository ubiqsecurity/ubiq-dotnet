using CommandLine;

namespace StructuredStrings
{
    internal class CommandLineOptions
    {
        [Option('e', "encrypttext", HelpText = "Set the field text value to encrypt and will return the encrypted cipher text")]
        public string EncryptText { get; set; }

        [Option('d', "decrypttext", HelpText = "Set the cipher text value to decrypt and will return the decrypted text")]
        public string DecryptText { get; set; }

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
