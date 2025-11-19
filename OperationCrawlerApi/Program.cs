using Fanex.Caching.Redis;
using Helper;
using Microsoft.OpenApi.Models;
using Tennis;
using Tennis.Extensions;
using Tennis.Storage;
using Translator.Extensions;
using Translator.Repositories;
using Translator.Services;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
var configuration = builder.Configuration;
// Add services to the container.

services.AddHealthChecks();
services.AddTimeIsHttpClient();
services.AddN8NHttpClient();
services.AddTransient<ItfTennisParser>();
services.AddTransient<DynamicCaching>();
services.AddTransient<AtpTourCrawler>();
services.AddTransient<TennisDataService>();
services.AddScoped<CheckTeamNameService, CheckTeamNameServiceImp>();
services.AddScoped<TeamRepository, TeamRepositoryImp>();

// Register memory cache
services.AddMemoryCache();
services.AddRedisCache(configuration.GetSection("FanexCachingRedis"));
services.AddTranslatorMySqlDatabase(configuration);
services.AddTennisMySqlDatabase(configuration);
services.AddControllers();
services.AddRazorPages();

// Configure Swagger/OpenAPI
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tennis Operation API",
        Version = "v1",
        Description = "API for crawling tennis data from various sources"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tennis Operation API v1");
    c.RoutePrefix = string.Empty; // Serve Swagger UI at the root URL
});

app.MapRazorPages();
app.MapControllers();
app.MapHealthChecks("/healthz");

await app.RunAsync();