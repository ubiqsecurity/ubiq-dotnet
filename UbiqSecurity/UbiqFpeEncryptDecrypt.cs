using System;
using System.IO;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UbiqSecurity.Constants;
using UbiqSecurity.Fpe;
using UbiqSecurity.Internals;
using UbiqSecurity.Model;

namespace UbiqSecurity
{
	public class UbiqFPEEncryptDecrypt : IDisposable
	{
		#region Private Data

		private string _fpe;
		private readonly IUbiqCredentials _ubiqCredentials;

		private bool _isInited;
		private int? _keyNumber;
		private FfsCacheManager _ffsCache;
		private FpeEncryptedKeyCacheManager _fpeCache;
		private FpeProcessor _fpeProcessor;
		private FpeTransactionManager _fpeTransactions;
		private UbiqWebServices _ubiqWebServices;       // null when disposed

		private const string _base2Characters = "01";
		private const int _ff1_base2_min_length = 20; // NIST requirement ceil(log(2(1000000)))

		#endregion

		#region Constructors

		public UbiqFPEEncryptDecrypt(IUbiqCredentials ubiqCredentials)
		{
			_isInited = false;
			_ubiqCredentials = ubiqCredentials;
			_ubiqWebServices = new UbiqWebServices(_ubiqCredentials);
			_ffsCache = new FfsCacheManager(_ubiqCredentials, _ubiqWebServices);
			_fpeCache = new FpeEncryptedKeyCacheManager(_ubiqCredentials, _ubiqWebServices);
			_fpeTransactions = new FpeTransactionManager(new UbiqWebServices(_ubiqCredentials));
			_fpeProcessor = new FpeProcessor(_fpeTransactions, 1);
			_fpeProcessor.StartUp();
		}

		#endregion

		#region IDisposable

		public virtual void Dispose()
		{
			if (_fpeProcessor != null)
			{
				_fpeProcessor.ShutDown();
			}

			if(_ffsCache != null)
			{
				_ffsCache.Dispose();
				_ffsCache = null;
			}

			if (_fpeCache != null)
			{
				_fpeCache.Dispose();
				_fpeCache = null;
			}

			if(_fpeTransactions != null)
			{
				_fpeTransactions.Dispose();
				_fpeTransactions = null;
			}

			if (_ubiqWebServices != null)
			{
				_ubiqWebServices.Dispose();
				_ubiqWebServices = null;
			}
		}

		#endregion

		#region Public Methods

		public string FfsName { get; private set; }

		public void ClearCache()
		{
			if (_ffsCache != null)
			{
				_ffsCache.Clear();
			}

			if (_fpeCache != null)
			{
				_fpeCache.Clear();
			}
		}

		public async Task<byte[]> DecryptAsync(string ffsName, byte[] plainBytes, byte[] tweak)
		{
			var input = Encoding.ASCII.GetString(plainBytes);

			var result = await DecryptAsync(ffsName, input, tweak);

			return Encoding.ASCII.GetBytes(result);
		}

		public async Task<string> DecryptAsync(string ffsName, string plainText, byte[] tweak)
		{
			await InitAsync(ffsName);
			var ffs = await GetFfsConfigurationAsync();
			var firstNonPassthrough = FindFirstIndexExclusive(plainText, ffs.Passthrough);
			if(firstNonPassthrough < 0)
			{
				firstNonPassthrough = 0;
			}

			var encodedValueKeyIndex = DecodeKeyNumIndex(ffs, plainText, firstNonPassthrough);
			var msb_encoding_bits = ffs.MsbEncodingBits;
			var keyNum = encodedValueKeyIndex >> (int)msb_encoding_bits.Value;

			var ch = ffs.OutputCharacters.Substring(encodedValueKeyIndex - (keyNum << (int)msb_encoding_bits.Value));

			var strChars = plainText.ToCharArray();
			strChars[firstNonPassthrough] = ch.ToCharArray()[0];

			plainText = new string(strChars);
			_keyNumber = keyNum;

			return await CipherAsync(ffsName, plainText, tweak, false);
		}

		public async Task<byte[]> EncryptAsync(string ffsName, byte[] plainBytes, byte[] tweak)
		{
			var input = Encoding.ASCII.GetString(plainBytes);
			var result = await EncryptAsync(ffsName, input, tweak);
			return Encoding.ASCII.GetBytes(result);

		}

		public async Task<string> EncryptAsync(string ffsName, string plainText, byte[] tweak)
		{
			await InitAsync(ffsName);
			var result = await CipherAsync(ffsName, plainText, tweak, true);
			var ffs = await GetFfsConfigurationAsync();
			var fpe = await GetFpeEncryptionKeyAsync();

			var firstNonPassthrough = FindFirstIndexExclusive(result, ffs.Passthrough);

			result = EncodeKeyNum(ffs, fpe.KeyNumber, result, firstNonPassthrough);

			return result;
		}

		#endregion

		#region Private Methods

		// convert to output radix
		private async Task<string> CipherAsync(string ffsName, string inputText, byte[] tweak, bool encrypt)
		{
			if (!_isInited)
			{
				throw new InvalidOperationException("object closed");
			}

			var ffs = await GetFfsConfigurationAsync();
			var inputCharacterSet = encrypt ? ffs.InputCharacters : ffs.OutputCharacters;
			var outputCharacterSet = encrypt ? ffs.OutputCharacters : ffs.InputCharacters;

			var parsedInput = ParseInput(inputText, ffs, encrypt);

			var encryptionKey = await GetFpeEncryptionKeyAsync();

			var rt = ConvertToOutputRadix(inputCharacterSet, _base2Characters, parsedInput.Trimmed);

			double padlen = Math.Ceiling(
				(double)Math.Max(
					_ff1_base2_min_length, 
					Log2(ffs.InputCharacters.Length) * parsedInput.Trimmed.Length
					)
				);

			rt = PadText(rt, padlen);
			if (ffs.TweakSource == TweakSourceConstants.Constant)
			{
				tweak = Convert.FromBase64String(ffs.Tweak);
			}

			var cipher = GetCipher(tweak, ffs, encryptionKey, _base2Characters.Length);

			string outputData;

			if (encrypt)
			{
				outputData = cipher.Encrypt(rt, tweak);
			}
			else
			{
				outputData = cipher.Decrypt(rt, tweak);
			}

			rt = ConvertToOutputRadix(_base2Characters, outputCharacterSet, outputData);
			var result = MergeToFormattedOutput(rt, parsedInput, outputCharacterSet.Substring(0, 1));

			// create the billing record
			var uuid = Guid.NewGuid().ToString();
			var timestamp = DateTime.UtcNow;
			_fpeTransactions.CreateBillableItem(uuid, encrypt ? ActionConstants.Encrypt : ActionConstants.Decrypt, ffs.Name, timestamp, 1);

			return result;
		}

		private string ConvertToOutputRadix(string inputCharacterSet, string outputCharacterSet, string rawtext)
		{
			// convert a given string to a numerical location based on a given Input_character_set
			BigInteger r1 = Bn.__bigint_set_str(rawtext, inputCharacterSet);

			// convert a numerical location code to a string based on its location in an Output_character_set
			string output = Bn.__bigint_get_str(outputCharacterSet, r1);
			return output;
		}

		private int DecodeKeyNumIndex(FfsConfigurationResponse ffs, string str, int position)
		{
			if (position < 0)
			{
				// throw new InvalidOperationException($"Bad String decoding position for: {str}");
				position = 0;
			}

			var charBuf = str.Substring(position, 1);

			int encodedValue = ffs.OutputCharacters.IndexOf(charBuf);
			return encodedValue;
		}

		private string EncodeKeyNum(FfsConfigurationResponse ffs, int keyNumber, string str, int position)
		{
			string buf;

			if (position < 0)
			{
				throw new InvalidOperationException($"Bad String decoding position for: {str}");
			}

			var strChars = str.ToCharArray();
			var charBuf = strChars[position];

			var ct_value = ffs.OutputCharacters.IndexOf(charBuf);
			var msb_encoding_bits = ffs.MsbEncodingBits;

			ct_value = ct_value + (keyNumber << (int)msb_encoding_bits.Value);

			var ch = ffs.OutputCharacters.Substring(ct_value, 1);
			strChars[position] = ch.ToCharArray()[0];

			buf = new string(strChars);
			return buf;
		}

		private int FindFirstIndexExclusive(string str, string searchChars)
		{
			if (string.IsNullOrEmpty(str))
			{
				return -1;
			}

			var strArray = str.ToCharArray();
			for (int i = 0; i < strArray.Length; i++)
			{
				if (searchChars.IndexOf(strArray[i]) < 0)
				{
					return i;
				}
			}
			return -1;
		}

		private IFFX GetCipher(byte[] tweak, FfsConfigurationResponse ffs, FpeEncryptionKeyResponse encryptionKey, int radix)
		{
			IFFX cypher;

			// set the tweak range and radix based on the FFS record
			long twkmin = ffs.TweakMinLength.HasValue ? ffs.TweakMinLength.Value : 0;
			long twkmax = ffs.TweakMaxLength.HasValue ? ffs.TweakMaxLength.Value : 0;

			_fpe = ffs.EncryptionAlgorithm;
			switch (_fpe.ToLower())
			{
				case FpeNameConstants.ff1:
					cypher = new FF1(encryptionKey.UnwrappedDataKey, tweak, twkmin, twkmax, radix);
					break;
				case FpeNameConstants.ff3_1:
					cypher = new FF3_1(encryptionKey.UnwrappedDataKey, tweak, radix);
					break;
				default:
					throw new InvalidOperationException($"Unknown FPE Algorithm: {_fpe}");
			}

			return cypher;
		}

		private async Task<FfsConfigurationResponse> GetFfsConfigurationAsync()
		{
			return await _ffsCache.GetAsync(new CacheKey { FfsName = FfsName });
		}

		private async Task<FpeEncryptionKeyResponse> GetFpeEncryptionKeyAsync()
		{
			return await _fpeCache.GetAsync(new CacheKey { FfsName = FfsName, KeyNumber = _keyNumber });
		}

		private async Task InitAsync(string ffsName)
		{
			if (string.IsNullOrEmpty(ffsName))
			{
				throw new ArgumentNullException("missing ffs_name");
			}

			FfsName = ffsName;

			if (_ubiqWebServices == null)
			{
				throw new ObjectDisposedException(GetType().Name);
			}

			if (await GetFfsConfigurationAsync() == null)
			{
				throw new InvalidOperationException("invalid FFS Configuration");
			}

			if (await GetFpeEncryptionKeyAsync() == null)
			{
				throw new InvalidOperationException("invalid or missing key");
			}

			_isInited = true;
		}

		/// <summary>
		/// Merges the given string into the  "formatted_dest" pattern using the 
		/// set of provided characters. 
		/// </summary>
		/// <param name="inputString"></param>
		/// <param name="parsedInput"></param>
		/// <param name="padCharacter"></param>
		/// <returns></returns>
		private string MergeToFormattedOutput(string inputString, FpeParseModel parsedInput, string padCharacter)
		{
			var ptChars = parsedInput.StringTemplate.ToCharArray();
			var inputChars = inputString.ToCharArray();

			var d = ptChars.Length - 1;
			var s = inputString.Length - 1;
			var padChar = padCharacter.ToCharArray()[0];

			while (s >= 0 && d >= 0)
			{
				// Find the first available destination character
				while (d >= 0 && ptChars[d] != padChar)
				{
					d--;
				}

				// Copy the encrypted text into the formatted output string
				if (d >= 0)
				{
					ptChars[d] = inputChars[s];
				}
				s--;
				d--;
			}

			return new string(ptChars);
		}

		private double Log2 (int x)
		{
			return (double)(Math.Log(x) / Math.Log(2));
		}

		private string RegexEscape(string patternCharacters)
		{
			var returnSet = patternCharacters;
			var bracketIndex = returnSet.IndexOf("]");
			if (bracketIndex > -1)
			{
				returnSet = $"{returnSet.Substring(bracketIndex, 1)}{returnSet.Remove(bracketIndex, 1)}";
			}

			return Regex.Escape(returnSet).Replace("-", "\\-");
		}

		/// <summary>
		/// Pads a given string with 0 characters at least as long as specified length
		/// </summary>
		/// <param name="inputString">The original string</param>
		/// <param name="length">The desired length of the new string</param>
		/// <returns>the padded string</returns>
		private string PadText(string inputString, double length)
		{
			if (inputString.Length >= length)
			{
				return inputString;
			}

			var sb = new StringBuilder();
			while (sb.Length < length - inputString.Length)
			{
				sb.Append('0');
			}
			sb.Append(inputString);

			return sb.ToString();
		}

		private FpeParseModel ParseInput(string input, FfsConfigurationResponse ffs, bool encrypt)
		{
			var inputRgxPattern = $"[{RegexEscape(ffs.InputCharacters)}]";
			var firstCharacter = ffs.OutputCharacters.Substring(0, 1);
			var invalidRgxPattern = $"[{RegexEscape(ffs.InputCharacters)}{RegexEscape(ffs.Passthrough)}]";
			if (!encrypt)
			{
				inputRgxPattern = $"[{RegexEscape(ffs.OutputCharacters)}]";
				firstCharacter = ffs.InputCharacters.Substring(0, 1);
				invalidRgxPattern = $"[{RegexEscape(ffs.OutputCharacters)}{RegexEscape(ffs.Passthrough)}]";
			}

			var inputRgx = new Regex(inputRgxPattern);
			var inputMatches = inputRgx.Matches(input);

			var formattedPassthroughCharacters = Regex.Replace(input, inputRgxPattern, firstCharacter);

			if (encrypt)
			{
				var invalidRgx = new Regex(invalidRgxPattern);
				MatchCollection invalidMatch = invalidRgx.Matches(input);
				if (invalidMatch.Count != input.Length)
				{
					throw new ArgumentOutOfRangeException("invalid character found in the input");
				}
			}

			var inputSb = new StringBuilder();
			foreach (Match group in inputMatches)
			{
				inputSb.Append(group.Value);
			}

			var result = new FpeParseModel
			{
				StringTemplate = formattedPassthroughCharacters,
				Trimmed = inputSb.ToString(),
			};

			return result;
		}

		#endregion

		#region Static Methods

		public static async Task<string> DecryptAsync(IUbiqCredentials ubiqCredentials, string inputText, string ffsName, byte[] tweak)
		{
			var response = string.Empty;
			using (var ubiqEncrypt = new UbiqFPEEncryptDecrypt(ubiqCredentials))
			{
				response = await ubiqEncrypt.DecryptAsync(ffsName, inputText, tweak);
			}

			return response;
		}

		public static async Task<string> EncryptAsync(IUbiqCredentials ubiqCredentials, string inputText, string ffsName, byte[] tweak)
		{
			var response = string.Empty;
			using (var ubiqEncrypt = new UbiqFPEEncryptDecrypt(ubiqCredentials))
			{
				response = await ubiqEncrypt.EncryptAsync(ffsName, inputText, tweak);
			}

			return response;
		}

		#endregion
	}
}
