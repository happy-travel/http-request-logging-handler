using System.Collections.Generic;

namespace HappyTravel.HttpRequestLogger.Models
{
    public class HttpRequestLogEntry
    {
        public string? Url { get; set; }
        public string? Method { get; set; }
        public Dictionary<string, string>? RequestHeaders { get; set; }
        public Dictionary<string, string>? ResponseHeaders { get; set; }
        public string? RequestBody { get; set; }
        public string? ResponseBody { get; set; }
        public int StatusCode { get; set; }
    }
}