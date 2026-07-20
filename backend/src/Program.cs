using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using UKPS.Api.Application;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Data;
using UKPS.Api.Data.Seeding;
using UKPS.Api.WebApi.Utilities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(
            builder.Configuration.GetConnectionString("DefaultConnection"),
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "ukps")
        )
        .UseSnakeCaseNamingConvention()
);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserInfoService, WebApiCurrentUserInfoService>();
builder.Services.AddUkpsServices();
builder.Services.AddSeedingServices();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    ConfigureJsonEnums(options.SerializerOptions);
});

builder
    .Services.AddControllers()
    .AddJsonOptions(options =>
    {
        ConfigureJsonEnums(options.JsonSerializerOptions);
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddSchemaTransformer(
        (schema, context, cancellationToken) =>
        {
            var type =
                Nullable.GetUnderlyingType(context.JsonTypeInfo.Type) ?? context.JsonTypeInfo.Type;

            if (type.IsEnum)
            {
                schema.Type = JsonSchemaType.String;
            }

            return Task.CompletedTask;
        }
    );
});

builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

var isOpenApiGeneration = Environment.CommandLine.Contains(
    "getdocument",
    StringComparison.OrdinalIgnoreCase
);
if (!isOpenApiGeneration)
{
    await app.MigrateDatabase();
    await app.SeedData();
}

await app.RunAsync();

static void ConfigureJsonEnums(JsonSerializerOptions options)
{
    options.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
}
