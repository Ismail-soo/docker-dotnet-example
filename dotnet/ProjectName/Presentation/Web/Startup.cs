using System;
using Data.SqlServer;
using ProjectName.Business.Core.Interfaces.Data;
using ProjectName.Business.Core.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Web
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<Startup> _logger;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            // Register DbContext (sederhana dan sudah benar di .NET 8)
            services.AddDbContext<ProjectNameContext>();
            services.AddScoped<IContext, ProjectNameContext>();
            services.AddScoped<IProjectNameContext, ProjectNameContext>();

            // Razor View Expander
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ProjectViewLocationExpander());
            });

            // MVC modern (.NET 8)
            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var webPort = Environment.GetEnvironmentVariable("WEB_PORT");
            _logger.LogInformation($"[Startup.Configure] Started on port: {webPort}");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthorization();

            // Routing modern
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
