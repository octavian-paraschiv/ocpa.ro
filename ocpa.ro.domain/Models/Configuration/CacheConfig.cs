namespace ocpa.ro.domain.Models.Configuration
{
    public class CacheConfig
    {
        public int CachePeriod { get; set; } = 30;
        public int MinSizeKB { get; set; } = 64;
    }
}
