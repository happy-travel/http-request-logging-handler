using System.Collections.Generic;

namespace HappyTravel.HttpRequestLogger.Models
{
    public record HttpRequestLogEntry
    {
        public string Url { get; init; }
        public string Method { get; init; }
        public Dictionary<string, string>? RequestHeaders { get; init; }
        public Dictionary<string, string>? ResponseHeaders { get; init; }
        public string? RequestBody { get; init; }
        public string? ResponseBody { get; init; }
        public int StatusCode { get; init; }
    }
}