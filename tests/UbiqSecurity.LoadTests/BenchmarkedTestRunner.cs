using System.Diagnostics;

namespace UbiqSecurity.LoadTests
{
    public class BenchmarkedTestRunner : TestRunner
    {
        private readonly Stopwatch _stopwatch = new();
        private readonly HashSet<string> _warmDatasets = new();

        public BenchmarkedTestRunner(UbiqStructuredEncryptDecrypt fpeEncryptDecrypt)
            :base(fpeEncryptDecrypt)
        {
        }

        protected override async Task OneIterationAsync(LoadTestData testData, Timings timings)
        {
            if (!_warmDatasets.Contains(testData.Dataset))
            {
                await WarmupIterationAsync(testData, timings);
            }

            // Give the test as good a chance as possible
            // of avoiding garbage collection
            //GC.Collect();
            //GC.WaitForPendingFinalizers();
            //GC.Collect();

            _stopwatch.Reset();
            _stopwatch.Start();
            await base.EncryptAsync(testData, timings);
            _stopwatch.Stop();
            timings.AddEncryptionTime(testData.Dataset, _stopwatch.Elapsed.TotalMilliseconds);

            _stopwatch.Reset();
            _stopwatch.Start();
            await base.DecryptAsync(testData, timings);
            _stopwatch.Stop();
            timings.DecryptionTimes.Add(_stopwatch.Elapsed.TotalMilliseconds);

        }

        public async Task WarmupIterationAsync(LoadTestData testData, Timings timings)
        {
            Console.WriteLine($"Warmup Iteration: {testData.Dataset}");

            await base.EncryptAsync(testData, timings);
            await base.DecryptAsync(testData, timings);

            _warmDatasets.Add(testData.Dataset);
        }
    }
}
