using Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Tennis.Storage;

namespace Tennis.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTennisMySqlDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TennisDatabase");

        services.AddDbContext<TennisDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
            ).LogTo(Console.WriteLine, LogLevel.Information)
        );
        return services;
    }

    public static IServiceCollection AddTennisService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddN8NHttpClient();
        services.AddTimeIsHttpClient();
        services.AddTransient<ItfTennisParser>();
        services.AddTransient<DynamicCaching>();
        services.AddTransient<AtpTourCrawler>();
        services.AddTransient<TennisDataService>();
        services.AddTennisMySqlDatabase(configuration);

        return services;
    }
}