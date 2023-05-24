using Newtonsoft.Json;
using UbiqSecurity.Billing;

namespace UbiqSecurity.Model
{
	internal class TrackingEventsRequest
	{
		public TrackingEventsRequest(BillingEvent[] billingEvents)
		{
			Usage = billingEvents;
		}

		[JsonProperty("usage")]
		public BillingEvent[] Usage { get; set; }
	}
}
