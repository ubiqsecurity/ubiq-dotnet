using Newtonsoft.Json;

namespace UbiqSecurity.Internals.WebService.Models
{
    internal class TrackingEventsResponse
    {
        internal TrackingEventsResponse()
        {
        }

        internal TrackingEventsResponse(int status, string message)
        {
            Status = status;
            Message = message;
        }

        [JsonProperty("message")]
        internal string Message { get; set; }

        [JsonProperty("status")]
        internal int Status { get; set; }
    }
}
