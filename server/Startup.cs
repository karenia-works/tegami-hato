using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Karenia.TegamiHato.Server.Services;
using Npgsql;
using Microsoft.EntityFrameworkCore;

namespace Karenia.TegamiHato.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private List<(LogLevel, string)> pendingLogs = new List<(LogLevel, string)>();
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();

            var domain = Environment.GetEnvironmentVariable("hato_domain");
            var apiKey = Environment.GetEnvironmentVariable("hato_api_key");
            if (domain == null)
                pendingLogs.Add((LogLevel.Warning, "Email domain not defined. Please define API key as environment variable 'hato_domain'. Email server will not start."));
            else if (apiKey == null)
                pendingLogs.Add((LogLevel.Warning, "API Key not defined. Please define API key as environment variable 'hato_api_key'. Email server will not start."));
            else
            {
                pendingLogs.Add((LogLevel.Information, "Starting email sending and receiving services."));
                services.AddSingleton<EmailRecvService>((srv) => new EmailRecvService(domain, apiKey, srv.GetService<ILogger<EmailRecvService>>()));

                services.AddSingleton<EmailSendingService>((srv) => new EmailSendingService(domain, apiKey, srv.GetService<ILogger<EmailSendingService>>()));

                services.AddSingleton<MailingChannelService>();
            }

            services.AddDbContext<Models.EmailSystemContext>(
                options => options.UseNpgsql(
                   "Host=localhost;Database=hato_db;Username=postgres;Password=postgres"
                )
            );
            services.BuildServiceProvider().GetService<Models.EmailSystemContext>().Database.Migrate();

            services.AddScoped<DatabaseService>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            foreach ((var level, var log) in pendingLogs)
            {
                logger.Log(level, log);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            // Warm up
            app.ApplicationServices.GetService<MailingChannelService>();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
