# Database ERD

`ukps-erd.mmd` is a Mermaid entity-relationship diagram of the UKPS PostgreSQL schema and
`ukps-erd.html` is an interactive viewer of the same data. Both are **generated** and must not be
edited by hand.

| File | Maintained by | Purpose |
| --- | --- | --- |
| `ukps-erd.mmd` | Generator (do not edit) | Mermaid `erDiagram` of every table, column, key and foreign key |
| `ukps-erd.html` | Generator (do not edit) | Interactive viewer: filter groups, focus on a table, hide columns or busy relationships |
| `erd-groups.yml` | Developers | Assigns each table to a logical group (Identity, Record Workflow, Reference Data, ...) |

## Source of truth

The schema structure (tables, columns, PostgreSQL types, nullability, primary keys, foreign keys
and cardinality) comes from the Entity Framework Core model in `AppDbContext`, read through EF's
relational model so Fluent API configuration, the snake_case naming convention and JSON-mapped
complex types are all reflected. No database connection is needed.

`erd-groups.yml` adds the human-maintained architectural categorisation. Groups are rendered as
coloured subgraphs in the diagram.

## Regenerating

```bash
./scripts/generate-erd.sh          # or: just erd
./scripts/generate-erd.sh --check  # exit 1 if the committed ERD is stale; writes nothing
```

The generator lives in `backend/tools/UKPS.ErdGenerator` and is a build-time tool only; the API
does not reference it. The first run builds the backend, subsequent runs take a couple of seconds.

If the EF model contains changes that have not yet been captured in a migration, the generator
prints a warning: the ERD always reflects the model, so add the migration as well.

## Adding or renaming a table

1. Add or change the EF entity and its `IEntityTypeConfiguration<>`.
2. Create the migration (`just generate-migration <Name>` in `backend/`).
3. Add the table to the appropriate group in `erd-groups.yml`.
4. Run `./scripts/generate-erd.sh`.
5. Commit the entity, migration, `erd-groups.yml`, `ukps-erd.mmd` and `ukps-erd.html` together.

Generation fails with a clear message if a table in the model is not in any group, if a grouped
table no longer exists, or if a table appears in more than one group. This is deliberate: every
new table needs an explicit architectural home.

```text
ERD generation failed.

The following tables are not assigned to an ERD group:
  - report_exports
Add each table to a group in documentation/database/erd-groups.yml and re-run the generator.
```

## Pre-commit hook

The repository uses [pre-commit](https://pre-commit.com). Enable the hooks once per clone:

```bash
pre-commit install
```

The `backend-erd` hook regenerates the ERD whenever files under `backend/src/Persistence/`,
`erd-groups.yml` or the generator change. If the generated files differ from what is staged,
pre-commit reports that the hook modified files; stage them and commit again.

## Viewing the diagram

The quickest way to explore the schema is to open `ukps-erd.html` in a browser. It loads Mermaid
and the classless Pico CSS stylesheet from a CDN, so it needs an internet connection; there is no
other styling to maintain. The full diagram is busy because `record_revisions`,
`app_user` and `regulatory_dates` are referenced from many tables; the viewer lets you:

- show only some groups, with tables from other groups drawn as dashed stubs where referenced;
- focus on one table and its direct (or two-hop) relations;
- switch column detail between all columns, keys only, or table names only;
- hide the incoming relationship lines of the most referenced tables (the FK markers stay);
- flip the layout direction, download the current view as SVG, or copy its Mermaid source.

For the static `.mmd` file:

- GitHub renders Mermaid inside Markdown code fences; the raw `.mmd` file is best viewed with the
  VS Code *Markdown Preview Mermaid Support* extension, [mermaid.live](https://mermaid.live), or
  rendered locally:

  ```bash
  npx -y @mermaid-js/mermaid-cli -i documentation/database/ukps-erd.mmd -o ukps-erd.svg
  ```

- Subgraphs in `erDiagram` require Mermaid 11.17 or later.

## Reading the diagram

- Column lines are `type name PK|FK|UK "nullable"`. Multi-word PostgreSQL types are shortened
  (for example `timestamp with time zone` is `timestamptz`).
- Relationship lines run from the principal (parent) table to the dependent (child) table and are
  labelled with the foreign key column. `||` means exactly one, `|o` zero or one, `o{` zero or
  many. A solid line is an identifying relationship (the FK is part of the child's primary key).
