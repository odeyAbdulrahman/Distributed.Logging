using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Sinks.Elasticsearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Distributed.Logging.Utilities
{
    public static class StaticSeriLoggerUtility
    {
        public static ILoggerFactory CreateLoggerFactory(this IConfiguration configuration, IHostEnvironment hostEnvironment)
        {
            string Environment = configuration["ElasticConfiguration:Environment"];
            var IndexFormat = $"app-logs-{Assembly.GetEntryAssembly().GetName().Name.ToLower().Replace(".", "-")}-{hostEnvironment.EnvironmentName.ToLower().Replace(".", "-")}-logs-{DateTime.UtcNow:yyyy-MM}";
            if (Environment == "Online")
                IndexFormat = $"app-logs-{Assembly.GetExecutingAssembly().GetName().Name.ToLower().Replace(".", "-")}-{hostEnvironment.EnvironmentName.ToLower().Replace(".", "-")}-logs-{DateTime.UtcNow:yyyy-MM}";

            string url = configuration["ElasticConfiguration:Uri"];
            var loggerConfiguration = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithEnvironmentUserName()
                .Filter.ByExcluding(x => x.Level == LogEventLevel.Debug)
                .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(url))
                {
                    IndexFormat = IndexFormat,
                    AutoRegisterTemplate = true,
                    NumberOfShards = 2,
                    NumberOfReplicas = 2,
                    ModifyConnectionSettings = conn =>
                    {
                        // Set up any additional connection settings here
                        return conn;
                    }
                })
                .Enrich.WithProperty("Enviroment", hostEnvironment.EnvironmentName)
                .ReadFrom.Configuration(configuration); ;

            var logger = loggerConfiguration.CreateLogger();
            var loggerFactory = new LoggerFactory(new[] { new SerilogLoggerProvider(logger) });
            return loggerFactory;
        }

        public static void Log<T>(this IConfiguration configuration, IHostEnvironment hostEnvironment, string message)
        {
            var loggerFactory = CreateLoggerFactory(configuration, hostEnvironment);
            var logger = loggerFactory.CreateLogger<T>();
            logger.LogInformation(message);
        }
    }
}
