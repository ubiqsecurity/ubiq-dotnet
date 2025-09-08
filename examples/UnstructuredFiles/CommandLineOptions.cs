using CommandLine;

namespace UnstructuredFiles
{
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