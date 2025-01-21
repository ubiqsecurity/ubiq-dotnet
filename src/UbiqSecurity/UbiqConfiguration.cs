using System;
using UbiqSecurity.Config;

namespace UbiqSecurity
{
	public class UbiqConfiguration
	{
		public UbiqConfiguration()
		{
		}

		public UbiqConfiguration(int wakeInterval, int minimumCount, int flushInterval, bool trapExceptions)
		{
			EventReportingWakeInterval = wakeInterval;
			EventReportingMinimumCount = minimumCount;
			EventReportingFlushInterval = flushInterval;
			EventReportingTrapExceptions = trapExceptions;
		}

		public UbiqConfiguration(int wakeInterval, int minimumCount, int flushInterval, bool trapExceptions, string timestampGranularity)
			:this(wakeInterval, minimumCount, flushInterval, trapExceptions)
		{
			// backwards compatibility w/ java library
			switch (timestampGranularity?.ToUpperInvariant())
			{
				case "HALF_DAYS":
					timestampGranularity = ChronoUnit.HalfDays.ToString();
					break;
				case "MILLIS":
					timestampGranularity = ChronoUnit.Milliseconds.ToString();
					break;
				case "NANOS":
					timestampGranularity = ChronoUnit.Nanoseconds.ToString();
					break;
			}

			if (!Enum.TryParse(timestampGranularity, out ChronoUnit unit))
			{
				throw new ArgumentException("Argument is not a valid ChronoUnit value", nameof(timestampGranularity)); 
			}

			EventReportingTimestampGranularity = unit;
		}

		/// <summary>
		/// EventReportingWakeIntervalis how many seconds elapse between when the event processor wakes up
		/// and sees what is available to send to the server
		/// </summary>
		public int EventReportingWakeInterval { get; private set; } = 1;

		/// <summary>
		/// EventReportingMinimumCount is how many billing events need to be queued before they will be sent.
		/// A billing event is based on the combination of API key, dataset, dataset_group, key_number, and 
		/// encrypt / decrypt action. So if a single library is used to encrypt 1M records using the same combination
		/// of these fields, this will only count as 1 billing event with a count of 1M
		/// </summary>
		public int EventReportingMinimumCount { get; private set; } = 5;

		/// <summary>
		/// EventReportingFlushInterval addresses the issue above where a single combination of data is used to
		/// encrypt 1M records but the billing event isn't sent because it is only one billing event.
		/// When this interval (seconds) is reached, all billing events will be sent.
		/// </summary>
		public int EventReportingFlushInterval { get; private set; } = 10;

		public bool EventReportingTrapExceptions { get; private set; }

		public ChronoUnit EventReportingTimestampGranularity { get; set; } = ChronoUnit.Nanoseconds;

        public IdpConfig Idp { get; set; }
	}
}
