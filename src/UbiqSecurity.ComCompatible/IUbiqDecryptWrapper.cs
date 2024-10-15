using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace UbiqSecurity.ComCompatible
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    [Guid("F214E44B-150B-4437-BEA8-B73C073348B5")]
    public interface IUbiqDecryptWrapper
    {
        void AddReportingUserDefinedMetadata(string jsonString);

        byte[] Decrypt(ref byte[] cipherBytes);

        string GetCopyOfUsage();

        byte[] Begin();

        byte[] Update(ref byte[] cipherBytes, int offset, int count);

        [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Keeping compatibility w/ existing Ubiq API")]
        byte[] End();
    }
}
