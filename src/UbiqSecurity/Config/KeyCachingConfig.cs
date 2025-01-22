namespace UbiqSecurity.Config
{
    public class KeyCachingConfig
    {
        public bool Unstructured { get; set; } = true;

        public bool Structured { get; set; } = true;

        public bool Encrypt { get; set; }

        public int TtlSeconds { get; set; } = 1800;
    }
}
