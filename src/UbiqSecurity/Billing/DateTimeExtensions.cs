using System;

namespace UbiqSecurity.Billing
{
	internal static class DateTimeExtensions
	{
		public static DateTime TruncatedTo(this DateTime dateTime, ChronoUnit unit)
		{
			switch (unit)
			{
				case ChronoUnit.Days:
					return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0);
				case ChronoUnit.HalfDays:
					if (dateTime.Hour >= 12)
					{ 
						return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 12, 0, 0, 0);
					}
					else
					{
						return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, 0, 0, 0, 0);
					}
				case ChronoUnit.Hours:
					return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0, 0);
				case ChronoUnit.Minutes:
					return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0, 0);
				case ChronoUnit.Seconds:
					return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, 0);
				case ChronoUnit.Milliseconds:
					return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Millisecond);
				case ChronoUnit.Nanoseconds:
				default:
					return dateTime;
			}
		}

		public static DateTime? TruncatedTo(this DateTime? dateTime, ChronoUnit unit)
		{
			if (dateTime == null || unit == ChronoUnit.Nanoseconds)
			{
				return dateTime;
			}

			return dateTime.Value.TruncatedTo(unit);
		}
	}
}
