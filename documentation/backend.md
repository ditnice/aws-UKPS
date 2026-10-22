## Directory Structure

The directory structure should match the following

```
├── src
│ ├── Application (Organised by domain)
│ │ ├── Common
│ │ ├── InternalServices (Application services for internal use)
│ │ │ ├── Authorisation
│ │ │ └── Identity
│ │ ├── Organisations
│ │ │ ├── Dtos
│ │ │ └── Errors
│ │ └── Users
│ │ ├── Dtos
│ │ └── Errors
│ ├── Persistence
│ │ ├── Configurations
│ │ │ ├── Email
│ │ │ ├── Identity
│ │ │ ├── MedicinesRevisionContent
│ │ │ ├── RecordWorkflow
│ │ │ ├── ReferenceData
│ │ │ ├── Reporting
│ │ │ ├── SharedRevisionContent
│ │ │ ├── UserFeatures
│ │ │ └── VaccinesRevisionContent
│ │ ├── Data
│ │ │ ├── Fakers
│ │ │ └── Seeding
│ │ ├── Entities
│ │ │ ├── Email
│ │ │ ├── Identity
│ │ │ ├── MedicinesRevisionContent
│ │ │ ├── RecordWorkflow
│ │ │ ├── ReferenceData
│ │ │ ├── Reporting
│ │ │ ├── SharedRevisionContent
│ │ │ ├── UserFeatures
│ │ │ └── VaccinesRevisionContent
│ │ ├── Enums
│ │ └── Migrations
│ ├── WebApi
│ │ ├── Controllers
│ │ └── InternalServices
│ │ └── Identity
│ └── Properties
└── tests
  ├── Application
  │ ├── Common
  │ ├── Organisations
  │ └── Users
  ├── Integration
  ├── Persistence
  ├── TestResults
  │ └── Coverage
  ├── Utilities
  │ ├── AssertionHelpers
  │ ├── Data
  │ ├── Fixtures
  │ └── Harnesses
  └── WebApi
  ├── Controllers
  └── Utilities
```

## Configuration

`backend/src/appsettings.json` and `backend/src/appsettings.Example.json` list
every writable property of the backend's configuration options, plus the
logging, connection-string and AWS settings used by this application. They are
not an inventory of every setting supported by ASP.NET Core or the AWS SDK.
Tests check the setting names in both files against the options types to prevent
missing or ignored application settings. They do not validate that configuration
values can be bound to those types.

For local development, copy `appsettings.Example.json` to the ignored
`appsettings.Development.json` alongside it, then replace placeholders as needed.
The example is not loaded automatically. Environment variables use `__` between
section and property names (for example, `Database__Username`) and override JSON
settings. Never commit real credentials. The example enables super-user
authentication bypass and database reseeding; use it only for local development
against a disposable database.

### Settings inventory

| Section | Settings | Behaviour |
| --- | --- | --- |
| `Logging:LogLevel` | `Default`, `Microsoft.AspNetCore` | Default log levels are `Information` and `Warning`. Additional category overrides are supported by ASP.NET Core. |
| `ConnectionStrings` | `DefaultConnection` | A non-empty value takes precedence over all structured database connection settings. The example uses local PostgreSQL. |
| `AWS` | `LoadSecrets`, `Region` | `LoadSecrets` defaults to `false`; AWS clients use `Region`, set to `eu-west-2` in both files and overridden by the task environment. A valid region default is required for OpenAPI generation during builds. The example enables secret loading and requires AWS credentials and permission to read the development Cognito secret. |
| `Cognito` | `ServiceUrlOverride`, `Region`, `UserPoolId`, `ClientId`, `ClientSecret` | The override is optional. The JWT authority is derived from the service URL and pool ID. Supply the client secret externally. |
| `Database` | `MigrateOnStartup`, `Host`, `Name`, `Password`, `Port`, `RootCertificate`, `Username` | Startup migration defaults to `false`. Without a connection string, host, name, credentials and root certificate are required; port defaults to `5432`. Structured connections always use TLS `VerifyFull`. |
| `DevAuthentication` | `IsEnabled`, `UserRole` | Disabled by default. When enabled, requests are authenticated using `UserRole`, which defaults to `Super`. |
| `Email` | `MaxResendSignUpLinkAttempts`, `ListenerEnabled`, `TestingEmailAddress`, `FromAddress`, `QueueUrl`, `QueueMaxNumberOfMessages`, `QueueWaitTimeInSeconds` | Resend limit defaults to `3` in the code and both appsettings files; dev/test ECS overrides it with `"3"`. `TestingEmailAddress` defaults to `null`; when set, it **adds** a recipient, rather than replacing the intended recipient. `ListenerEnabled` defaults to `true`; set it to `false` to stop the SQS email queue listener polling (used by the Docker Compose E2E stack, which has no AWS access). Sender and queue URL are required. Queue batch size defaults to `1` (range `1–10`); wait time defaults to `20` seconds (range `0–20`). |
| `Seeding` | `ReseedOnStartup`, `SeedUsersJson` | Reseeding defaults to `false`; the configured user list defaults to `[]`. The example enables reseeding and includes a synthetic super user. |
| `UserOnboarding` | `SetupLink`, `SetupTokenExpiryTimeSeconds` | Replace the link placeholder with the frontend base URL. Token expiry defaults to `900` seconds and must be at least `60`. |

### Database connection modes and secrets

- **Local PostgreSQL:** retain `ConnectionStrings:DefaultConnection` from the
  example. Structured connection settings, including `RootCertificate`, are
  ignored. `Database:MigrateOnStartup` still controls startup migration.
- **Structured/Aurora connection:** unset the connection string and provide
  `Database:Host`, `Name`, `Username`, `Password` and `RootCertificate`.
  `RootCertificate` must point to an accessible CA bundle. The backend container
  supplies `/app/certs/eu-west-2-bundle.pem` through `Database__RootCertificate`;
  when running outside that container, supply your own accessible bundle path.

`AWS:LoadSecrets` loads only the Cognito client secret from the development
Secrets Manager reference configured in `AwsConfigurationExtensions`. It does
not load database credentials. Deployed ECS tasks receive Cognito and database
credentials through environment-variable secret injection. The database
credential placeholders in the example are unused until the connection string
is removed and must then be replaced through an external configuration source.

AWS SQS and SES clients use the shared `AWS` configuration. `Email:Region`,
`Email:BaseDomain`, `Email:ReplyToAddress` and `Email:ConfigurationSetName` are
not consumed by the current backend and are omitted from its task definitions.
Adding them to appsettings does not enable those behaviours.

The dev and test backend task definitions explicitly set the resend limit to
`3` and the development authentication role to `Super`; development authentication
remains disabled by the base appsettings. `Email:TestingEmailAddress` stays unset
rather than passing the string `"null"` as an environment variable. The root
certificate path remains supplied by the backend Dockerfile rather than duplicated
in the task definitions.

`Email:MaxResendSignUpLinkAttempts` retains a numeric default of `3` so options
can bind during local builds and OpenAPI generation without ECS environment
variables. The task definition's `Email__MaxResendSignUpLinkAttempts` overrides
this default when deployed. Do not replace numeric settings with string
placeholders, as they cannot bind to integer options.

### DevAuthentication

The `DevAuthentication` section controls the development authentication bypass.

When `IsEnabled` is set to `true`, authentication challenges are bypassed and requests are automatically authenticated using `DevAuthentication:UserRole`, which defaults to **Super**. This is intended for local development and testing only.

```json
  "DevAuthentication":{
    "IsEnabled": true
  }
```

**Do not enable this setting in production environments**, as it effectively grants unrestricted access without requiring authentication.

### Seeding the Database on Startup

The API can automatically seed the database with development data when it starts.

To enable this, set the `Seeding:ReseedOnStartup` configuration value to `true` in your `appsettings.json` (or another configuration source).

```json
{
  "Seeding": {
    "ReseedOnStartup": true
  }
}
```

When `ReseedOnStartup` is set to `true`, the application will reseed the database each time the API starts. This is intended for local development and testing, allowing developers to begin with a known dataset.

> **Warning**
>
> Reseeding will remove any existing seeded data before recreating it. Do **not** enable this setting in production or in environments where data must be preserved.

After the database has been reseeded, it is recommended to set `ReseedOnStartup` back to `false` to prevent the database from being reseated on every application startup.

```json
{
  "Seeding": {
    "ReseedOnStartup": false
  }
}
```

## Development

### Certificates

For local development, to use HTTPS you need to install certificates locally on you machine. To do this do the following:

1. Ensure the the 'certutil' is installed on your machine

```
sudo apt update
sudo apt install libnss3-tools
```

2. Clean and trust asp net core dev certificates

```
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

3. Check the certificate and been installed and trusted

```
dotnet dev-certs https --check --trust
```

The frontend also needs to run over HTTPS to work with these cookies locally — see
[`frontend.md#certificates`](./frontend.md#certificates) for the remaining setup.
