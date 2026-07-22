## Directory Structure

The directory structure should match the following

├── src
│   ├── Application (Organised by domain)
│   │   ├── Common
│   │   ├── InternalServices (Application services for internal use)
│   │   │   ├── Authorisation
│   │   │   └── Identity
│   │   ├── Organisations
│   │   │   ├── Dtos
│   │   │   └── Errors
│   │   └── Users
│   │       ├── Dtos
│   │       └── Errors
│   ├── Persistence
│   │   ├── Configurations
│   │   │   ├── Email
│   │   │   ├── Identity
│   │   │   ├── MedicinesRevisionContent
│   │   │   ├── RecordWorkflow
│   │   │   ├── ReferenceData
│   │   │   ├── Reporting
│   │   │   ├── SharedRevisionContent
│   │   │   ├── UserFeatures
│   │   │   └── VaccinesRevisionContent
│   │   ├── Data
│   │   │   ├── Fakers
│   │   │   └── Seeding
│   │   ├── Entities
│   │   │   ├── Email
│   │   │   ├── Identity
│   │   │   ├── MedicinesRevisionContent
│   │   │   ├── RecordWorkflow
│   │   │   ├── ReferenceData
│   │   │   ├── Reporting
│   │   │   ├── SharedRevisionContent
│   │   │   ├── UserFeatures
│   │   │   └── VaccinesRevisionContent
│   │   ├── Enums
│   │   └── Migrations
│   ├── WebApi
│   │   ├── Controllers
│   │   └── InternalServices
│   │       └── Identity
│   └── Properties
└── tests
    ├── Application
    │   ├── Common
    │   ├── Organisations
    │   └── Users
    ├── Integration
    ├── Persistence
    ├── TestResults
    │   └── Coverage
    ├── Utilities
    │   ├── AssertionHelpers
    │   ├── Data
    │   ├── Fixtures
    │   └── Harnesses
    └── WebApi
        ├── Controllers
        └── Utilities


## Seeding the Database on Startup

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
