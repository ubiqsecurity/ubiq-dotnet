using System;
using System.Security.Cryptography;

namespace UbiqSecurity.Fpe.Ciphers
{
	internal class SystemCipher : ICipher, IDisposable
	{
		private readonly Aes _cipher;

		public SystemCipher()
		{
            _cipher = Aes.Create();
            _cipher.KeySize = 128;
            _cipher.Padding = PaddingMode.None;
            _cipher.Mode = CipherMode.CBC;
		}

		public void Ciph(byte[] key, byte[] src, int srcOffset, byte[] dest, int destOffset)
		{
			Prf(key, src, srcOffset, dest, destOffset, 16);
		}

		public virtual void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Prf(byte[] key, byte[] src, byte[] dest)
		{
			Prf(key, src, 0, dest, 0, src.Length);
		}

		public void Prf(byte[] key, byte[] src, int srcOffset, byte[] dest, int destOffset, int length)
		{
			byte[] iv = { (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
					(byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
					(byte) 0x00, (byte) 0x00 };

			var enc = _cipher.CreateEncryptor(key, iv);

			for (int i = 0; i < length && i < (src.Length - srcOffset); i += enc.InputBlockSize)
			{
				enc.TransformBlock(src, i + srcOffset, enc.InputBlockSize, dest, destOffset);
			}
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_cipher?.Dispose();
			}
		}
	}
}
