using System;

namespace ocpa.ro.domain.Models.Medical
{
    public class TestSearchRequest
    {
        public int? Id { get; set; }
        public int? Pid { get; set; }
        public string Cnp { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
