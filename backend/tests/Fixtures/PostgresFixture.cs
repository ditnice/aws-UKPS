using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Respawn;
using Respawn.Graph;
using Testcontainers.PostgreSql;
using UKPS.Api.Data;

namespace UKPS.Api.Tests.Fixtures;

public sealed class PostgresFixture : IAsyncLifetime, IAsyncDisposable
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder(
        "postgres:17-alpine"
    ).Build();

    private Respawner? _respawner;
    private ApiFactory? _factory;

    public ApiFactory Factory => _factory ??= new ApiFactory(_container.GetConnectionString());

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        await using AppDbContext context = CreateContext();
        await context.Database.MigrateAsync();

        DbConnection connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        _respawner = await Respawner.CreateAsync(
            connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                SchemasToInclude = ["ukps"],
                TablesToIgnore = [new Table("ukps", "__EFMigrationsHistory")],
                WithReseed = true,
            }
        );
    }

    public async Task ResetDatabaseAsync()
    {
        if (_respawner is null)
        {
            throw new InvalidOperationException(
                "The fixture has not been initialized; ResetDatabaseAsync was called before InitializeAsync completed."
            );
        }

        await using AppDbContext context = CreateContext();
        DbConnection connection = context.Database.GetDbConnection();
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    internal AppDbContext CreateContext()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                _container.GetConnectionString(),
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "ukps")
            )
            .UseSnakeCaseNamingConvention()
            .Options;

        return new AppDbContext(options);
    }

    async Task IAsyncLifetime.DisposeAsync() => await DisposeAsync();

    public async ValueTask DisposeAsync()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }

        await _container.DisposeAsync();
    }
}
