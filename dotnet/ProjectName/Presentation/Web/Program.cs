using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Web
{
    public class Program
    {
        public static int Main(string[] args)
        {
            // Setup Serilog awal
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code);

            // Optional Application Insights
            var aiKey = Environment.GetEnvironmentVariable("WEB_APPLICATION_INSIGHTS_KEY");
            if (!string.IsNullOrWhiteSpace(aiKey))
            {
                Log.Information("Using Application Insights with Serilog");
                loggerConfig = loggerConfig.WriteTo.ApplicationInsights(aiKey, TelemetryConverter.Events);
            }

            Log.Logger = loggerConfig.CreateLogger();

            try
            {
                Log.Information("Starting Web Host");

                var builder = WebApplication.CreateBuilder(args);

                // Integrate Serilog into .NET host
                builder.Host.UseSerilog();

                // Add Startup.cs style support
                var startup = new Startup(builder.Configuration, Log.Logger.ForContext<Startup>());
                startup.ConfigureServices(builder.Services);

                var app = builder.Build();
                startup.Configure(app, app.Environment);

                app.Run();

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
