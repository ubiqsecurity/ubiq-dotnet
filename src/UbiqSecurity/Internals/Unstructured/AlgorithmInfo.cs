using System;
using System.Linq;

namespace UbiqSecurity.Internals.Unstructured
{
    // TODO: test AES-128 mode via different API key.
    // Supports AES-GCM only!
    internal class AlgorithmInfo
    {
        private static readonly AlgorithmInfo[] SupportedAlgorithms = new AlgorithmInfo[]
        {
            new AlgorithmInfo(0, "AES-256-GCM", 32, 12, 16),
            new AlgorithmInfo(1, "AES-128-GCM", 16, 12, 16),
        };

        internal AlgorithmInfo(string name)
        {
            // note: throws if no matching name
            var match = SupportedAlgorithms.First(alg => alg.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            Id = match.Id;
            Name = match.Name;
            KeyLength = match.KeyLength;
            InitVectorLength = match.InitVectorLength;
            MacLength = match.MacLength;
        }

        internal AlgorithmInfo(byte id)
        {
            // note: throws if no matching Id
            var match = SupportedAlgorithms.First(alg => alg.Id == id);

            Id = match.Id;
            Name = match.Name;
            KeyLength = match.KeyLength;
            InitVectorLength = match.InitVectorLength;
            MacLength = match.MacLength;
        }

        private AlgorithmInfo(byte id, string name, int keyLength, int initVectorLength, int macLength)
        {
            Id = id;
            Name = name;
            KeyLength = keyLength;
            InitVectorLength = initVectorLength;
            MacLength = macLength;
        }

        internal byte Id { get; }

        internal string Name { get; }

        internal int KeyLength { get; } // in bytes

        internal int InitVectorLength { get; } // in bytes

        internal int MacLength { get; } // in bytes
    }
}
