using System;
using Microsoft.Extensions.Logging;

namespace HappyTravel.HttpRequestLogger.Infrastructure
{
    public static class LoggerExtensions
    {
        static LoggerExtensions()
        {
            HttpRequestLogging = LoggerMessage.Define(LogLevel.Information,
                new EventId(120000, "HttpRequestLogging"),
                "Http request logging");
            
        }
    
                
         public static void LogHttpRequestLogging(this ILogger logger, Exception exception = null)
            => HttpRequestLogging(logger, exception);
    
    
        
        private static readonly Action<ILogger, Exception> HttpRequestLogging;
    }
}