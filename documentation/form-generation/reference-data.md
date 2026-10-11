# Reference data

Reference data is the fixed lists that form options come from, such as BNF chapters and
therapeutic areas. This document explains how it is seeded, how to change it safely and how forms
use it. For how options are declared on a question, see
[Creating form pages](creating-form-pages.md#step-5-add-options).

## What is seeded

| Table | Entity | Rows | Notes |
|---|---|---|---|
| `bnf_chapters` | `BnfChapter` | 15 | Top-level BNF chapters 1 to 15, code = chapter number. Sections (1.1, 1.2, …) are not seeded yet |
| `therapeutic_areas` | `TherapeuticArea` | 20 | Labels include their number, as shown to users: "1: Addiction" … "20: Urologic" |

Other reference data (vaccine platforms, formulation types and so on) is not seeded yet.

## How seeding works

Reference data is seeded by **EF migrations**, so every environment gets the same rows with the
same IDs.

| Piece | Location |
|---|---|
| The lists | `backend/src/Persistence/Data/ReferenceData/BnfChapterSeedData.cs`, `TherapeuticAreaSeedData.cs` |
| `HasData(...)` calls | `backend/src/Persistence/Configurations/ReferenceData/BnfChapterConfiguration.cs`, `TherapeuticAreaConfiguration.cs` |
| Migration | `backend/src/Persistence/Migrations/20261011202235_SeedBnfChaptersAndTherapeuticAreas.cs` |

EF compares the `HasData` rows with the model snapshot and generates `InsertData`, `UpdateData` or
`DeleteData` calls for the difference. The rows are recorded in
`AppDbContextModelSnapshot.cs`, so the seed lists and the configuration calls must stay in the
code: removing them would make the next migration delete the data.

## Changing reference data

### Adding rows or changing labels

1. Edit the seed list. Give new rows the next unused ID; never re-use an ID.
2. Generate a migration from `backend/`. The design-time host needs a connection string, but
   nothing connects to a database:

   ```sh
   ConnectionStrings__DefaultConnection="Host=localhost;Database=ukps;Username=x;Password=x" \
   AWS__LoadSecrets=false \
   just generate-migration <Name>
   ```

3. If the migration inserts rows, add the identity sequence reset (below) to the end of `Up`.
4. Review the generated migration: it should contain only the rows you meant to change.

Changing a label changes what users see on forms immediately. It does not need a form version
bump, because options are loaded on each request and are not part of the definition fingerprint
(only the option query is).

### Resetting identity sequences

Seed rows are inserted with explicit IDs, and Postgres does not advance an identity column's
sequence when an ID is given. Without a reset, the next row created through the application would
be given ID 1 and fail with a duplicate key. Any migration that inserts seed rows should end with:

```csharp
migrationBuilder.Sql(
    "SELECT setval(pg_get_serial_sequence('ukps.<table>', 'id'), (SELECT MAX(id) FROM ukps.<table>));"
);
```

`SeedBnfChaptersAndTherapeuticAreas` does this for both of its tables.

### Removing rows: archive, don't delete

Never delete a reference data row, or re-use its ID for something else. Saved answers and junction
rows point at these IDs, foreign keys use `Restrict`, and audit history stores the IDs as text.

To withdraw an option, set `IsArchived = true` in the seed list and generate a migration (an
`UpdateData`). On forms:

- an archived item is no longer offered to records that have not chosen it;
- a record that already has it still sees it selected, can keep it, and can save the page;
- once a record changes away from it, it cannot be chosen again.

### Hierarchies

`BnfChapter` and `TherapeuticArea` are trees: a child row has `ParentId` set to its parent's ID.
To add BNF sections, add rows such as `(16, "1.1", "Dyspepsia and gastro-oesophageal reflux
disease")` with `ParentId = 1`. Use an authoritative source for the codes and labels.

How the current form options react to children:

- **BNF chapter** offers top-level chapters only (`ParentId == null`), so adding sections does not
  change the question. Offering sections is a definition change and needs a version bump.
- **Therapeutic area** offers leaf nodes only, labelled `Parent › Child`. Adding a child under an
  area replaces that area with its children in the list. **Records that already chose the parent
  are affected**: the parent is no longer offered, even as the saved value, so their page cannot
  be saved until the user picks a child. Before splitting an area, plan how to move existing
  answers (for example a data migration to a child), or keep the parent selectable.

## Local development

`backend/src/appsettings.Development.json` sets `Seeding:ReseedOnStartup` to `true`. Each time the
API starts it deletes the local database, re-runs every migration (including the reference data)
and re-seeds the development users and organisations. Reference data therefore always comes back;
records and answers you created do not.

## Tests

- The shared test database is emptied by Respawn before each test, so ordinary database tests do
  not see seeded rows; tests insert the reference data they need.
- `backend/tests/Persistence/ReferenceDataSeedTests.cs` creates a separate database in the test
  container, runs the migrations and checks the seeded rows and the sequence reset. Extend it when
  you seed another table.
