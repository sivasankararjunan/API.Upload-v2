

using Microsoft.Extensions.Logging;

namespace FileUploadService
{
    public class AppInsight
    {
        public string InstrumentationKey { get; set; }
        public LogLevel LogLevel { get; set; }
    }
}
