namespace ocpa.ro.api.Models
{
    public class UploadDataPart
    {
        public string PartBase64 { get; set; }
        public int PartIndex { get; set; }
        public int TotalParts { get; set; }
    }
}
