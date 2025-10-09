using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using HtmlAgilityPack;

using Tennis;
using Tennis.Extensions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
// Add services to the container.

services.AddHealthChecks();
services.CreateAtpTourHttpClient();
services.CreateItfHttpClient();
services.CreateTimeIsHttpClient();

services.AddTransient<AtpTourCrawler, AtpTourCrawler>();
services.AddTransient<ItfTennisCrawler, ItfTennisCrawler>();
services.AddControllers();

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

app.MapControllers();
app.MapHealthChecks("/healthz");

await app.RunAsync();
