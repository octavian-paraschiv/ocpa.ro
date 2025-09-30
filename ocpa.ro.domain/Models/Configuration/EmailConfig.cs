namespace ocpa.ro.domain.Models.Configuration
{
    public class EmailConfig
    {
        public string ServerAddress { get; set; } = "mail.2mwin-dns.com";
        public int ServerPort { get; set; } = 25;
        public string FromAddress { get; set; } = "do_not_reply@ocpa.ro";
        public string FromName { get; set; } = "OCPA.RO EMAIL SENDER";
        public string Credentials { get; set; } = "default credentials";
    }
}
