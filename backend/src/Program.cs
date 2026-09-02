using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using UKPS.Api.Application;
using UKPS.Api.Application.Authentication;
using UKPS.Api.Application.InternalServices.Communication;
using UKPS.Api.Application.InternalServices.Hosting;
using UKPS.Api.Application.InternalServices.Identity;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Data.Seeding;
using UKPS.Api.WebApi;
using UKPS.Api.WebApi.InternalServices.Hosting;
using UKPS.Api.WebApi.InternalServices.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.ConfigureAwsSecrets();

builder.Services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(
            DatabaseConnectionStringFactory.GetConnectionString(builder.Configuration),
            npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "ukps")
        )
        .UseSnakeCaseNamingConvention()
);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserInfoService, WebApiCurrentUserInfoService>();
builder.Services.AddScoped<ISetupLinkCreator, SetupLinkCreator>();
builder.Services.AddUkpsServices();
builder.Services.AddSeedingServices();
builder.Services.AddTransient<IDatabaseMigrator, DatabaseMigrator>();

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());

builder.Services.Configure<CognitoOptions>(
    builder.Configuration.GetSection(CognitoOptions.SectionName)
);
builder.Services.Configure<DatabaseOptions>(
    builder.Configuration.GetSection(DatabaseOptions.SectionName)
);

builder
    .Services.AddOptions<EmailOptions>()
    .Bind(builder.Configuration.GetSection(EmailOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder
    .Services.AddOptions<UserOnboardingOptions>()
    .Bind(builder.Configuration.GetSection(UserOnboardingOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

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

// Add CORS to allow only http://localhost:3000
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy
            .WithOrigins("http://localhost:3000", "https://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders("WWW-Authenticate")
    )
);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    // Ensure OpenAPI schemas represent enums as strings and mark non-nullable properties as required.
    options.AddSchemaTransformer(
        (schema, context, cancellationToken) =>
        {
            var type =
                Nullable.GetUnderlyingType(context.JsonTypeInfo.Type) ?? context.JsonTypeInfo.Type;

            if (type.IsEnum)
            {
                schema.Type = JsonSchemaType.String;
            }

            if (schema.Properties is not null)
            {
                foreach (var property in context.JsonTypeInfo.Properties)
                {
                    if (!property.IsGetNullable && schema.Properties.ContainsKey(property.Name))
                    {
                        (schema.Required ??= new HashSet<string>(StringComparer.Ordinal)).Add(
                            property.Name
                        );
                    }
                }
            }

            return Task.CompletedTask;
        }
    );
});

builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database");

builder.AddAwsBearerAuthentication();

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Use CORS middleware
app.UseCors();

app.UseAuthentication();
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
    options.NumberHandling = JsonNumberHandling.Strict;
    options.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
}
