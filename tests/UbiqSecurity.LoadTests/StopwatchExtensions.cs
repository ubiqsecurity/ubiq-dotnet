using System.Diagnostics;

namespace UbiqSecurity.LoadTests
{
	public static class StopwatchExtensions
	{
		public static double ElapsedNanoseconds(this Stopwatch stopwatch)
		{
			return stopwatch.Elapsed.Ticks / Stopwatch.Frequency * 1000000000;
		}
	}
}
