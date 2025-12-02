using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog.Sinks.ApplicationInsights;

namespace Web
{
    public class Program
    {
        public static int Main(string[] args)
        {
                    var loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console(theme: AnsiConsoleTheme.Code);

        var aiKey = Environment.GetEnvironmentVariable("WEB_APPLICATION_INSIGHTS_KEY");

        if (!string.IsNullOrWhiteSpace(aiKey))
        {
            loggerConfig = loggerConfig.WriteTo.ApplicationInsights(
                aiKey,
                TelemetryConverter.Traces
            );
        }

        Log.Logger = loggerConfig.CreateLogger();

        try
        {
            Log.Information("Starting Web Host");

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog();

            var startup = new Startup(builder.Configuration);
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
