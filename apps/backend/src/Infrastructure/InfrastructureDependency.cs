using Microsoft.EntityFrameworkCore;
using Minio;
using Stocker.Core.Clients;
using Stocker.Core.Settings;
using Stocker.Infrastructure.Database;
using Stocker.Infrastructure.Database.Interceptors;
using TickerQ.Dashboard.DependencyInjection;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.DbContextFactory;
using TickerQ.EntityFrameworkCore.DependencyInjection;

namespace Stocker.Infrastructure;

public static class InfrastructureDependency
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<StockerDataContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                .AddInterceptors(new SoftDeleteInterceptors());
        });


        services.AddTickerQ(opt =>
        {
            opt.AddOperationalStore(ef =>
            {
                ef.UseTickerQDbContext<TickerQDbContext>(db =>
                    db.UseSqlServer(configuration.GetConnectionString("TickerQ"), b => b.MigrationsAssembly(typeof(Program).Assembly)));
            });
            opt.AddDashboard();
        });


        var minioConfig = configuration.GetSection("Minio").Get<MinioSettings>() ??
                          throw new ArgumentException("Missing Minio configuration");
        services.AddMinio(configureClient =>
        {
            configureClient
                .WithEndpoint(minioConfig.Endpoint)
                .WithCredentials(minioConfig.AccessKey, minioConfig.SecretKey)
                .WithSSL(false);
        });
        services.Configure<MinioSettings>(configuration.GetSection(key: "Minio"));

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });

        return services;
    }
}