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
1. Install the _HappyTravel.HttpRequestLogger_ package
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

3. Add a section "_RequestLoggingOptions_" to app configuration root

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

#### Additional
Logging will work only if the logging level for the the app or the handler will be set to "Information" or higher.
To enable log level only for the handler use logging configuration as in example:

```js
"Logging": {
    "LogLevel": {
    ...
      "HappyTravel.HttpRequestLogger": "Information",
    ...
    }
  }
```
