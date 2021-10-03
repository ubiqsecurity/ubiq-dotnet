using Newtonsoft.Json;

namespace UbiqSecurity.Model
{
	internal class FpeBillingResponse
	{
		#region Serializable Properties

		[JsonProperty("message")]
		internal string Message { get; set; }

		[JsonProperty("status")]
		internal int Status { get; set; }

		[JsonProperty("last_valid")]
		internal IdRecord LastValidRecord { get; set; }

		#endregion
	}

	internal class IdRecord
	{
		#region Serializable Properties

		[JsonProperty("id")]
		internal string Id { get; set; }

		#endregion
	}
}
