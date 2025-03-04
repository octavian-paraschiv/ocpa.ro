namespace ocpa.ro.api.Models.Configuration
{
    public class EmailConfig
    {
        public static readonly string SectionName = "Email";

        public string ServerAddress { get; set; }
        public int ServerPort { get; set; }
        public string FromAddress { get; set; }
        public string FromName { get; set; }
        public string Credentials { get; set; }
    }
}
