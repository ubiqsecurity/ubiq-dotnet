using System.Runtime.InteropServices;

namespace UbiqSecurity.ComCompatible
{
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    [Guid("055DB9B7-4D2A-47FD-8528-D6BF4ECA0311")]
    public interface IUbiqFpeEncryptDecryptWrapper
    {
        void AddReportingUserDefinedMetadata(string jsonString);

        void ClearCache();

        byte[] DecryptBytes(string datasetName, ref byte[] cipherBytes);

        byte[] DecryptBytesWithTweak(string datasetName, ref byte[] cipherBytes, ref byte[] tweak);

        string DecryptString(string datasetName, string cipherText);

        string DecryptStringWithTweak(string datasetName, string cipherText, ref byte[] tweak);

        void Dispose();

        byte[] EncryptBytes(string datasetName, ref byte[] plainBytes);

        byte[] EncryptBytesWithTweak(string datasetName, ref byte[] plainBytes, ref byte[] tweak);

        string EncryptString(string datasetName, string plainText);

        string EncryptStringWithTweak(string datasetName, string plainText, ref byte[] tweak);

        string[] EncryptForSearch(string datasetName, string plainText);

        string[] EncryptForSearchWithTweak(string datasetName, string plainText, ref byte[] tweak);

        string GetCopyOfUsage();
    }
}
