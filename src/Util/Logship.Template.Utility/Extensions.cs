using Logship.Template.Utility.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace Logship.Template.Utility
{
    public static class Extensions
    {
        [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
        public static IServiceCollection AddTemplateServices(this IServiceCollection services, IConfiguration config)
        {
            var outputMode = config.GetValue<string>("outputMode") ?? "agent";
            var endpoint = config.GetValue<string>("logshipEndpoint")!.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new InvalidOperationException("logshipEndpoint is required.");
            }

            if (outputMode.Equals("direct", StringComparison.OrdinalIgnoreCase))
            {
                var account = config.GetValue<Guid>("logshipAccount")!;
                var bearerToken = config.GetValue<string>("logshipBearerToken")!;
                services.AddSingleton<ILogshipExporter, LogshipExporter>(_ =>
                {
                    return new LogshipExporter(_.GetRequiredService<IHttpClientFactory>(), endpoint, account, bearerToken);
                });
            }
            else if (outputMode.Equals("agent", StringComparison.OrdinalIgnoreCase))
            {
                services.AddSingleton<ILogshipExporter, LogshipAgentExporter>(_ =>
                {
                    return new LogshipAgentExporter(endpoint, _.GetRequiredService<ILogger<LogshipAgentExporter>>());
                });
            }
            else
            {
                throw new InvalidOperationException($"Invalid outputMode '{outputMode}'. Valid values are 'agent' or 'direct'");
            }

            // Add services here.
            return services;
        }
    }
}
