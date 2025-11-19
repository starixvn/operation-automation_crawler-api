using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Translator.Storage;

namespace Translator.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTranslatorMySqlDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OperationCrawlerDatabase");

        services.AddDbContext<OperationCrawlerDbContext>(options =>
            options.UseMySql(
                connectionString,
                ServerVersion.AutoDetect(connectionString)
            ).LogTo(Console.WriteLine, LogLevel.Information)
        );
        return services;
    }
}