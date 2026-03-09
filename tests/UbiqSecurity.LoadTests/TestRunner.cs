using System.Globalization;
using Newtonsoft.Json;

namespace UbiqSecurity.LoadTests
{
    public class TestRunner : IDisposable
    {
        private readonly UbiqStructuredEncryptDecrypt _fpeEncryptDecrypt = null;

        public TestRunner(UbiqStructuredEncryptDecrypt fpeEncryptDecrypt)
        {
            _fpeEncryptDecrypt = fpeEncryptDecrypt;
        }

        public virtual async Task<Timings> RunAsync(IEnumerable<string> jsonDataPaths)
        {
            Console.WriteLine("Starting");

            Timings timings = new();
            LoadTestData testData;

            try
            {
                foreach (var jsonDataPath in jsonDataPaths)
                {
                    Console.WriteLine($"Loading tests from {jsonDataPath}");

                    using var fileStream = new FileStream(jsonDataPath, FileMode.Open);
                    using var streamReader = new StreamReader(fileStream);
                    using var jsonReader = new JsonTextReader(streamReader);

                    var serializer = new JsonSerializer();

                    while (jsonReader.Read())
                    {
                        if (jsonReader.TokenType == JsonToken.StartObject)
                        {
                            testData = serializer.Deserialize<LoadTestData>(jsonReader);
                            await OneIterationAsync(testData, timings);
                        }
                    }

                    jsonReader.Close();
                    streamReader.Close();
                    fileStream.Close();
                }
            }
            catch (Exception ex)
            {
                timings.ErrorCount++;
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }

            return timings;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fpeEncryptDecrypt?.Dispose();
            }
        }

        protected virtual async Task OneIterationAsync(LoadTestData testData, Timings timings)
        {
            await EncryptAsync(testData, timings);
            await DecryptAsync(testData, timings);
        }

        protected virtual async Task EncryptAsync(LoadTestData testData, Timings timings)
        {
            var cipherText = await _fpeEncryptDecrypt.InferredEncryptAsync(testData.Dataset, testData.PlainText);
            if (cipherText != testData.CipherText)
            {
                timings.ErrorCount++;
                Console.WriteLine($"Error cipherText {testData.CipherText} expected, actual = {cipherText}, plainText = {testData.PlainText}");
            }
        }

        protected virtual async Task DecryptAsync(LoadTestData testData, Timings timings)
        {
            var plainText = await _fpeEncryptDecrypt.InferredDecryptAsync(testData.Dataset, testData.CipherText);
            if (plainText != testData.PlainText)
            {
                timings.ErrorCount++;
                Console.WriteLine($"Error plainText {testData.CipherText} expected");
            }
        }
    }
}
