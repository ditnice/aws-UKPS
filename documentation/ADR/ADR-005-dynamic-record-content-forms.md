# ADR-005: Dynamic record content forms

**Status:** Accepted

## Context

Medicine and vaccine records have dozens of content pages, each made up of questions stored
across many typed tables. Writing every page by hand, with its own endpoint, DTOs, validation and
audit, would repeat the same work many times and let the client and server rules drift apart.

The [form builder brief](../form-generation/ukps-form-builder.md) proposed defining pages as data and serving
them to a generic renderer. While building the first slice (the five Indication details pages)
some of its proposals had to change to fit the existing code. This record captures the decisions
that shape the design and the places where the code differs from the brief. The design itself is
described in [the high-level design](../form-generation/high-level-design.md).

## Decision

### Forms are defined in C# and served one page at a time

Each record type has a `FormDefinition` written with a fluent builder
(`MedicineFormDefinition`), versioned with the entities and migrations it binds to. It is built
and validated once at startup; an invalid definition stops the API from starting. One `GET`
returns a page's structure, rules, options and current answers, and one `PUT` saves every answer
on the page.

Definitions are not editable at runtime (no database or CMS authoring).

### Answers stay in the typed tables

There is no generic key/value answers table. Each question has a binding (`ColumnBinding`,
`JunctionBinding`) that reads and writes its typed column or table, built from typed expressions
so entity renames break the build. Reporting and existing queries keep working against the same
tables.

### The server is authoritative for validation

The server re-runs every rule on save; the client mirrors the rules with Zod so users see errors
straight away. Both sides trim text before validating and report only the first failing rule per
question, with the message written in the definition. A shared fixture
(`backend/tests/Application/Forms/Fixtures/rule-cases.json`) is run by xUnit and Vitest, and a
test fails if any rule kind has no cases. Checks that only make sense on the server (answer shape,
option validity, versions) are never sent to the client.

### Question IDs are a public contract

A question's ID (`medicines_product_detail.indication`) is the answer key, the payload key, the
validation error key, the frontend field name and the audit `field_path`. Released IDs never
change.

### Whole-page saves with two version checks

- `formVersion` rejects saves from a client that loaded an older definition. A test pins a
  fingerprint of each definition next to its version, so a change without a version bump fails
  the build.
- `revisionVersion` is the revision's Postgres `xmin`. It rejects saves based on stale data, and
  EF's concurrency check rejects saves that race each other.

Both return 409 with a `code` (`form_version_changed`, `revision_changed`, or
`revision_not_draft` for a revision that is no longer a draft). Only draft revisions can be
edited, by users with the new `Operation.EditRecordContent`, which Standard users have and
`Operation.Update` does not give them.

### Enum names are serialised as declared

Question types and rule kinds are sent as `"Textarea"`, `"Required"` and so on, like every other
enum in the API, rather than the lowercase names (`"textarea"`) the brief showed. The generated
TypeScript types follow.

### Regulatory dates are updated in place

The brief proposed replacing a `RegulatoryDate` row (delete, then insert) when its precision
changes. Owning tables point at these rows through nullable `*DateId` foreign keys with
`Restrict` deletes, and the date row has no reference back to its owner. The binding (when it is
built) will instead find the row through its owner's foreign key, create it and set the key if it
is missing, and update its precision, value and `IsConfidential` in place. The row keeps its ID
and no foreign keys move.

### Reference data is seeded by migration

Reference data used for options is seeded with `HasData` and EF migrations, with explicit IDs,
followed by an identity sequence reset. Rows are archived, never deleted. See
[Reference data](../form-generation/reference-data.md).

## Consequences

- Most new pages are a backend-only change: add a page method, bump the version, update tests.
  The frontend renders it with no changes.
- A new question type, rule kind or binding needs changes on both sides and new fixture cases,
  which is deliberate friction.
- Every save produces one audit event with a row per changed answer. Single values are stored
  raw, so audit can be queried directly (for example, fields moving away from `Unknown`).
- Clients must handle 409 by asking the user to reload; saves are never retried automatically.
- Changing a released question ID or removing reference data breaks history, so both are ruled
  out.
- The design relies on content rows already existing. Pages that bind to tables whose rows are
  not created by record creation need row creation (and a startup check on non-nullable
  columns) first.

## Future consideration

- **Conditions and stale answers**: pages and questions shown only when earlier answers match,
  with answers that no longer apply cleared on save. The definition, `context` field and audit
  format were shaped to allow this; it is not built.
- **Add-another groups** (including one level of nesting) and **precision dates**: planned in the
  brief, not built.
- **Runtime authoring** of definitions (database or Payload CMS) was rejected for now: definitions
  are code-reviewed and versioned with the schema they bind to.
