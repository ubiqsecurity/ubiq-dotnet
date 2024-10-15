using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace UbiqSecurity.ComCompatible
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("BE381CB9-1C1C-4DD7-81C6-CFB3434C0456")]
    public class UbiqFpeEncryptDecryptWrapper : IDisposable, IUbiqFpeEncryptDecryptWrapper
    {
        private readonly UbiqFPEEncryptDecrypt _inner;

        public UbiqFpeEncryptDecryptWrapper(IUbiqCredentials ubiqCredentials, UbiqConfiguration ubiqConfiguration = null)
        {
            _inner = new UbiqFPEEncryptDecrypt(ubiqCredentials, ubiqConfiguration ?? new UbiqConfiguration());
        }

        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void ClearCache() => _inner.ClearCache();

        public void AddReportingUserDefinedMetadata(string jsonString)
        {
            _inner.AddReportingUserDefinedMetadata(jsonString);
        }

        public byte[] DecryptBytes(string datasetName, ref byte[] cipherBytes)
        {
            var tempCipherBytes = cipherBytes;

            var task = Task.Run(async () => await _inner.DecryptAsync(datasetName, tempCipherBytes));

            return task.Result;
        }

        public byte[] DecryptBytesWithTweak(string datasetName, ref byte[] cipherBytes, ref byte[] tweak)
        {
            var tempCipherBytes = cipherBytes;
            var tempTweak = tweak;

            var task = Task.Run(async () => await _inner.DecryptAsync(datasetName, tempCipherBytes, tempTweak));

            return task.Result;
        }

        public string DecryptString(string datasetName, string cipherText)
        {
            var task = Task.Run(async () => await _inner.DecryptAsync(datasetName, cipherText));
                
            return task.Result;
        }

        public string DecryptStringWithTweak(string datasetName, string cipherText, ref byte[] tweak)
        {
            var tempTweak = tweak;

            var task = Task.Run(async () => await _inner.DecryptAsync(datasetName, cipherText, tempTweak));

            return task.Result;
        }

        public byte[] EncryptBytes(string datasetName, ref byte[] plainBytes)
        {
            var tempPlainBytes = plainBytes;

            var task = Task.Run(async () => await _inner.EncryptAsync(datasetName, tempPlainBytes));

            return task.Result;
        }

        public byte[] EncryptBytesWithTweak(string datasetName, ref byte[] plainBytes, ref byte[] tweak)
        {
            var tempPlainBytes = plainBytes;
            var tempTweak = tweak;

            var task = Task.Run(async () => await _inner.EncryptAsync(datasetName, tempPlainBytes, tempTweak));

            return task.Result;
        }

        public string EncryptString(string datasetName, string plainText)
        {
            var task = Task.Run(async () => await _inner.EncryptAsync(datasetName, plainText));

            return task.Result;
        }

        public string EncryptStringWithTweak(string datasetName, string plainText, ref byte[] tweak)
        {
            var tempTweak = tweak;

            var task = Task.Run(async () => await _inner.EncryptAsync(datasetName, plainText, tempTweak));

            return task.Result;
        }

        public string[] EncryptForSearch(string datasetName, string plainText)
        {
            var task = Task.Run(async () => await _inner.EncryptForSearchAsync(datasetName, plainText));

            return task.Result?.ToArray();
        }

        public string[] EncryptForSearchWithTweak(string datasetName, string plainText, ref byte[] tweak)
        {
            var tempTweak = tweak;

            var task = Task.Run(async () => await _inner.EncryptForSearchAsync(datasetName, plainText, tempTweak));

            return task.Result?.ToArray();
        }

        public string GetCopyOfUsage() => _inner.GetCopyOfUsage();

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner?.Dispose();
            }
        }
    }
}
