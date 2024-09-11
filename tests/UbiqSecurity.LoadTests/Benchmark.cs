using System.Diagnostics;
using Newtonsoft.Json;

namespace UbiqSecurity.LoadTests
{
	public class Benchmark : IDisposable
	{
		private readonly Stopwatch _stopwatch = new();
		private readonly UbiqFPEEncryptDecrypt _fpeEncryptDecrypt = null;

		private bool _isWarm = false;
		private Timings _timings = null;

		public Benchmark(IUbiqCredentials credentials)
		{
			_fpeEncryptDecrypt = new UbiqFPEEncryptDecrypt(credentials);
		}

		public async Task<Timings> RunAsync(IEnumerable<string> jsonDataPaths)
		{
			Console.WriteLine("Starting");

			_timings = new Timings();

			try
			{
                foreach (var jsonDataPath in jsonDataPaths)
                {
				    using var fileStream = new FileStream(jsonDataPath, FileMode.Open);
				    using var streamReader = new StreamReader(fileStream);
				    using var jsonReader = new JsonTextReader(streamReader);

				    var serializer = new JsonSerializer();
				    LoadTestData testData = null;

				    while (jsonReader.Read())
				    {
					    if (jsonReader.TokenType == JsonToken.StartObject)
					    {
						    testData = serializer.Deserialize<LoadTestData>(jsonReader);

						    if (!_isWarm)
						    {
							    await WarmupIterationAsync(testData);
						    }

						    await OneIterationAsync(testData);
					    }
				    }

				    jsonReader.Close();
				    streamReader.Close();
				    fileStream.Close();
                }
            }
			catch(Exception ex)
			{
                _timings.ErrorCount++;
				Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
			}

			return _timings;
		}

		public async Task OneIterationAsync(LoadTestData testData)
		{
			// Give the test as good a chance as possible
			// of avoiding garbage collection
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			_stopwatch.Reset();
			_stopwatch.Start();

			var cipherText = await _fpeEncryptDecrypt.EncryptAsync(testData.Dataset, testData.PlainText);

			_stopwatch.Stop();
			//Console.WriteLine($"{testData.Dataset}: {_stopwatch.Elapsed.TotalMilliseconds} ms");
			_timings.AddEncryptionTime(testData.Dataset, _stopwatch.Elapsed.TotalMilliseconds);

			if (cipherText != testData.CipherText)
			{
                _timings.ErrorCount++;
                Console.WriteLine($"Error cipherText {testData.CipherText} expected");
			}

			_stopwatch.Reset();
			_stopwatch.Start();

			var plainText = await _fpeEncryptDecrypt.DecryptAsync(testData.Dataset, cipherText);

			_stopwatch.Stop();
			_timings.DecryptionTimes.Add(_stopwatch.Elapsed.TotalMilliseconds);

			if (plainText != testData.PlainText)
			{
                _timings.ErrorCount++;
                Console.WriteLine($"Error plainText {testData.CipherText} expected");
			}
		}

		public async Task WarmupIterationAsync(LoadTestData testData)
		{
			Console.WriteLine("Warmup Iteration");

			var cipherText = await _fpeEncryptDecrypt.EncryptAsync(testData.Dataset, testData.PlainText);

			if (cipherText != testData.CipherText)
			{
                _timings.ErrorCount++;
				Console.WriteLine($"Error cipherText {testData.CipherText} expected");
			}

			var plainText = await _fpeEncryptDecrypt.DecryptAsync(testData.Dataset, cipherText);

			if (plainText != testData.PlainText)
			{
                _timings.ErrorCount++;
                Console.WriteLine($"Error plainText {testData.CipherText} expected");
			}

			_isWarm = true;
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
	}
}
