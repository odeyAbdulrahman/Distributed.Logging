using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System.Reflection;

namespace Distributed.Logging.Utilities
{
    public static class SeriLoggerUtility
    {
        public static Action<HostBuilderContext, LoggerConfiguration> Configure =>
            (hostContext, configuration) =>
            {
                var IndexFormat = $"app-logs-{Assembly.GetEntryAssembly().GetName().Name.ToLower().Replace(".", "-")}-{hostContext.HostingEnvironment.EnvironmentName.ToLower().Replace(".", "-")}-logs-{DateTime.UtcNow:yyyy-MM}";
                configuration
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithEnvironmentUserName()
                .Filter.ByExcluding(x => x.Level == LogEventLevel.Debug)
                .WriteTo.Elasticsearch
                (
                    new ElasticsearchSinkOptions(new Uri(hostContext.Configuration["ElasticConfiguration:Uri"]))
                    {
                        IndexFormat = IndexFormat,
                        AutoRegisterTemplate = true,
                        NumberOfShards = 2,
                        NumberOfReplicas = 1,
                    }
                 )
                .Enrich.WithProperty("Enviroment", hostContext.HostingEnvironment.EnvironmentName)
                .ReadFrom.Configuration(hostContext.Configuration);
            };
    }
}
