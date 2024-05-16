namespace ocpa.ro.api.Models.Meteo
{
    public class UploadDataPart
    {
        public string PartBase64 { get; set; }
        public int PartIndex { get; set; }
        public int TotalParts { get; set; }
    }

    public enum UploadDataPartFeedback
    {
        Continue,
        Abort,
        Finished
    }

    public class UploadDataPartResponse
    {
        public UploadDataPartFeedback Feedback { get; private set; }
        public string ErrorDetail { get; private set; }

        private UploadDataPartResponse() { }

        public static readonly UploadDataPartResponse Continue =
            new UploadDataPartResponse { Feedback = UploadDataPartFeedback.Continue };

        public static readonly UploadDataPartResponse Finished =
            new UploadDataPartResponse { Feedback = UploadDataPartFeedback.Finished };

        public static UploadDataPartResponse Abort(string error) =>
            new UploadDataPartResponse { Feedback = UploadDataPartFeedback.Finished, ErrorDetail = error };
    }
}
