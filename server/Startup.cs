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


            var pgsqlLinkParams = Environment.GetEnvironmentVariable("hato_pgsql");
            // * Database
            services.AddDbContext<Models.EmailSystemContext>(
                options =>
                {
                    options.UseNpgsql(
                       pgsqlLinkParams
                    );
                }
            );
            if (Environment.GetEnvironmentVariable("ef_test") != null) return;

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
            {
                // * Email service
                var domain = Environment.GetEnvironmentVariable("hato_domain");
                var apiKey = Environment.GetEnvironmentVariable("hato_api_key");

                if (domain == null)
                    pendingLogs.Add((LogLevel.Error, "Email domain not defined. Please define API key as environment variable 'hato_domain'. Email server will not start."));

                if (apiKey == null)
                    pendingLogs.Add((LogLevel.Error, "API Key not defined. Please define API key as environment variable 'hato_api_key'. Email server will not start."));

                if (domain != null && apiKey != null)
                {
                    services.AddSingleton<EmailRecvService>((srv) => new EmailRecvService(domain, apiKey, srv.GetService<ILogger<EmailRecvService>>()));

                    services.AddSingleton<EmailSendingService>((srv) => new EmailSendingService(domain, apiKey, srv.GetService<ILogger<EmailSendingService>>()));

                    services.AddScoped<EmailRecvAdaptor>();
                }
            }

            {
                // config OSS
                var domain = Environment.GetEnvironmentVariable("hato_oss_domain");
                var key = Environment.GetEnvironmentVariable("hato_oss_key");
                var secret = Environment.GetEnvironmentVariable("hato_oss_secret");
                var spaceName = Environment.GetEnvironmentVariable("hato_oss_space");

                if (domain == null)
                    pendingLogs.Add((LogLevel.Error, "OSS domain not defined. Please define API key as environment variable 'hato_oss_domain'."));
                if (key == null)
                    pendingLogs.Add((LogLevel.Error, "OSS key not defined. Please define API key as environment variable 'hato_oss_key'."));
                if (secret == null)
                    pendingLogs.Add((LogLevel.Error, "OSS secret not defined. Please define API key as environment variable 'hato_oss_secret'."));
                if (spaceName == null)
                    pendingLogs.Add((LogLevel.Error, "OSS space name not defined. Please define API key as environment variable 'hato_oss_space'."));

                if (domain != null && key != null && spaceName != null && secret != null)
                    services.AddSingleton<ObjectStorageService>(srv => new ObjectStorageService(domain, key, secret, spaceName));
            }

            var identityServer = services.AddIdentityServer(opt =>
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
                .AddJwtBearerClientAuthentication();


            if (Environment.GetEnvironmentVariable("hato_use_devel_sign") == null)
            {
                var cert_filename = Environment.GetEnvironmentVariable("cert_name");
                var cert_password = Environment.GetEnvironmentVariable("cert_password");
                if (cert_filename == null || cert_password == null)
                {
                    // first let's get pass building phase
                    identityServer.AddDeveloperSigningCredential();
                    pendingLogs.Add((LogLevel.Error, "Certificate not provided! please provide certificate path as 'cert_path' and certificate password as 'cert_password'!"));
                }
                var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2($"{cert_filename}", cert_password);
                identityServer.AddSigningCredential(cert, "RS256");
            }
            else
            {
                identityServer.AddDeveloperSigningCredential();
            }

            services.AddAuthorization(option =>
            {
                option.AddPolicy(
                "api", policy =>
                {
                    policy.AddAuthenticationSchemes("api");

                    policy.RequireAuthenticatedUser();
                }
                );
            });
            services.AddAuthentication().AddLocalApi("api", options =>
            {
                options.ExpectedScope = "api";
            });
            services.AddLocalApiAuthentication();
            services.AddControllers();
            services.AddCors(x => x.AddPolicy("AcceptAll", builder =>
            {
                builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
            }));
        }

        public class InspectMiddleware { }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env,
            ILogger<Startup> logger,
            ILogger<InspectMiddleware> logger2,
            IHostApplicationLifetime lifetime)
        {
            app.UseCors("AcceptAll");
            if (env.IsDevelopment())
                app.Use(async (ctx, next) =>
                {
                    await next.Invoke();
                    logger2.LogInformation($"{ctx.Response.StatusCode}:{ctx.Request.Path}{ctx.Request.QueryString}");
                });

            foreach ((var level, var log) in pendingLogs)
            {
                logger.Log(level, log);
            }
            if (pendingLogs.Any(item => item.Item1 == LogLevel.Error))
            {
                logger.LogCritical("There's error in startup. Aborting.");
                lifetime.StopApplication();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();

            app.UseRouting();
            app.UseIdentityServer();

            app.UseAuthorization();
            app.UseAuthentication();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
