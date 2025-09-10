using Logship.Template.Exporter.ConsoleHost.Internal;
using Logship.Template.Utility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Logship.Template.Exporter.ConsoleHost
{
    internal sealed class Program
    {
        static async Task<int> Main(string[] args)
        {
            using var cts = new CancellationTokenSource();
            var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings()
            {
                ApplicationName = "Logship.Template.Exporter",
                Args = args,
                DisableDefaults = true,
            });

            var config = builder.Configuration
                .AddCommandLine(args)
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json")
                .Build();

            builder.Logging.AddConsole();

            builder.Services
                .AddHttpClient()
                .AddTemplateServices(config);

            using var app = builder.Build();
            var logger = app.Services.GetRequiredService<ILogger<Program>>();
            try
            {
                await app.RunAsync();
            }
            catch (OperationCanceledException) when (cts.IsCancellationRequested)
            {
                // noop
            }
            catch (Exception ex)
            {
                Log.UncaughtException(logger, ex);
                return 1;
            }

            return 0;
        }
    }
}
