using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace UbiqSecurity.ComCompatible
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("0C75565B-DD1A-4D40-B27E-6892DE895ADE")]
    public class UbiqEncryptWrapper : IUbiqEncryptWrapper, IDisposable
    {
        private readonly UbiqEncrypt _encryptor;

        public UbiqEncryptWrapper(IUbiqCredentials ubiqCredentials, int usesRequested = 1, UbiqConfiguration ubiqConfiguration = null)
        {
            _encryptor = new UbiqEncrypt(ubiqCredentials, usesRequested, ubiqConfiguration ?? new UbiqConfiguration());
        }

        public byte[] Encrypt(ref byte[] plainBytes)
        {
            // plainBytes is a ref parameter because COM/VB6 doesn't support passing arrays by value
            // however ref parameters  are not allowed to be used in lambda expressions (like the call below)
            // so we make a temporary copy
            var tempBytes = plainBytes;

            var task = Task.Run(async () => await _encryptor.EncryptAsync(tempBytes));

            return task.Result;
        }

        public byte[] Begin()
        {
            var task = Task.Run(async () => await _encryptor.BeginAsync());

            return task.Result;
        }

        public byte[] Update(ref byte[] plainBytes, int offset, int count)
        {
            return _encryptor.Update(plainBytes, offset, count);
        }

        public byte[] End()
        {
            return _encryptor.End();
        }

        public void AddReportingUserDefinedMetadata(string jsonString)
        {
            _encryptor.AddReportingUserDefinedMetadata(jsonString);
        }

        public string GetCopyOfUsage()
        {
            return _encryptor.GetCopyOfUsage();
        }

        public void Dispose()
        {
            _encryptor?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
