using Newtonsoft.Json;
using UbiqSecurity.Internals.Billing;

namespace UbiqSecurity.Internals.WebService.Models
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
