using Newtonsoft.Json;

namespace UbiqSecurity.Model
{
	internal class FpeBillingResponse
	{
		internal FpeBillingResponse()
		{
		}

		internal FpeBillingResponse(int status, string message)
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
