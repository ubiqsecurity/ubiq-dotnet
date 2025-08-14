using System.Collections.Concurrent;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;

namespace UbiqSecurity.Mssql
{
    public class UbiqSql
    {
        private static readonly ConcurrentDictionary<string, UbiqStructuredEncryptDecrypt> EncryptDecryptObjects = new ConcurrentDictionary<string, UbiqStructuredEncryptDecrypt>();

        [SqlFunction(DataAccess = DataAccessKind.Read)]
        public static string Encrypt(string datasetName, string plainText)
        {
            var ubiqFpeEncryptDecrypt = GetEncryptDecrypt();

            var task = Task.Run(async () => await ubiqFpeEncryptDecrypt.EncryptAsync(datasetName, plainText));

            return task.Result;
        }

        [SqlFunction(DataAccess = DataAccessKind.Read)]
        public static string Decrypt(string datasetName, string cipherText)
        {
            var ubiqFpeEncryptDecrypt = GetEncryptDecrypt();
            
            var task = Task.Run(async () => await ubiqFpeEncryptDecrypt.DecryptAsync(datasetName, cipherText));

            return task.Result;
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

            // TODO: get private key from somewhere safe, windows cred manager? disk?
            // TODO: get customer id from somewhere
            var encryptDecrypt = CryptographyBuilder
                                                .Create()
                                                .WithCredentials(UbiqFactory.CreateIdpCredentials(userName, userName, "https://api-dev.ubiqsecurity.com"))
                                                .WithConfig(x =>
                                                {
                                                    x.Idp = new Config.IdpConfig()
                                                    {
                                                        UbiqCustomerId = "48309b5a-1ae8-4ade-9afc-7a74a9af6d48",
                                                        Provider = Config.IdpProvider.SelfSigned,
                                                        SelfSignIdentity = userName,
                                                        SelfSignKey = "-----BEGIN RSA PRIVATE KEY-----\r\n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\r\n-----END RSA PRIVATE KEY-----",
                                                    };
                                                })
                                                .BuildStructured();

            EncryptDecryptObjects.TryAdd(userName, encryptDecrypt);

            return encryptDecrypt;
        }
    }
}
