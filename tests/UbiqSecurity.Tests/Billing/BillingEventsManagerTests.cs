using Newtonsoft.Json;
using UbiqSecurity.Config;
using UbiqSecurity.Internals.Billing;
using UbiqSecurity.Internals.WebService;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity.Tests.Billing
{
    public class BillingEventsManagerTests
    {
        [Fact]
        public void GetSerializedEvents_NoBillingEventsInQueue_ReturnsEmptyUsageArray()
        {
            var config = new UbiqConfiguration()
            {
                EventReporting = new EventReportingConfig
                {
                    TimestampGranularity = ChronoUnit.Hours.ToString(),
                }
            };

            var credentials = UbiqFactory.ReadCredentialsFromFile(string.Empty, "ubiq-dotnet");
            var webservice = new UbiqWebService(credentials, config);

            var sut = new BillingEventsManager(config, webservice);

            var result = sut.GetSerializedEvents();

            Assert.Equal("{\"usage\":[]}", result);
        }

        [Fact]
        public async Task GetSerializedEvents_BillingEventInQueue_ReturnsPopulatedUsageArray()
        {
            var config = new UbiqConfiguration()
            {
                EventReporting = new EventReportingConfig
                {
                    TimestampGranularity = ChronoUnit.Hours.ToString(),
                }
            };

            var credentials = UbiqFactory.ReadCredentialsFromFile(string.Empty, "ubiq-dotnet");
            var webservice = new UbiqWebService(credentials, config);

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
