using GameStore.Api.Data;
using GameStore.Api.Endpoints;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

#if ASPIRE
builder.AddServiceDefaults();
#endif

#if ASPIRE
builder.AddNpgsqlDbContext<GameStoreContext>("GameStore");
#else
builder.Services.AddDbContextPool<GameStoreContext>(options =>
{
    var cs = builder.Configuration.GetConnectionString("GameStore");
    options.UseNpgsql(cs);
});
#endif

#if ASPIRE
builder.AddRedisDistributedCache("redis");
#else
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration =
        builder.Configuration.GetConnectionString("Redis");
});
#endif

#if !ASPIRE
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetSampler(new AlwaysOnSampler())
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()

            // 👇 DEBUG CRÍTICO
            .AddConsoleExporter()

            // 👇 Exporter real
            .AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://otel-collector:4317");
                options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
            });
    });
#endif


var app = builder.Build();

#if ASPIRE
app.MapDefaultEndpoints();
#endif

app.MapGamesEndpoints();
app.MapGenresEndpoints();

await app.MigrateDbAsync();

app.Run();
