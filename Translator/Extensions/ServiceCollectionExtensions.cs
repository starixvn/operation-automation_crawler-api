using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Translator.Repositories;
using Translator.Services;

namespace Translator.Extensions;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddTranslatorService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<CheckTeamNameService, CheckTeamNameServiceImp>();
        services.AddScoped<TeamRepository, TeamRepositoryImp>();
        return services;
    }
}