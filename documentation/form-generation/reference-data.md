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
| `therapeutic_areas` | `TherapeuticArea` | 108 | 20 top-level areas (`"Addiction"`, …), the rest are child areas (`"Opioid use disorder"`, …) with `ParentId` set |
| `formulation_type`, `mhra_procedure_type`, `irp_reference_regulator`, `irp_route`, `atmp_classification`, `patient_pathway_point`, `uk_patient_population_range` | | varies | Flat lists |

`BnfChapter` stores the code in `Code`, and the label is the name alone. `TherapeuticArea` has no
code column: the code exists only in the source file, to link parents and children.

### How the synthetic records use the trees

This matters for which options a form offers, because a saved value must be offered or the page
cannot be saved unchanged.

- **BNF:** all 500 synthetic records reference a **section** (for example `1.3`), never a
  top-level chapter.
- **Therapeutic areas:** every one of the 522 selections is a **child** (leaf) area, never a
  top-level one.

## How the form uses it: option providers

Questions refer to a named option provider with `[FormOptions(key)]`. A reference data provider
is an EF query and a projection to `ReferenceOptionRow(Id, Label, IsArchived)`, run on every
request, so changes to rows show up without a restart. The proposed providers are:

| Key | Query | Label | Notes |
|---|---|---|---|
| `bnf-top-level` | `ParentId == null`, ordered by `DisplayOrder` | `"{Code}: {Label}"`, e.g. `"1: Gastro-Intestinal System"` | Matches the BNF chapter screen design. **See the open question below** |
| `therapeutic-areas` | leaves (`!Children.Any()`), ordered by parent then own `DisplayOrder` | `"Parent › Child"`, e.g. `"Addiction › Opioid use disorder"` | Matches how the synthetic records use the tree |

Values on the wire are reference IDs as strings (`"12"`). Audit rows store the ID, not the
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

- **BNF chapter**: offering only top-level chapters means that adding sections does not change
  the question. Offering sections is a provider query change, which changes the form
  fingerprint and needs a version bump.
- **Therapeutic area**: offers leaves only. Adding a child under a leaf replaces that leaf with
  its children in the list. Records that already chose the former leaf no longer see it offered,
  so their page cannot be saved until the user picks a child. Before splitting an area, plan how
  to move existing answers, or keep the parent selectable.

Changing a label changes what users see on forms immediately. It does not need a form version
bump: rows are not part of the fingerprint, only the provider's query is.

## Open questions

- **BNF top-level vs sections.** The design screen asks for a BNF *chapter*, but the synthetic
  data stores sections. With a `bnf-top-level` provider, every seeded record's saved BNF value
  would not be offered, and saving that page would fail with "Select a valid option" until the
  user picks a chapter. The options are:
  - offer sections, labelled `"1.3: Antisecretory drugs…"`, or grouped by chapter;
  - always offer the saved value, even when the query filters it out (as archived items are
    today);
  - change the synthetic data to store chapters.
- **Environments other than Development.** Nothing loads reference data where
  `ReseedOnStartup` is `false`. Before forms are used outside local development, reference data
  needs a production path. That could be migration `HasData` seeding with fixed IDs (plus an
  identity sequence reset), or a separate load step.

## Tests

- The shared test database is emptied by Respawn before each test, so database tests do not
  see seeded rows. Tests insert the reference data they need.
- Option provider tests should insert a small tree for each provider and check:
  - the filter (top-level or leaves);
  - the ordering;
  - the labels;
  - archived handling: hidden unless saved, shown when saved.
