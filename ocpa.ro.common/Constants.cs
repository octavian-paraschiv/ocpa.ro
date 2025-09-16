namespace ocpa.ro.common;

public static class Constants
{
    public const string AppName = "Backend API for ocpa.ro";
    public const string ApiVersion = "v1";
    public const string CompanyName = "OPMedia Research";
    public const string CopyrightNotice = "Copyright © " + CompanyName;
    public const string DateFormat = "yyyy-MM-dd";

    public const string BackendVersion = "1.0.0";

    public const int MaxPlainRequestSize = 1024 * 1024;
    public const long MaxMultipartRequestSize = 256L * MaxPlainRequestSize;
}