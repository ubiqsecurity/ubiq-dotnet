using System.Collections.Generic;
using UbiqSecurity.Internals.Cache;
using UbiqSecurity.Internals.WebService.Models;

namespace UbiqSecurity.Internals.Structured.Pipeline
{
    internal class OperationContext
    {
        public FfsRecord Dataset { get; set; }

        public IFfxCache FfxCache { get; set; }

        public int? KeyNumber { get; set; }

        public string OriginalValue { get; set; }

        public string CurrentValue { get; set; }

        public bool IsEncrypt { get; set; }

        public byte[] UserSuppliedTweak { get; set; }

        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
    }
}
