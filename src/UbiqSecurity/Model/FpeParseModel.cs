using System;
using UbiqSecurity.Internals;

namespace UbiqSecurity.Model
{
	internal class FpeParseModel
	{
		public string Trimmed { get; set; }

		public string StringTemplate { get; set; }

        public string Prefix { get; set; }

        public string Suffix { get; set; }

        public bool PassthroughProcessed { get; set; }

        public char TemplateChar { get; set; }

        public int DecodeKeyNumber(FfsRecord dataset, int position)
		{
			if (position < 0 || position >= Trimmed.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(position), $"Invalid string decoding position: {position}");
			}

			char charBuf = Trimmed[position];
			
			int encodedValue = dataset.OutputCharacters.IndexOf(charBuf);

			int keyNumber = encodedValue >> (int)dataset.MsbEncodingBits.Value;

			char ch = dataset.OutputCharacters[encodedValue - (keyNumber << (int)dataset.MsbEncodingBits.Value)];

			Trimmed = Trimmed.ReplaceCharacter(ch, position);

			return keyNumber;
		}

		
	}
}
