using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace UbiqSecurity.ComCompatible
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("020732E7-3420-4A68-9A09-BAB3D0BE984C")]
    public class UbiqDecryptWrapper : IUbiqDecryptWrapper, IDisposable
    {
        private readonly UbiqDecrypt _decryptor;

        public UbiqDecryptWrapper(IUbiqCredentials ubiqCredentials, UbiqConfiguration ubiqConfiguration = null)
        {
            _decryptor = new UbiqDecrypt(ubiqCredentials, ubiqConfiguration ?? new UbiqConfiguration());
        }

        public byte[] Decrypt(ref byte[] cipherBytes)
        {
            var tempCipherBytes = cipherBytes;

            var task = Task.Run(async () => await _decryptor.DecryptAsync(tempCipherBytes));

            return task.Result;
        }

        public byte[] Begin()
        {
            return _decryptor.Begin();
        }

        public byte[] Update(ref byte[] cipherBytes, int offset, int count)
        {
            var tempCipherBytes = cipherBytes;

            var task = Task.Run(async () => await _decryptor.UpdateAsync(tempCipherBytes, offset, count));

            return task.Result;
        }

        public byte[] End()
        {
            return _decryptor.End();
        }

        public void AddReportingUserDefinedMetadata(string jsonString)
        {
            _decryptor.AddReportingUserDefinedMetadata(jsonString);
        }

        public string GetCopyOfUsage()
        {
            return _decryptor.GetCopyOfUsage();
        }

        public void Dispose()
        {
            _decryptor?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
