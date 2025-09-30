namespace ocpa.ro.domain.Models.Configuration
{
    public class DatabaseConfig
    {
        public static readonly string SectionName = "Database";
        public string ConnectionString { get; set; }
        public string MeteoConnectionString { get; set; }
    }
}
