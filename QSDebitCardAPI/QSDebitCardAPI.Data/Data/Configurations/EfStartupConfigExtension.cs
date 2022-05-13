using QSDataUpdateAPI.Data.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace QSDataUpdateAPI.Data
{
    public static class EfStartupConfigExtension
    {
        public static IServiceCollection AddDbContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<QuickServiceDbContext>(options =>
                       options.UseSqlServer(configuration.GetConnectionString("QuickServiceDbConn")));
            return services;
        }
        public static IServiceCollection AddEntityFrameworkProvider(this IServiceCollection services)
        {
            services.AddEntityFrameworkSqlServer();
            return services;
        }
    }
}
