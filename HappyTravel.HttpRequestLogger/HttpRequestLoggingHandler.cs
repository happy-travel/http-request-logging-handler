using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.HttpRequestLogger.Models;
using HappyTravel.HttpRequestLogger.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HappyTravel.HttpRequestLogger
{
    public class HttpRequestLoggingHandler : DelegatingHandler
    {
        public HttpRequestLoggingHandler(IOptionsMonitor<RequestLoggingOptions> loggingOptions, IOptions<SensitiveDataProcessingOptions> sensitiveDataProcessingOptions,
            ILogger<HttpRequestLoggingHandler> logger)
        {
            _loggingOptions = loggingOptions;
            _sensitiveDataProcessingOptions = sensitiveDataProcessingOptions;
            _logger = logger;
        }


        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var options = _loggingOptions.CurrentValue;
            if (!options.IsEnabled)
                return base.SendAsync(request, cancellationToken);

            if (options.UrlPattern is not null)
            {
                var regex = new Regex(options.UrlPattern);
                if (!regex.IsMatch(request.RequestUri?.ToString() ?? string.Empty))
                    return base.SendAsync(request, cancellationToken);
            }

            return SendWithLog(request, options, cancellationToken);
        }


        private async Task<HttpResponseMessage> SendWithLog(HttpRequestMessage request, RequestLoggingOptions options,
            CancellationToken cancellationToken)
        {
            var requestBody = request.Content is not null
                ? await request.Content.ReadAsStringAsync(cancellationToken)
                : null;
            
            var logEntry = new HttpRequestLogEntry
            {
                Method = request.Method.Method,
                Url = request.RequestUri?.AbsoluteUri ?? string.Empty,
                RequestHeaders = options.AreRequestHeadersHidden 
                    ? new ()
                    : GetDictionary(request.Headers),
                RequestBody = requestBody,
            };

            try
            {
                var response = await base.SendAsync(request, cancellationToken);

                logEntry.ResponseHeaders = options.AreResponseHeadersHidden
                    ? new()
                    : GetDictionary(response.Headers);
                logEntry.ResponseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                logEntry.StatusCode = (int) response.StatusCode;
                
                var sanitizedEntry = _sensitiveDataProcessingOptions.Value.SanitizingFunction?.Invoke(logEntry) ?? logEntry;

                WriteLog(sanitizedEntry);
                return response;
            }
            catch (Exception ex)
            {
                WriteLog(logEntry, ex);
                throw;
            }
            

            void WriteLog(HttpRequestLogEntry entry, Exception? exception = null)
            {
                _logger.LogInformation(new EventId(120000, "HttpRequestLogging"),
                    exception, 
                    "{Method}: {Url}\n{RequestHeaders}\n{RequestBody}\n{ResponseHeaders}\n{ResponseBody}\n{StatusCode}",
                    entry.Method,
                    entry.Url,
                    entry.RequestHeaders,
                    entry.RequestBody,
                    entry.ResponseHeaders,
                    entry.ResponseBody,
                    entry.StatusCode);
            }
        }
        
        
        private static Dictionary<string, string> GetDictionary(HttpHeaders headers)
        {
            return headers.ToDictionary(h => h.Key, 
                h => string.Join(";", h.Value));
        }


        private readonly IOptionsMonitor<RequestLoggingOptions> _loggingOptions;
        private readonly IOptions<SensitiveDataProcessingOptions> _sensitiveDataProcessingOptions;
        private readonly ILogger<HttpRequestLoggingHandler> _logger;
    }
}