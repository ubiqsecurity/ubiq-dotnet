using System.Threading.Tasks;
using Microsoft.SqlServer.Server;

namespace UbiqSecurity.Mssql
{
    public class UbiqSql
    {
        private static IUbiqCredentials _ubiqCredentials = null;
        private static UbiqFPEEncryptDecrypt _ubiqFpeEncryptDecrypt = null;
        private static readonly object _lock = new object();

        [SqlProcedure]
        public static void Init(string apiKey, string secretSigningKey, string cryptoAccessKey)
        {
            lock(_lock)
            {
                _ubiqFpeEncryptDecrypt?.Dispose();

                _ubiqCredentials = UbiqFactory.CreateCredentials(apiKey, secretSigningKey, cryptoAccessKey);
                _ubiqFpeEncryptDecrypt = new UbiqFPEEncryptDecrypt(_ubiqCredentials);
            }
        }

        [SqlFunction]
        public static string Encrypt(string datasetName, string plainText)
        {
            if (_ubiqFpeEncryptDecrypt == null)
            {
                throw new System.Exception("Ubiq encrption not initialized, call init() first");
            }

            var task = Task.Run(async () => await _ubiqFpeEncryptDecrypt.EncryptAsync(datasetName, plainText));

            return task.Result;
        }

        [SqlFunction]
        public static string Decrypt(string datasetName, string cipherText)
        {
            if (_ubiqFpeEncryptDecrypt == null)
            {
                throw new System.Exception("Ubiq decrption not initialized, call init() first");
            }

            var task = Task.Run(async () => await _ubiqFpeEncryptDecrypt.DecryptAsync(datasetName, cipherText));

            return task.Result;
        }
    }
}
