namespace UbiqSecurity.Tests
{
	public class UbiqConfigurationTests
	{
		[Fact]
		public void Constructor_JavaHalfDays_TimestampGranularitySetToHalfDays()
		{
			var sut = new UbiqConfiguration(0, 0, 0, false, "HALF_DAYS");

			Assert.Equal(ChronoUnit.HalfDays, sut.EventReportingTimestampGranularity);
		}

		[Fact]
		public void Constructor_JavaMillis_TimestampGranularitySetToMilliseconds()
		{
			var sut = new UbiqConfiguration(0, 0, 0, false, "MILLIS");

			Assert.Equal(ChronoUnit.Milliseconds, sut.EventReportingTimestampGranularity);
		}

		[Fact]
		public void Constructor_Milliseconds_TimestampGranularitySetToMilliseconds()
		{
			var sut = new UbiqConfiguration(0, 0, 0, false, "Milliseconds");

			Assert.Equal(ChronoUnit.Milliseconds, sut.EventReportingTimestampGranularity);
		}
	}
}
