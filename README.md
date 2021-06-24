# http-request-logging-handler

#### Summary
Common request handler pluggable as delegating handler with hot configuration reload support.
Can be added to any HttpClient.
Request fields captured:
- HttpMethod
- Url
- Request headers
- Request body
- Response headers
- Response body
- Status code

#### Usage
1. Install the package
2. Call configuration method after other client handlers registration:

```cs
services.AddHttpClient("ClientName", client => client.BaseAddress = new Uri("https://api-url"))
                .AddHttpClientRequestLogging(configuration: configuration,
                    sensitiveDataLoggingOptions: options =>
                    {
                        options.SanitizingFunction = entry =>
                        {
                            // Clear all confidential information
                            return entry;
                        };
                    });
```

Method parameters:
- _configuration_ - IConfiguration of app, with no dependency on its source: appSettings.json, consul etc.
- _sensitiveDataLoggingOptions_ - an object containing a delegate that will clear the data not to be logged **[Optional]**

3. Add a section "RequestLoggingOptions" to app configuration

```js
...
{
  "RequestLoggingOptions": 
  {
  	"IsEnabled": true, // to enable or disable the whole handler
    "UrlPattern": "*/test$", // url pattern in RegEx format to allow logging [Optional]
    "AreRequestHeadersHidden": true, // true if request headers should not be logged [Optional]
    "AreResponseHeadersHidden": true // true if response heades should not be logged [Optional]
  }
}
...
```
