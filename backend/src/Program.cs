using System.Text.Json;
using System.Text.Json.Nodes;
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
using UKPS.Api.WebApi.Authorisation;
using UKPS.Api.WebApi.Configuration;
using UKPS.Api.WebApi.Controllers;
using UKPS.Api.WebApi.InternalServices.Authentication;
using UKPS.Api.WebApi.InternalServices.Hosting;
using UKPS.Api.WebApi.InternalServices.Identity;
using UKPS.Api.WebApi.OpenApi;
using UKPS.Api.WebApi.Validators;

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
builder.Services.AddScoped<IEmailVerificationLinkCreator, EmailVerificationLinkCreator>();
builder.Services.AddScoped<IAccessTokenProvider, HttpContextAccessTokenProvider>();
builder.Services.AddUkpsServices();
builder.Services.AddSeedingServices();
builder.Services.AddTransient<IDatabaseMigrator, DatabaseMigrator>();

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());

builder
    .BindAppOptions<CognitoOptions>()
    .BindAppOptions<DatabaseOptions>()
    .BindAppOptions<EmailOptions>()
    .BindAppOptions<HostingOptions>()
    .BindAppOptions<UserOnboardingOptions>();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    ConfigureJsonEnums(options.SerializerOptions);
});

builder
    .Services.AddControllers(options =>
    {
        options.Conventions.Add(new ProducesAuthorisationResponsesConvention());
        options.Filters.Add<SelectedOrganisationMismatchExceptionFilter>();
    })
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

                // Values are omitted for [Flags] enums by default.
                if (schema.Enum is null || schema.Enum.Count == 0)
                {
                    schema.Enum = [.. Enum.GetNames(type).Select(name => (JsonNode)name)];
                }
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

    // Describe [MaxItemLength] as the maximum length of each item of a string array.
    options.AddSchemaTransformer(
        (schema, context, cancellationToken) =>
        {
            if (schema.Properties is null)
            {
                return Task.CompletedTask;
            }

            foreach (var property in context.JsonTypeInfo.Properties)
            {
                if (
                    property
                        .AttributeProvider?.GetCustomAttributes(
                            typeof(MaxItemLengthAttribute),
                            inherit: true
                        )
                        .FirstOrDefault()
                        is MaxItemLengthAttribute maxItemLength
                    && schema.Properties.TryGetValue(property.Name, out var propertySchema)
                    && propertySchema is OpenApiSchema { Items: OpenApiSchema items }
                )
                {
                    items.MaxLength = maxItemLength.Length;
                }
            }

            return Task.CompletedTask;
        }
    );
});

builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>("database");

builder.AddAwsBearerAuthentication();

builder.Services.AddUkpsAuthorisation();

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

if (!OpenApiDocumentGeneration.IsRunning)
{
    await app.SeedData();
    await app.MigrateDatabase();
}

await app.RunAsync();

static void ConfigureJsonEnums(JsonSerializerOptions options)
{
    options.NumberHandling = JsonNumberHandling.Strict;
    options.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
}
