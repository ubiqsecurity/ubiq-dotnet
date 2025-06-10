namespace UbiqSecurity.Tests
{
	public class UbiqConfigurationTests
	{
		[Fact]
		public void Constructor_JavaHalfDays_TimestampGranularitySetToHalfDays()
		{
            var sut = new UbiqConfiguration();
            sut.EventReporting.TimestampGranularity = "HALF_DAYS";

			Assert.Equal(ChronoUnit.HalfDays, sut.EventReporting.ChronoTimestampGranularity);
		}

		[Fact]
		public void Constructor_JavaMillis_TimestampGranularitySetToMilliseconds()
		{
            var sut = new UbiqConfiguration();
            sut.EventReporting.TimestampGranularity = "MILLIS";

			Assert.Equal(ChronoUnit.Milliseconds, sut.EventReporting.ChronoTimestampGranularity);
		}

		[Fact]
		public void Constructor_Milliseconds_TimestampGranularitySetToMilliseconds()
		{
            var sut = new UbiqConfiguration();
            sut.EventReporting.TimestampGranularity = "Milliseconds";

			Assert.Equal(ChronoUnit.Milliseconds, sut.EventReporting.ChronoTimestampGranularity);
		}
	}
}
