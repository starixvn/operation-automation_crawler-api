using Tennis;
using Tennis.Extensions;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;
// Add services to the container.

services.CreateHttpClient("AtpTour");

services.AddTransient<AtpTourCrawler, AtpTourCrawler>();
services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapControllers();

await app.RunAsync();
