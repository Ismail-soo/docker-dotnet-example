using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.ApplicationInsights.Sinks.ApplicationInsights.TelemetryConverters;
using Serilog.Sinks.SystemConsole.Themes;

namespace Web
{
    public class Program
    {
        public static int Main(string[] args)
        {
            // Setup dasar Serilog
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code);

            // Application Insights (optional)
            var aiKey = Environment.GetEnvironmentVariable("WEB_APPLICATION_INSIGHTS_KEY");
            if (!string.IsNullOrWhiteSpace(aiKey))
            {
                loggerConfig = loggerConfig.WriteTo.ApplicationInsights(
                    aiKey,
                    new TraceTelemetryConverter()   // ← ini COMPATIBLE .NET 8
                );
            }

            Log.Logger = loggerConfig.CreateLogger();

            try
            {
                Log.Information("Starting Web Host");

                var builder = WebApplication.CreateBuilder(args);

                // Integrasi Serilog resmi .NET 8
                builder.Host.UseSerilog();

                // Kompatibilitas dengan Startup.cs lama
                var startupLogger = Log.ForContext<Startup>();
                var startup = new Startup(builder.Configuration, startupLogger);

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
