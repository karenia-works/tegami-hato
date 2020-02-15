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
using StackExchange.Redis;
using System.Runtime.Caching;
using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.Services;
using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;

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

            services.AddDistributedMemoryCache();


            // * Database
            services.AddDbContext<Models.EmailSystemContext>(
                options =>
                {
                    options.UseNpgsql(
                        "Host=localhost;Database=hato_db;Port=54321;Username=postgres;Password=postgres"
                    );
                }
            );
            {
                var db = services.BuildServiceProvider().GetService<Models.EmailSystemContext>();
                db.Database.Migrate();
            }

            services.AddScoped<DatabaseService>();

            services.AddSingleton<ICorsPolicyService>(
                new DefaultCorsPolicyService(
                    new LoggerFactory().CreateLogger<DefaultCorsPolicyService>())
                {
                    AllowedOrigins = new[] { "*" },
                    AllowAll = true
                });

            // * Email service
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

                services.AddSingleton<EmailRecvAdaptor>();
            }


            services.AddIdentityServer(opt =>
            {
                opt.Events.RaiseErrorEvents = true;
                opt.Events.RaiseFailureEvents = true;
                opt.UserInteraction.LoginUrl = null;
                opt.UserInteraction.LogoutUrl = null;
            })
               .AddInMemoryClients(IdentityConstants.clients)
               .AddInMemoryCaching()
            //    .AddPersistedGrantStore()
               .AddInMemoryApiResources(IdentityConstants.apiResources)
               .AddResourceOwnerValidator<UserIdentityService>()
               .AddDeveloperSigningCredential()
               .AddJwtBearerClientAuthentication();

            services.AddAuthorization();
            services.AddAuthentication();
            services.AddLocalApiAuthentication();

            services.AddControllers();
        }

        public class InspectMiddleware { }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger, ILogger<InspectMiddleware> logger2)
        {
            app.Use(async (ctx, next) =>
            {
                await next.Invoke();
                logger2.LogInformation($"{ctx.Response.StatusCode}:{ctx.Request.Path}{ctx.Request.QueryString}");
            });

            foreach ((var level, var log) in pendingLogs)
            {
                logger.Log(level, log);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();

            app.UseRouting();
            app.UseIdentityServer();

            // Warm up
            app.ApplicationServices.GetService<MailingChannelService>();

            app.UseAuthorization();
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
