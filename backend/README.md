## Testing

Running the test suite (`dotnet test` or `just test`) requires a running Docker daemon. Tests that touch the database start a disposable `postgres:17-alpine` container via Testcontainers, apply the EF Core migrations once at startup, and use Respawn to reset the database between tests so each test runs against a clean, migrated schema. The first run pulls the Postgres image, which may take a few minutes; subsequent runs reuse the cached image and start quickly.

