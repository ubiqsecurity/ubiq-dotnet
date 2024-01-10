using System;
using UbiqSecurity.Fpe;

namespace UbiqSecurity.Internals
{
	internal class FfxContext
	{
		internal const string FF1AlgorithmName = "FF1";

		internal int KeyNumber { get; set; }

		internal FF1 FF1 {get; private set; }

		internal void SetFF1(FF1 ff1, int keyNumber)
		{
			FF1 = ff1;
			KeyNumber = keyNumber;
		}

		public string Cipher(string algorithm, string plainText, byte[] tweak, bool encrypt)
		{
			switch (algorithm.ToUpperInvariant())
			{
				case FF1AlgorithmName:
					return FF1.Cipher(plainText, tweak, encrypt);
				default:
					throw new ArgumentException($"Algorithm {algorithm} not supported", nameof(algorithm));
			}
		}
	}
}
