using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HappyTravel.HttpRequestLogger.Infrastructure;
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
                var regex = new Regex(options.UrlPattern, RegexOptions.Compiled);
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

                logEntry = logEntry with
                {
                    ResponseHeaders = options.AreResponseHeadersHidden
                        ? new()
                        : GetDictionary(response.Headers),
                    ResponseBody = await response.Content.ReadAsStringAsync(cancellationToken),
                    StatusCode = (int) response.StatusCode
                };
                
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
                var sb = new StringBuilder();
                var data = new Dictionary<string, object>
                {
                    ["Method"] = entry.Method,
                    ["Url"] = entry.Url,
                    ["RequestHeaders"] = ConvertToString(entry.RequestHeaders, sb),
                    ["RequestBody"] = entry.RequestBody ?? string.Empty,
                    ["ResponseHeaders"] = ConvertToString(entry.ResponseHeaders, sb),
                    ["ResponseBody"] = entry.ResponseBody ?? string.Empty,
                    ["StatusCode"] = entry.StatusCode.ToString()
                };

                using var scope = _logger.BeginScope(data);
                _logger.LogHttpRequestLogging(exception);
            }
        }
        
        
        private static Dictionary<string, string> GetDictionary(HttpHeaders headers)
        {
            return headers.ToDictionary(h => h.Key, 
                h => string.Join(";", h.Value));
        }

        private static string ConvertToString(Dictionary<string, string>? dictionary, StringBuilder sb)
        {
            if (dictionary is null)
                return string.Empty;

            sb.Clear();

            foreach (var (key, value) in dictionary) 
                sb.AppendFormat("{0}: {1}{2}", key, value, Environment.NewLine);

            return sb.ToString();
        }


        private readonly IOptionsMonitor<RequestLoggingOptions> _loggingOptions;
        private readonly IOptions<SensitiveDataProcessingOptions> _sensitiveDataProcessingOptions;
        private readonly ILogger<HttpRequestLoggingHandler> _logger;
    }
}