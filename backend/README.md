## Testing

Running the test suite (`dotnet test` or `just test`) requires a running Docker daemon. Tests that touch the database start a disposable `postgres:17-alpine` container via Testcontainers, apply the EF Core migrations once at startup, and use Respawn to reset the database between tests so each test runs against a clean, migrated schema. The first run pulls the Postgres image, which may take a few minutes; subsequent runs reuse the cached image and start quickly.

## CI checks

The `Backend checks` GitHub Actions workflow runs on every pull request, pushes to `main`, and manual dispatch. It builds `UKPS.Api.slnx` (including regenerating OpenAPI) with analyzer and code-style enforcement, runs the API test suite with coverage, checks CSharpier formatting, and regenerates the frontend API client. The separate migration test project is not part of this solution.

Prerequisites: .NET 10 SDK, Docker, and `just`. API client generation also needs Node.js 24 and pnpm 12. CI uses .NET SDK `10.0.404` and pnpm `12.13.0`, matching the Dockerfiles.

Run the backend checks locally from `backend`:

```sh
dotnet tool restore --tool-manifest dotnet-tools.json
dotnet restore UKPS.Api.slnx
dotnet build UKPS.Api.slnx --configuration Release --no-restore -p:EnforceCodeStyle=true
just coverage
dotnet csharpier check .
```

Coverage uses Coverlet's Microsoft.Testing.Platform integration, matching the runner in the root `global.json`. `just coverage` enforces at least 80% total line and branch coverage of `UKPS.Api`, excluding `Program.cs` and generated `obj` sources. Cobertura reports are written to `tests/TestResults/Coverage`; ReportGenerator writes HTML to `tests/TestResults/CoverageReport`. CI uploads available reports even when the test/coverage check fails.

`just coverage` clears previous reports and rebuilds/tests in the default Debug configuration; CI instead tests the Release build with `--no-build` and requires at least one test. If the local test/coverage command fails, the recipe stops before generating HTML. Run `dotnet tool run reportgenerator` from `backend` to generate HTML from any coverage XML that was produced.

As in pre-commit, the build checks `.editorconfig` code-style rules and analyzers rather than running a separate `dotnet format` check; CSharpier checks layout. Use `just fix-style` and `just format` to apply fixes locally.

To check the generated API files, run from the repository root:

```sh
pnpm --dir frontend install --frozen-lockfile
just regenerate
git status --porcelain --untracked-files=all -- backend/openapi/UKPS.Api.json frontend/src/client/generated
```

The final command should produce no output: CI rejects staged or unstaged changes, deletions, and newly generated untracked files under these paths. Locally, commit the regenerated files and rerun the check for a clean result. No running frontend, database service, or AWS credentials are required for generation.
