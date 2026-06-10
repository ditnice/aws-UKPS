# UKPS.Data — Entity Framework Core

EF Core 9 data layer for UK PharmaScan (UKPS), targeting PostgreSQL via Npgsql.

## Project structure

```
UKPS.Data/
├── Enums/
│   └── Enums.cs                          All domain enums
├── Entities/
│   ├── Identity/                         Organisation, User, UserOrgMembership, audit tables
│   ├── RecordWorkflow/                   Record, RecordRevision, QaReview, RecordEvent, etc.
│   ├── SharedRevisionContent/            RegulatoryDate, RecordHta, RecordClinicalTrial, etc.
│   ├── MedicinesRevisionContent/         All medicines-specific section sub-tables
│   ├── VaccinesRevisionContent/          All vaccines-specific section sub-tables
│   ├── ReferenceData/                    Lookup tables + BnfChapter + TherapeuticArea
│   ├── Reporting/                        ReportPreset, ReportAudit, EmailTemplate, EmailAudit
│   └── UserFeatures/                     RecordWatchlist
├── Configurations/
│   ├── Identity/                         Fluent API configs for identity entities
│   ├── RecordWorkflow/                   Fluent API configs for core workflow entities
│   ├── SharedRevisionContent/            Fluent API configs for shared revision content
│   ├── MedicinesRevisionContent/         Fluent API configs for medicines entities
│   ├── VaccinesRevisionContent/          Fluent API configs for vaccines entities
│   ├── ReferenceData/                    Fluent API configs for reference data
│   └── Reporting/                        Fluent API configs for reporting, email, user features
├── UkpsDbContext.cs                      DbContext with all DbSets
├── ServiceCollectionExtensions.cs        DI registration with Npgsql + snake_case
└── appsettings.example.json              Example connection string
```

## Key conventions

### Naming
- All tables live in the `ukps` PostgreSQL schema
- `app_user` is used instead of `user` to avoid the PostgreSQL reserved word
- snake_case naming convention applied via `.UseSnakeCaseNamingConvention()`

### Enums
- All domain enums stored as `string` (PostgreSQL varchar) via `.HasConversion<string>()`
- Exception: `PharmaceuticalEntity` is a `[Flags]` enum stored as `integer` via
  `.HasConversion<int>()` — Medicines=1, Vaccines=2, Both=3

### Circular references
`Record` ↔ `RecordRevision` is a circular relationship:
- `Record.PublishedRevisionId` and `Record.CurrentDraftRevisionId` are both nullable
- `RecordRevision.RecordId` is non-nullable
- Both sides use `OnDelete(DeleteBehavior.Restrict)`
- Insert order: Record (with null revision FKs) → RecordRevision → update Record FKs

### Delete behaviour
All FK relationships use `OnDelete(DeleteBehavior.Restrict)` to prevent accidental
cascades, since historical data must be preserved.

## Adding migrations

Always use descriptive names:

```bash
# Good
dotnet ef migrations add AddUserEmailIndex
dotnet ef migrations add AddVaccinesDiseaseDetailTable
dotnet ef migrations add ChangeWorkflowStatusToString

# Avoid
dotnet ef migrations add Migration20260608
dotnet ef migrations add Update
```

Apply migrations:
```bash
dotnet ef database update
```

## Connection string

See `appsettings.example.json`. Connection pooling is configured via the connection string:
- `Pooling=true`
- `Minimum Pool Size=1`
- `Maximum Pool Size=20`
- `Connection Idle Lifetime=300` (seconds before idle connections are closed)
