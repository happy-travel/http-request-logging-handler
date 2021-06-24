using System;
using HappyTravel.HttpRequestLogger.Models;

namespace HappyTravel.HttpRequestLogger.Options
{
    public class SensitiveDataProcessingOptions
    {
        public Func<HttpRequestLogEntry, HttpRequestLogEntry>? SanitizingFunction { get; set; }
    }
}