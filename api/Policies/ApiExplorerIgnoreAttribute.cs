using Microsoft.AspNetCore.Mvc;

namespace ocpa.ro.api.Policies
{
    public class ApiExplorerIgnoreAttribute : ApiExplorerSettingsAttribute
    {
        public static bool IsDevelopment { get; set; } = true;

        public new bool IgnoreApi => !IsDevelopment || base.IgnoreApi;

        public ApiExplorerIgnoreAttribute()
        {
            base.IgnoreApi = !IsDevelopment;
        }
    }
}
