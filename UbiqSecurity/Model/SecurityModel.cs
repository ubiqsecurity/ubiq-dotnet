using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UbiqSecurity.Model
{
    public class SecurityModel
    {
        [JsonProperty("algorithm")]
        public string Algorithm { get; set; }

        [JsonProperty("enable_data_fragmentation")]
        public bool EnableDataFragmentation { get; set; }
    }
}
