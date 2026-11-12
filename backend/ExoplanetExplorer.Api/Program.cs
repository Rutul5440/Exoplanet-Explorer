using System.Net;
using ExoplanetExplorer.Api.Configuration;
using ExoplanetExplorer.Api.Data;
using ExoplanetExplorer.Api.Endpoints;
using ExoplanetExplorer.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<SyncOptions>()
    .Bind(builder.Configuration.GetSection(SyncOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var connectionString = builder.Configuration.GetConnectionString("ExoplanetDb")
    ?? throw new InvalidOperationException("Connection string 'ExoplanetDb' is required.");

builder.Services.AddDbContext<ExoplanetDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure(3)));

builder.Services.AddMemoryCache(options => options.SizeLimit = 512);
builder.Services.AddSingleton<CatalogCacheInvalidator>();
builder.Services.AddSingleton<SyncCoordinator>();

builder.Services.AddHttpClient<INasaTapClient, NasaTapClient>((sp, client) =>
{
    var syncOptions = sp.GetRequiredService<IOptions<SyncOptions>>().Value;
    client.Timeout = TimeSpan.FromSeconds(syncOptions.SyncTimeoutSeconds);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("ExoplanetExplorer/1.0");
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
});

builder.Services.AddScoped<ExoplanetSyncService>();
builder.Services.AddScoped<ExoplanetQueryService>();
builder.Services.AddHostedService<NightlySyncWorker>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
        }
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("FrontendPolicy");

app.MapExoplanetEndpoints();
app.MapSyncEndpoints();

app.Run();

/// <summary>
/// Entry point marker for WebApplicationFactory integration tests.
/// </summary>
public partial class Program;
