using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace UbiqSecurity.ComCompatible
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    [Guid("B2AFA8AF-180E-437A-A023-185FA54CF56D")]
    public interface IUbiqEncryptWrapper
    {
        void AddReportingUserDefinedMetadata(string jsonString);

        byte[] Encrypt(ref byte[] plainBytes);

        string GetCopyOfUsage();

        byte[] Begin();

        byte[] Update(ref byte[] plainBytes, int offset, int count);

        [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Keeping compatibility w/ existing Ubiq API")]
        byte[] End();
    }
}
