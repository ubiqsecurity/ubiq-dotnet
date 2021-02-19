using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UbiqSecurity.Internals;
using UbiqSecurity.Model;

namespace UbiqSecurity
{
    public class UbiqDecrypt : IDisposable
    {
        #region Private Data
        private UbiqWebServices _ubiqWebServices;   // null on dispose

        private CipherHeader _cipherHeader;         // extracted from beginning of ciphertext
        private ByteBuffer _byteBuffer;
        private DecryptionKeyResponse _decryptionKey;
        private AesGcmBlockCipher _aesGcmBlockCipher;
        #endregion

        #region Constructor
        public UbiqDecrypt(IUbiqCredentials ubiqCredentials)
        {
            _ubiqWebServices = new UbiqWebServices(ubiqCredentials);
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            Debug.WriteLine($"{GetType().Name}.{nameof(Dispose)}");

            if (_ubiqWebServices != null)
            {
                // reports decryption key usage to server, if applicable
                ResetAsync().Wait();        

                _ubiqWebServices.Dispose();
                _ubiqWebServices = null;
            }
        }
        #endregion

        #region Methods
        public byte[] Begin()
        {
            if (_ubiqWebServices == null)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
            else if (_aesGcmBlockCipher != null)
            {
                throw new InvalidOperationException("decryption in progress");
            }

            // prepare to receive initial header bytes
            _cipherHeader = null;
            _byteBuffer = null;

            // note: cached '_decryptionKey' may be present from a previous decryption run

            return new byte[0];
        }

        // each encryption has a header on it that identifies the algorithm
        // used and an encryption of the data key that was used to encrypt
        // the original plain text. there is no guarantee how much of that
        // data will be passed to this function or how many times this
        // function will be called to process all of the data. to that end,
        // this function buffers data internally, when it is unable to
        // process it.
        
        // the function buffers data internally until the entire header is
        // received. once the header has been received, the encrypted data
        // key is sent to the server for decryption. after the header has
        // been successfully handled, this function always decrypts all of
        // the data in its internal buffer
        public async Task<byte[]> UpdateAsync(byte[] cipherBytes, int offset, int count)
        {
            if (_ubiqWebServices == null)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            byte[] plainBytes = new byte[0];        // returned

            if (_byteBuffer == null)
            {
                _byteBuffer = new ByteBuffer();
            }

            // make sure new data is appended to end
            _byteBuffer.Enqueue(cipherBytes, offset, count);

            if (_cipherHeader == null)
            {
                // see if we've got enough data for the header record
                using (var byteStream = new MemoryStream(_byteBuffer.Peek()))
                {
                    _cipherHeader = CipherHeader.Deserialize(byteStream);
                }

                if (_cipherHeader != null)
                {
                    // success: prune cipher header bytes from the buffer
                    _byteBuffer.Dequeue(_cipherHeader.Length());

                    if (_decryptionKey != null)
                    {
                        // See if we can reuse the key from a previous decryption, meaning
                        // the new data was encrypted with the same key as the old data - i.e.
                        // both cipher headers have the same key.
                        //
                        // If not, clear the previous decryption key.
                        if (!_cipherHeader.EncryptedDataKeyBytes.SequenceEqual(
                            _decryptionKey.LastCipherHeaderEncryptedDataKeyBytes))
                        {
                            await ResetAsync().ConfigureAwait(false);
                            Debug.Assert(_decryptionKey == null);
                        }
                    }

                    // If needed, use the header info to fetch the decryption key.
                    if (_decryptionKey == null)
                    {
                        // JIT: request encryption key from server
                        _decryptionKey = await _ubiqWebServices.GetDecryptionKeyAsync(_cipherHeader.EncryptedDataKeyBytes).ConfigureAwait(false);
                    }

                    if (_decryptionKey != null)
                    {
                        var algorithmInfo = new AlgorithmInfo(_cipherHeader.AlgorithmId);

                        // save key extracted from header to detect future key changes
                        _decryptionKey.LastCipherHeaderEncryptedDataKeyBytes = _cipherHeader.EncryptedDataKeyBytes;

                        // create decryptor from header-specified algorithm + server-supplied decryption key
                        _aesGcmBlockCipher = new AesGcmBlockCipher(forEncryption: false,
                            algorithmInfo: algorithmInfo,
                            key: _decryptionKey.UnwrappedDataKey,
                            initVector: _cipherHeader.InitVectorBytes,
                            additionalBytes: ((_cipherHeader.Flags & CipherHeader.FLAGS_AAD_ENABLED) != 0)
                                ? _cipherHeader.Serialize()
                                : null);

                        _decryptionKey.KeyUseCount++;
                    }
                }
                else
                {
                    // holding pattern... need more header bytes
                    return plainBytes;
                }
            }

            // If we get this far, assume we have a valid header record.
            Debug.Assert(_cipherHeader != null);

            if ((_decryptionKey != null) && (_aesGcmBlockCipher != null))
            {
                // pass all available buffered bytes to the decryptor
                if (_byteBuffer.Length > 0)
                {
                    // tricky: the block cipher object will process all provided ciphertext 
                    // (including the trailing MAC signature), but may only return a subset of that
                    // as plaintext
                    var bufferedBytes = _byteBuffer.Dequeue(_byteBuffer.Length);
                    plainBytes = _aesGcmBlockCipher.Update(bufferedBytes, 0, bufferedBytes.Length);
                }
            }

            return plainBytes;
        }

        public byte[] End()
        {
            if (_ubiqWebServices == null)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            var finalPlainBytes = _aesGcmBlockCipher.Finalize();
            _aesGcmBlockCipher = null;
            _byteBuffer = null;
            return finalPlainBytes;
        }

        public static async Task<byte[]> DecryptAsync(IUbiqCredentials ubiqCredentials, byte[] data)
        {
            // handy inline function
            void WriteBytesToStream(Stream stream, byte[] bytes)
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            using (var memoryStream = new MemoryStream())
            {
                using (var ubiqDecrypt = new UbiqDecrypt(ubiqCredentials))
                {
                    WriteBytesToStream(memoryStream, ubiqDecrypt.Begin());
                    WriteBytesToStream(memoryStream, await ubiqDecrypt.UpdateAsync(data, 0, data.Length).ConfigureAwait(false));
                    WriteBytesToStream(memoryStream, ubiqDecrypt.End());
                }

                return memoryStream.ToArray();
            }
        }
        #endregion

        #region Private Methods
        // Reset the internal state of the decryption object.
        // This function can be called at any time to abort an existing
        // decryption operation.  It is also called by internal functions
        // when a new decryption requires a different key than the one
        // used by the previous decryption.
        private async Task ResetAsync()
        {
            Debug.Assert(_ubiqWebServices != null);

            if (_decryptionKey != null)
            {
                if (_decryptionKey.KeyUseCount > 0)
                {
                    // report key usage to server
                    Debug.WriteLine($"{GetType().Name}.{nameof(ResetAsync)}: reporting key count: {_decryptionKey.KeyUseCount}");
                    await _ubiqWebServices.UpdateDecryptionKeyUsageAsync(_decryptionKey.KeyUseCount,
                        _decryptionKey.KeyFingerprint, _decryptionKey.EncryptionSession).ConfigureAwait(false);
                }

                _decryptionKey = null;
            }

            _aesGcmBlockCipher = null;
        }
        #endregion
    }
}
