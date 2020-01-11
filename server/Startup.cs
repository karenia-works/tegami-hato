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

namespace Karenia.TegamiHato.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var domain = Environment.GetEnvironmentVariable("hato_domain");
            if (domain == null) throw new Exception("API Key not defined. Please define API key as environment variable 'hato_domain'");
            var apiKey = Environment.GetEnvironmentVariable("hato_api_key");
            if (apiKey == null) throw new Exception("API Key not defined. Please define API key as environment variable 'hato_api_key'");

            services.AddLogging();
            services.AddSingleton<EmailRecvService>((srv) => new EmailRecvService(domain, apiKey, srv.GetService<ILogger<EmailRecvService>>()));
            services.AddSingleton<EmailSendingService>((srv) => new EmailSendingService(domain, apiKey, srv.GetService<ILogger<EmailSendingService>>()));
            services.AddSingleton<MailingChannelService>();

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
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
