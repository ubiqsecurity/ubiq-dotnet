using UbiqSecurity.Model;

namespace UbiqSecurity.Internals
{
	internal static class FfsRecordExtensions
	{
		public static int DecodeKeyNumber(this FfsRecord dataset, ref string text)
		{
			char charBuf = text[0];

			int encodedValue = dataset.OutputCharacters.IndexOf(charBuf);

			int keyNumber = encodedValue >> (int)dataset.MsbEncodingBits.Value;

			encodedValue -= keyNumber << (int)dataset.MsbEncodingBits.Value;

			char ch = dataset.OutputCharacters[encodedValue];

			text = text.ReplaceCharacter(ch, 0);

			return keyNumber;
		}

		public static string EncodeKeyNumber(this FfsRecord dataset, string text, int keyNumber)
		{
			var charBuf = text[0];

			var keyNumIndex = dataset.OutputCharacters.IndexOf(charBuf);

			var ctValue = keyNumIndex + (keyNumber << (int)dataset.MsbEncodingBits.Value);

			var ch = dataset.OutputCharacters[ctValue];

			var encodedText = text.ReplaceCharacter(ch, 0);

			return encodedText;
		}
	}
}
