using System;
using HappyTravel.HttpRequestLogger.Models;
using HappyTravel.HttpRequestLogger.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.HttpRequestLogger
{
    public static class HttpLoggingServiceCollectionExtensions
    {
        public static void AddHttpClientRequestLogging(this IHttpClientBuilder builder, IConfiguration configuration, 
            Action<SensitiveDataProcessingOptions> sensitiveDataLoggingOptions)
        {
            builder.Services.Configure<RequestLoggingOptions>(configuration.GetSection("RequestLoggingOptions"));
            builder.Services.Configure(sensitiveDataLoggingOptions);
            builder.Services.AddTransient<HttpRequestLoggingHandler>();
            builder.AddHttpMessageHandler<HttpRequestLoggingHandler>();
        }
    }
}