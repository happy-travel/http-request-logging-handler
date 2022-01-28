namespace HappyTravel.HttpRequestLogger.Options
{
    public class RequestLoggingOptions
    {
        public bool IsEnabled { get; set; }
        public string? UrlPattern { get; set; }
        public bool AreRequestHeadersHidden { get; set; }
        public bool AreResponseHeadersHidden { get; set; }
        public int? MaxRequestBodySize { get; set; }
        public int? MaxResponseBodySize { get; set; }
    }
}