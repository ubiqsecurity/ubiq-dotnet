using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UbiqSecurity.Internals;

namespace UbiqSecurity.Billing
{
	internal class BillingEvent
	{
		private DateTime? _firstCalled = DateTime.UtcNow;
		private DateTime? _lastCalled = DateTime.UtcNow;

		public BillingEvent()
		{
		}

		public BillingEvent(string apiKey, string datasetName, string datasetGroupName, BillingAction billingAction, DatasetType datasetType, int keyNumber, long count)
		{
			ApiKey = apiKey;
			DatasetName = datasetName;
			DatasetGroupName = datasetGroupName;
			BillingAction = billingAction;
			DatasetType = datasetType;
			KeyNumber = keyNumber;
			Count = count;
		}

		[JsonProperty("product")]
		public static string Product => "ubiq-dotnet";

		[JsonProperty("product_version")]
		public static string ProductVersion => typeof(BillingEvent).Assembly.GetName().Version.ToString();

		[JsonProperty("user_agent")]
		public static string UserAgent => $"{Product}/{ProductVersion}";

		[JsonProperty("api_version")]
		public static string ApiVersion => "V3";

		[JsonProperty("api_key")]
		public string ApiKey { get; set; }

		[JsonProperty("datasets")]
		public string DatasetName { get; set; }

		[JsonProperty("dataset_groups")]
		public string DatasetGroupName { get; set; }

		[JsonProperty("action")]
		[JsonConverter(typeof(StringEnumConverter), converterParameters: typeof(LowerCaseNamingStrategy))]
		public BillingAction BillingAction { get; set; }

		[JsonProperty("dataset_type")]
		[JsonConverter(typeof(StringEnumConverter), converterParameters: typeof(LowerCaseNamingStrategy))]
		public DatasetType DatasetType { get; set; }

		[JsonProperty("key_number")]
		public int KeyNumber { get; set; }

		[JsonProperty("count")]
		public long Count { get; set; }

		[JsonProperty("last_call_timestamp")]
		public DateTime? LastCalled { 
			get => _lastCalled.TruncatedTo(TimestampGranularity);
			set => _lastCalled = value;
		}

		[JsonProperty("first_call_timestamp")]
		public DateTime? FirstCalled
		{
			get => _firstCalled.TruncatedTo(TimestampGranularity);
			set => _firstCalled = value;
		}

		[JsonProperty("user_defined")]
		public string UserDefinedMetadata { get; set; }

		[JsonIgnore]
		public ChronoUnit TimestampGranularity { get; set; } = ChronoUnit.Nanoseconds;

		public override string ToString()
		{
			return $"api_key='{ApiKey}' datasets='{DatasetName}' billing_action='{BillingAction.ToString().ToLowerInvariant()}' dataset_groups='{DatasetGroupName}' key_number='{KeyNumber}' dataset_type='{DatasetType.ToString().ToLowerInvariant()}'";
		}
	}
}
