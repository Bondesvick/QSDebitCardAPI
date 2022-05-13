using System;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using QSDataUpdateAPI.Core.Domain.Entities;
using QSDataUpdateAPI.Core.Interfaces.Services.Helpers.Redbox;
using QSDataUpdateAPI.Data;
using QSDataUpdateAPI.Data.Data.Repositories;
using QSDataUpdateAPI.Data.Repositories;
using QSDataUpdateAPI.Domain.Services;
using QSDataUpdateAPI.Domain.Services.Helpers;
using QSDataUpdateAPI.Domain.Services.Interfaces;
using QSDataUpdateAPI.Domain.Services.RedboxProxies;
using QSDataUpdateAPI.Filters;
using QSDebitCardAPI.Domain.Services.RedboxServiceProxies;
using QSDebitCardAPI.Domain.Services.RedboxServiceProxies.Interfaces;
using QSDebitCardAPI.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace QSDebitCardAPI.Helpers.Extensions
{
    public static class StartupConfigExtensions
    {
        public static CorsOptions ConfigureCorsPolicy(this CorsOptions corsOptions, IConfiguration configuration)
        {
            var allowedOriginsConfig = configuration["AppSettings:AllowedOrigins"];
            var allowedOrigins = string.IsNullOrEmpty(allowedOriginsConfig) ? new string[] { } : allowedOriginsConfig.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(o => o.Trim());
            if (allowedOrigins.Any())
            {
                corsOptions.AddPolicy("AllowSpecifiedHostsOnly",
                                  corsPolicyBuilder => corsPolicyBuilder
                                  .WithOrigins(allowedOrigins.ToArray())
                                  .AllowAnyHeader()
                                  .AllowAnyMethod()
                                 );
            }
            else
            {
                corsOptions.AddPolicy("AllowSpecifiedHostsOnly",
                              corsPolicyBuilder => corsPolicyBuilder
                                                    .AllowAnyHeader()
                                                    .AllowAnyMethod());
            }
            return corsOptions;
        }

        public static SwaggerGenOptions ConfigureSwagger(this SwaggerGenOptions options)
        {
            options.SwaggerDoc("QuickService_Debit_Card_Request_v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "QuickService Debit Card Request API",
                Version = "v1",
                Description = "API Docs QuickService Debit Card Request"
            });
            options.OperationFilter<SwaggerCustomHeaderFilter>();
            //options.ResolveConflictingActions((description) => description.First());
            return options;
        }

        public static SwaggerUIOptions ConfigureSwaggerUI(this SwaggerUIOptions options)
        {
            options.SwaggerEndpoint("./swagger/QuickService_Debit_Card_Request_v1/swagger.json", " QuickService Debit Card Request API Docs");
            options.RoutePrefix = string.Empty;
            return options;
        }

        public static IServiceCollection ConfigureApplicationCoreServices(this IServiceCollection services)
        {
            services.AddScoped<IRepository<CustomerRequest, long>, BaseEFRepository<CustomerRequest, long>>();
            services.AddScoped<IRepository<Audit, long>, BaseEFRepository<Audit, long>>();
            services.AddScoped<IRepository<DebitCardDetails, int>, BaseEFRepository<DebitCardDetails, int>>();
            services.AddScoped<ICustomerRequestDataRepository, CustomerRequestDataRepository>();
            services.AddScoped<ICustomerRequestService, CustomerRequestService>();
            services.AddTransient<IIdVerificationService, IdVerificationService>();
            services.AddTransient<IAuditLogService, AuditLogService>();
            return services;
        }

        public static IServiceCollection ConfigureInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            //data services
            services.AddDbContext(configuration);
            services.AddEntityFrameworkProvider();

            services.AddSingleton<IAppLogger, AppLoger>();
            services.AddScoped<IAppSettings, AppSettings>();
            services.AddScoped<ISoapRequestHelper, SoapRequestHelper>();
            services.AddScoped<IRedboxRequestManagerProxy, RedboxRequestManagerProxy>();
            services.AddScoped<IRedboxAccountServiceProxy, AccountServiceProxy>();
            services.AddScoped<IRedboxOtpServiceProxy, OtpServiceProxy>();
            services.AddScoped<IRedboxSMSService, RedboxSMSServiceProxy>();
            services.AddScoped<IRedboxEmailService, RedboxEmailServiceProxy>();

            return services;
        }

        public static IServiceCollection ConfigureAppFilters(this IServiceCollection services)
        {
            services.AddScoped<AuthSecretKeyFilter>();
            return services;
        }

        public static IApplicationBuilder ConfigUseDirectoryBrowsing(this IApplicationBuilder app)
        {
            var logPath = $"{Directory.GetCurrentDirectory()}/Logs";
            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "Logs")),
                RequestPath = "/Logs"
            });
            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(
            Path.Combine(Directory.GetCurrentDirectory(), "Logs")),
                RequestPath = "/Logs"
            });
            return app;
        }
    }
}