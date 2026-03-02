using System;
using Newtonsoft.Json;

namespace UbiqSecurity.Internals.WebService.Models
{
    internal class DataTypeConfig
    {
        [JsonProperty("size")]
        public long Size { get; set; }

        [JsonProperty("min_input_int_value")]
        public long MinInputIntValue { get; set; }

        [JsonProperty("max_input_int_value")]
        public long MaxInputIntValue { get; set; }

        [JsonProperty("epoch")]
        public DateTime Epoch { get; set; }

        [JsonProperty("min_input_date_value")]
        public DateTime MinInputDateValue { get; set; }

        [JsonProperty("max_input_date_value")]
        public DateTime MaxInputDateValue { get; set; }
    }
}
