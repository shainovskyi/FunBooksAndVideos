using System.Text.Json.Serialization;
using FunBooksAndVideos.Api.Middleware;
using FunBooksAndVideos.Application;
using FunBooksAndVideos.Infrastructure;
using FunBooksAndVideos.Infrastructure.Persistence;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("serilog.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"serilog.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services);
});

var app = builder.Build();

await app.Services.GetRequiredService<DatabaseInitializer>().InitializeAsync();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "FunBooksAndVideos API v1");
    });
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Exposes the implicit Program class to WebApplicationFactory in integration tests.
public partial class Program { }
