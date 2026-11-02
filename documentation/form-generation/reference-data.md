# Reference data

Reference data is the fixed lists that form options come from, such as BNF chapters and
therapeutic areas. This document explains where that data comes from today, and how the
proposed form [option providers](creating-form-pages.md#step-5-add-options) will use it.

> The seeding described here **exists today**. The option providers are part of the
> **proposed** attribute-driven form design; see the [high-level design](high-level-design.md).

## Where the data comes from

Reference data is **not** seeded by EF migrations. It is loaded by the **synthetic data seeder**,
along with the development organisations, users and records.

| Piece | Location |
|---|---|
| Source data (reference lists plus 500 synthetic medicine records) | `backend/src/Persistence/Data/Seeding/SyntheticData/synthetic-medicine-records.json.gz` |
| Shape of the reference lists | `SyntheticData/SyntheticReferenceData.cs` |
| Builds the entities (hierarchies linked by `ParentCode`) | `SyntheticData/SeedReferenceData.cs` |
| Writes everything | `Seeding/SeedDataWriter.cs` (deletes the database, runs migrations, inserts) |
| Trigger | `Seeding/WebApplicationSeedingExtensions.cs`, when `Seeding:ReseedOnStartup` is `true` |

`ReseedOnStartup` is `true` in `appsettings.Development.json` and `appsettings.Example.json`, and
`false` in `appsettings.json`. Each local start therefore:
- deletes the database;
- re-runs every migration;
- reloads the reference data and synthetic records.

Records and answers you created yourself do not survive a restart.

IDs come from identity columns, in the order the seeder inserts rows. They are stable between
local runs of the same data file, but they are **not** a contract: never hard-code them.

## What is loaded

| Table | Entity | Rows | Shape |
|---|---|---|---|
| `bnf_chapters` | `BnfChapter` | 92 | 15 top-level chapters (`Code` `"1"`…`"15"`), the rest are sections (`"1.3"`, `"2.5"`, …) with `ParentId` set |
| `therapeutic_areas` | `TherapeuticArea` | 108 | 20 top-level areas (`Code` `"1"`…`"20"`, `"Addiction"`, …), the rest are child areas (`"1.1"`, `"Opioid use disorder"`, …) with `ParentId` set |
| `formulation_type`, `mhra_procedure_type`, `irp_reference_regulator`, `irp_route`, `atmp_classification`, `patient_pathway_point`, `uk_patient_population_range` | | varies | Flat lists |

`BnfChapter` and `TherapeuticArea` both store the code in `Code`, and the label is the name
alone. (`TherapeuticArea.Code` was added by the `AddRecordContentForms` migration; the seeder
fills it from the source file, where it also links parents and children.)

### How the synthetic records use the trees

This matters for which options a form offers, because a saved value must be offered or the page
cannot be saved unchanged. Both questions use a two-level `HierarchicalSelect` whose options
include children, so these saved values are offered and load into their parent and child pair.

- **BNF:** all 500 synthetic records reference a **section** (for example `1.3`), never a
  top-level chapter.
- **Therapeutic areas:** every one of the 522 selections is a **child** (leaf) area, never a
  top-level one.

## How the form uses it: option providers

A question uses the provider registered for its referenced entity, or the one named by
`[FormOptions(key)]` when several are registered. A reference data provider
is an EF query and a projection to `ReferenceOptionRow(Id, Label, IsArchived)`, run on every
request, so changes to rows show up without a restart. The providers are:

| Key | Kind | Query | Label |
|---|---|---|---|
| `bnf-chapters` | Tree | every row, ordered by `DisplayOrder` | `"{Code}: {Label}"`, e.g. `"1: Gastro-Intestinal System"` and `"1.3: Antisecretory drugs and mucosal protectants"` |
| `therapeutic-areas` | Tree | every row, ordered by `DisplayOrder` | `"{Code}: {Label}"`, e.g. `"1: Addiction"` and `"1.1: Opioid use disorder"` |

A tree provider groups rows by `ParentId` into parents with children; rows more than two levels
deep are an error. The answer is the most specific choice, so a value can be a parent or a child
ID. Values on the wire are reference IDs as strings (`"12"`). Audit rows store the ID, not the
label.

### Archived items

`IsArchived` withdraws an option without deleting it:

- an archived item is no longer offered to records that have not chosen it;
- a record that already has it still sees it selected, can keep it, and can save the page;
- once a record changes away from it, it cannot be chosen again.

Never delete a reference data row, or re-use its ID for something else. Saved answers and
junction rows point at these IDs, foreign keys use `Restrict`, and audit history stores the IDs
as text.

### Hierarchies and changing the tree

- Both trees offer parents and children, and either can be the answer, so adding children
  under an existing item does not invalidate saved answers.
- Only two levels are supported. Adding a third level makes the provider throw when the page is
  loaded; that would need a provider and control change.
- If an archived parent has a saved child, the parent is still shown (with only its saved
  children) so the saved answer can be displayed.

Changing a label changes what users see on forms immediately. It does not need a form version
bump: rows are not part of the fingerprint, only the provider's query is.

## Open questions

- **Environments other than Development.** Nothing loads reference data where
  `ReseedOnStartup` is `false`. Before forms are used outside local development, reference data
  needs a production path. That could be migration `HasData` seeding with fixed IDs (plus an
  identity sequence reset), or a separate load step.

## Tests

- The shared test database is emptied by Respawn before each test, so database tests do not
  see seeded rows. Tests insert the reference data they need.
- `RecordFormServiceTests` inserts a small BNF and therapeutic area tree and checks the tree
  shape, the labels, and archived handling (hidden unless saved, shown when saved).
