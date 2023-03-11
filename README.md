# Distributed Logging with Elasticsearch and Kibana

![Logo](https://i.postimg.cc/WbS5ny0q/2125009.png)

Did you know that up to 30% of software development time is spent debugging code? In this article, we'll show you how to save time and effort by automating the error-catching process and logging errors to Elasticsearch.
Elasticsearch is a distributed, free and open search and analytics engine for all types of data, including textual, numerical, geospatial, structured, and unstructured. And through it, you can store logs of other types, according to your needs in the project. This powerful tool allows you to easily view and analyze system-level errors later on Kibana, a free and open user interface that lets you visualize your Elasticsearch data and navigate the Elastic Stack.
Kibana lets you do anything from tracking query load to understanding the way requests flow through your apps. To enable logging with Elasticsearch in .NET, we'll be using Serilog, a plugin which provides diagnostics logging to files, console, and elsewhere. Various sinks are available for Serilog which we can set up easily has a clean API.


## How to use:
After adding the library to our project, we have to follow the following steps:

1 - We will add some lines to Program.cs
```javascript
using Distributed.Logging.Utilities;

var builder = WebApplication.CreateBuilder(args);
    builder.Host.ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddEnvironmentVariables();
            }).UseSerilog(SeriLoggerUtility.Configure);
var app = builder.Build();
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.Run();
```
2 - add Elastic URL in appseting.json .
```javascript
{
    "Serilog": {
      "MinimumLevel": {
        "Default": "Information",
        "Override": {
          "Microsoft": "Information",
          "System": "Warning"
        }
      }
    },
    "ElasticConfiguration": {
      "Uri": "http://localhost:9200"
    }
}
```
