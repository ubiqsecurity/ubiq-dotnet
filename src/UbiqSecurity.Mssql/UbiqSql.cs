using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;

namespace UbiqSecurity.Mssql
{
    public class UbiqSql
    {
        private static readonly ConcurrentDictionary<string, UbiqStructuredEncryptDecrypt> EncryptDecryptObjects = new ConcurrentDictionary<string, UbiqStructuredEncryptDecrypt>();

        [SqlFunction(DataAccess = DataAccessKind.Read, SystemDataAccess = SystemDataAccessKind.Read)]
        public static string Encrypt(string datasetName, string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText;
            }

            var ubiqFpeEncryptDecrypt = GetEncryptDecrypt();

            var task = Task.Run(async () => await ubiqFpeEncryptDecrypt.EncryptAsync(datasetName, plainText));

            return task.Result;
        }

        [SqlFunction(DataAccess = DataAccessKind.Read, SystemDataAccess = SystemDataAccessKind.Read)]
        public static string Encrypt_Nop(string datasetName, string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText;
            }

            var ubiqFpeEncryptDecrypt = GetEncryptDecrypt();

            return plainText;
        }

        [SqlFunction(DataAccess = DataAccessKind.Read, SystemDataAccess = SystemDataAccessKind.Read)]
        public static string Encrypt_NoPipeline(string datasetName, string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return plainText;
            }

            var ubiqFpeEncryptDecrypt = GetEncryptDecrypt();

            var task = Task.Run(async () => await ubiqFpeEncryptDecrypt.NullEncryptAsync(datasetName, plainText));

            return task.Result;
        }

        [SqlFunction(DataAccess = DataAccessKind.Read, SystemDataAccess = SystemDataAccessKind.Read)]
        public static string Decrypt(string datasetName, string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return cipherText;
            }

            var ubiqFpeEncryptDecrypt = GetEncryptDecrypt();
            
            var task = Task.Run(async () => await ubiqFpeEncryptDecrypt.DecryptAsync(datasetName, cipherText));

            return task.Result;
        }

        [SqlFunction(DataAccess = DataAccessKind.Read, SystemDataAccess = SystemDataAccessKind.Read)]
        public static string Decrypt_Nop(string datasetName, string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return cipherText;
            }

            var ubiqFpeEncryptDecrypt = GetEncryptDecrypt();

            return cipherText;
        }

        [SqlFunction(DataAccess = DataAccessKind.Read, SystemDataAccess = SystemDataAccessKind.Read)]
        public static string Decrypt_NoPipeline(string datasetName, string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return cipherText;
            }

            var ubiqFpeEncryptDecrypt = GetEncryptDecrypt();

            var task = Task.Run(async () => await ubiqFpeEncryptDecrypt.NullDecryptAsync(datasetName, cipherText));

            return task.Result;
        }

        // find the directory that the ubiqsecurity.dll was import from
        private static string GetImportDirectory()
        {
            string dllImportPath = null;

            using (var conn = new SqlConnection("context connection=true"))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT TOP 1 name FROM sys.assembly_files WHERE name like '%ubiqsecurity.dll'", conn);

                dllImportPath = (string)cmd.ExecuteScalar();
            }

            return dllImportPath.Replace("ubiqsecurity.dll", "");
        }

        private static string GetTestIdentity()
        {
            using (var conn = new SqlConnection("context connection=true"))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("SELECT SUSER_SNAME()", conn);

                return (string)cmd.ExecuteScalar();
            }
        }

        private static UbiqStructuredEncryptDecrypt GetEncryptDecrypt()
        {
            var userName = GetTestIdentity();

            if (EncryptDecryptObjects.ContainsKey(userName) && EncryptDecryptObjects.TryGetValue(userName, out var result))
            {
                return result;
            }

            var currentDirectory = GetImportDirectory();

            var encryptDecrypt = CryptographyBuilder
                                                .Create()
                                                .WithCredentialsFromFile(Path.Combine(currentDirectory, "credentials"))
                                                .WithCredentials(x =>
                                                {
                                                    x.IdpUsername = userName;
                                                })
                                                .WithConfigFromFile(Path.Combine(currentDirectory, "config.json"))
                                                .WithConfig(x =>
                                                {
                                                    // x.EventReporting.Enabled = false;
                                                    x.Idp.SelfSignIdentity = userName;
                                                })
                                                .BuildStructured();

            EncryptDecryptObjects.TryAdd(userName, encryptDecrypt);

            return encryptDecrypt;
        }
    }
}
