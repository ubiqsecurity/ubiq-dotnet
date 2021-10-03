using Newtonsoft.Json;

namespace UbiqSecurity.Model
{
	public class FpeTransactionRecord
	{
		public FpeTransactionRecord()
		{
		}

		/// <summary>
		/// Constructs a new fpe transaction record
		/// </summary>
		/// <param name="id">a unique GUID string</param>
		/// <param name="action">either "encrypt" or "decrypt"</param>
		/// <param name="ffsName">the name of the FFS model, for example "ALPHANUM_SSN"</param>
		/// <param name="timeStamp">the timestamp String in ISO8601 format</param>
		/// <param name="count">the number of transactions of this type</param>
		public FpeTransactionRecord(string id, string action, string ffsName, string timeStamp, int count)
		{
			Id = id;
			Action = action;
			FfsName = ffsName;
			TimeStamp = timeStamp;
			Count = count;
		}

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("action")]
		public string Action { get; set; }

		[JsonProperty("ffs_name")]
		public string FfsName { get; set; }

		[JsonProperty("timestamp")]
		public string TimeStamp { get; set; }

		[JsonProperty("count")]
		public int Count { get; set; }
	}
}
