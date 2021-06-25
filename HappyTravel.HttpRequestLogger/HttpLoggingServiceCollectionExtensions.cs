using System;
using HappyTravel.HttpRequestLogger.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HappyTravel.HttpRequestLogger
{
    public static class HttpLoggingServiceCollectionExtensions
    {
        public static IHttpClientBuilder AddHttpClientRequestLogging(this IHttpClientBuilder builder, IConfiguration configuration, 
            Action<SensitiveDataProcessingOptions>? sensitiveDataLoggingOptions = null)
        {
            builder.Services.Configure<RequestLoggingOptions>(configuration.GetSection("RequestLoggingOptions"));
            builder.Services.AddTransient<HttpRequestLoggingHandler>();
            builder.AddHttpMessageHandler<HttpRequestLoggingHandler>();

            if (sensitiveDataLoggingOptions is not null)
                builder.Services.Configure(sensitiveDataLoggingOptions);
            else
                builder.Services.Configure<SensitiveDataProcessingOptions>(o => o.SanitizingFunction = null);

            return builder;
        }
    }
}