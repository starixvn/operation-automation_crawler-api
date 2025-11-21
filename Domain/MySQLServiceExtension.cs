using Domain.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Domain;
public static class MySQLServiceExtension
{
    public static IServiceCollection AddMySqlDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OperationCrawlerDatabase");

        services.AddDbContext<OperationCrawlerDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
            ).LogTo(Console.WriteLine, LogLevel.Information),
            ServiceLifetime.Singleton
        );
        return services;
    }
}
