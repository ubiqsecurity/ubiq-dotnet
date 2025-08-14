using System;
using Newtonsoft.Json;

namespace UbiqSecurity.Config
{
    public class EventReportingConfig
    {
        private string _timestampGranularity = "NANOS";

        /// <summary>
        /// Gets or sets how many seconds elapse between when the event processor wakes up
        /// and sees what is available to send to the server
        /// </summary>
        [JsonProperty("wake_interval")]
        public int WakeInterval { get; set; } = 1;

        /// <summary>
        /// Gets or sets how many billing events need to be queued before they will be sent.
        /// A billing event is based on the combination of API key, dataset, dataset_group, key_number, and
        /// encrypt / decrypt action. So if a single library is used to encrypt 1M records using the same combination
        /// of these fields, this will only count as 1 billing event with a count of 1M
        /// </summary>
        [JsonProperty("minimum_count")]
        public int MinimumCount { get; set; } = 5;

        /// <summary>
        /// Gets or sets FlushInterval, which addresses the issue above where a single combination of data is used to
        /// encrypt 1M records but the billing event isn't sent because it is only one billing event.
        /// When this interval (seconds) is reached, all billing events will be sent.
        /// </summary>
        [JsonProperty("flush_interval")]
        public int FlushInterval { get; set; } = 10;

        [JsonProperty("trap_exceptions")]
        public bool TrapExceptions { get; set; } = true;

        [JsonProperty("timestamp_granularity")]
        public string TimestampGranularity
        {
            get => _timestampGranularity;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }

                // backwards compatibility w/ java library
                switch (value.ToUpperInvariant())
                {
                    case "HALF_DAYS":
                        _timestampGranularity = ChronoUnit.HalfDays.ToString();
                        break;
                    case "MILLIS":
                        _timestampGranularity = ChronoUnit.Milliseconds.ToString();
                        break;
                    case "NANOS":
                        _timestampGranularity = ChronoUnit.Nanoseconds.ToString();
                        break;
                    default:
                        _timestampGranularity = value;
                        break;
                }

                if (!Enum.TryParse(_timestampGranularity, out ChronoUnit unit))
                {
                    throw new ArgumentException("Argument is not a valid ChronoUnit value", nameof(value));
                }

                ChronoTimestampGranularity = unit;
            }
        }

        public ChronoUnit ChronoTimestampGranularity { get; private set; } = ChronoUnit.Nanoseconds;
    }
}
