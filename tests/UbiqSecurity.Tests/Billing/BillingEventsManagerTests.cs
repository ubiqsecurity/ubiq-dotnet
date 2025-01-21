using Newtonsoft.Json;
using UbiqSecurity.Billing;
using UbiqSecurity.Internals;
using UbiqSecurity.Model;

namespace UbiqSecurity.Tests.Billing
{
    public class BillingEventsManagerTests
	{
		[Fact]
		public void GetSerializedEvents_NoBillingEventsInQueue_ReturnsEmptyUsageArray()
		{
			var config = new UbiqConfiguration()
			{
				EventReportingTimestampGranularity = ChronoUnit.Hours
			};

			var credentials = UbiqFactory.ReadCredentialsFromFile(string.Empty, "ubiq-dotnet");
			var webservice = new UbiqWebServices(credentials, config);
			
			var sut = new BillingEventsManager(config, webservice);

			var result = sut.GetSerializedEvents();

			Assert.Equal("{\"usage\":[]}", result);
		}

		[Fact]
		public async Task GetSerializedEvents_BillingEventInQueue_ReturnsPopulatedUsageArray()
		{
			var config = new UbiqConfiguration()
			{
				EventReportingTimestampGranularity = ChronoUnit.Hours
			};

			var credentials = UbiqFactory.ReadCredentialsFromFile(string.Empty, "ubiq-dotnet");
			var webservice = new UbiqWebServices(credentials, config);

			var sut = new BillingEventsManager(config, webservice);

			await sut.AddBillingEventAsync("key", "dataset", "group", BillingAction.Encrypt, DatasetType.Structured, 1, 1);
			await sut.AddBillingEventAsync("key", "dataset", "group", BillingAction.Decrypt, DatasetType.Structured, 1, 1);

			var result = sut.GetSerializedEvents();

			var request = JsonConvert.DeserializeObject<TrackingEventsRequest>(result);

			Assert.Equal(2, request.Usage.Length);
			Assert.Equal(1, request.Usage.First().Count);
		}
	}
}
