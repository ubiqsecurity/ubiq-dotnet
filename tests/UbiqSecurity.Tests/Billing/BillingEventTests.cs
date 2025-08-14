using System.Text.Json;
using Newtonsoft.Json;
using UbiqSecurity.Internals.Billing;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace UbiqSecurity.Tests.Billing
{
    public class BillingEventTests
	{
		[Fact]
		public void FirstCalled_ChronoUnitDays_TruncatesFirstCalledToDays()
		{
			var expectedDate = new DateTime(2001, 2, 3);

			var billingEvent = new BillingEvent
			{
				TimestampGranularity = ChronoUnit.Days,
				FirstCalled = new DateTime(2001, 2, 3, 4, 5, 6, 7),
				LastCalled = new DateTime(2001, 2, 3, 4, 5, 6, 7)
			};

			Assert.Equal(expectedDate, billingEvent.FirstCalled);
			Assert.Equal(expectedDate, billingEvent.LastCalled);
		}

		[Fact]
		public void FirstCalled_ChronoUnitHalfDaysPM_TruncatesFirstCalledToNoon()
		{
			var expectedDate = new DateTime(2001, 2, 3, 12, 0 , 0);

			var billingEvent = new BillingEvent
			{
				TimestampGranularity = ChronoUnit.HalfDays,
				FirstCalled = new DateTime(2001, 2, 3, 14, 5, 6, 7),
				LastCalled = new DateTime(2001, 2, 3, 14, 5, 6, 7)
			};

			Assert.Equal(expectedDate, billingEvent.FirstCalled);
			Assert.Equal(expectedDate, billingEvent.LastCalled);
		}

		[Fact]
		public void FirstCalled_ChronoUnitHalfDaysAM_TruncatesFirstCalledToMidnight()
		{
			var expectedDate = new DateTime(2001, 2, 3, 0, 0, 0);

			var billingEvent = new BillingEvent
			{
				TimestampGranularity = ChronoUnit.HalfDays,
				FirstCalled = new DateTime(2001, 2, 3, 4, 5, 6, 7),
				LastCalled = new DateTime(2001, 2, 3, 4, 5, 6, 7)
			};

			Assert.Equal(expectedDate, billingEvent.FirstCalled);
			Assert.Equal(expectedDate, billingEvent.LastCalled);
		}

		[Fact]
		public void FirstCalled_ChronoUnitHours_TruncatesFirstCalledToHours()
		{
			var expectedDate = new DateTime(2001, 2, 3, 4, 0, 0);

			var billingEvent = new BillingEvent
			{
				TimestampGranularity = ChronoUnit.Hours,
				FirstCalled = new DateTime(2001, 2, 3, 4, 5, 6, 7),
				LastCalled = new DateTime(2001, 2, 3, 4, 5, 6, 7)
			};

			Assert.Equal(expectedDate, billingEvent.FirstCalled);
			Assert.Equal(expectedDate, billingEvent.LastCalled);
		}

		[Fact]
		public void FirstCalled_ChronoUnitMinutes_TruncatesFirstCalledToMinutes()
		{
			var expectedDate = new DateTime(2001, 2, 3, 4, 5, 0);

			var billingEvent = new BillingEvent
			{
				TimestampGranularity = ChronoUnit.Minutes,
				FirstCalled = new DateTime(2001, 2, 3, 4, 5, 6, 7),
				LastCalled = new DateTime(2001, 2, 3, 4, 5, 6, 7)
			};

			Assert.Equal(expectedDate, billingEvent.FirstCalled);
			Assert.Equal(expectedDate, billingEvent.LastCalled);
		}

		[Fact]
		public void FirstCalled_ChronoUnitSeconds_TruncatesFirstCalledToSeconds()
		{
			var expectedDate = new DateTime(2001, 2, 3, 4, 5, 6);

			var billingEvent = new BillingEvent
			{
				TimestampGranularity = ChronoUnit.Seconds,
				FirstCalled = new DateTime(2001, 2, 3, 4, 5, 6, 7),
				LastCalled = new DateTime(2001, 2, 3, 4, 5, 6, 7)
			};

			Assert.Equal(expectedDate, billingEvent.FirstCalled);
			Assert.Equal(expectedDate, billingEvent.LastCalled);
		}

		[Fact]
		public void SerializeAndUnserialize_ChronoUnitSeconds_TruncatesFirstCalledToSeconds()
		{
			var expectedDate = new DateTime(2001, 2, 3, 4, 5, 6);

			var billingEvent = new BillingEvent
			{
				TimestampGranularity = ChronoUnit.Seconds,
				FirstCalled = new DateTime(2001, 2, 3, 4, 5, 6, 7),
				LastCalled = new DateTime(2001, 2, 3, 4, 5, 6, 7)
			};

			var jsonString = JsonConvert.SerializeObject(billingEvent);

			var rehydratedEvent = JsonConvert.DeserializeObject<BillingEvent>(jsonString);

			Assert.Equal(expectedDate, rehydratedEvent.FirstCalled);
			Assert.Equal(expectedDate, rehydratedEvent.LastCalled);
		}
	}
}
