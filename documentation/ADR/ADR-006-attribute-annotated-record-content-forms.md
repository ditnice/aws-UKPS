# ADR-006: Attribute-annotated entities as the record content form definition

**Status:** Proposed

## Context

Record content (the questions a company answers about a medicine or vaccine) is spread over
dozens of typed EF content tables, such as `medicines_indication_details` and its junction
tables. Forms over this content need a page-by-page UI, client and server validation with the
same messages, field-level audit, and concurrency protection. Hand-building each page would mean
writing a page, a DTO, an endpoint and validation for every group of questions.

The [form builder brief](../form-generation/ukps-form-builder.md) settled the runtime model:

- the backend serves one page at a time as JSON;
- the frontend renders it generically;
- one endpoint validates, saves through **bindings** to typed columns, audits and returns the
  next page;
- question IDs are a public contract (payload keys, error keys and audit field paths).

That brief proposed describing each form in a **separate C# definition**: a fluent builder that
lists sections, pages and questions, with each question bound to an entity property by an
expression.

Our concern with a separate definition is that it duplicates the entity model:

- each column is described in two places (the entity and the form), which can drift;
- nothing forces a new column to be considered for the form;
- the definition file grows to cover every content table, far from the code it describes.

## Decision

The form is declared **on the EF content entities, with attributes**, and built into an
in-memory form model at startup.

- **Attributes** (`Persistence/FormAnnotations/`, pure metadata):
  - `[FormSection]` on the class;
  - `[FormPage]`, `[FormField]`, `[FormRequired]`, `[FormMaxLength]`, `[FormMaxItems]`,
    `[FormOptions]` and `[FormIgnore]` on properties.
- **Explicit question IDs.** The ID is written out in `[FormField]` and is never derived from
  the property name, so a C# rename cannot change the public contract.
- **Bindings come from EF metadata**, not from expressions in attributes:
  - a scalar property becomes a column binding;
  - a `[Flags]` enum becomes a flags binding;
  - an annotated navigation collection becomes a junction binding.
- **Options come from named providers** registered in C# (`[FormOptions("bnf-top-level")]`),
  because attributes cannot hold queries or tuples. A question with no `[FormOptions]` uses the
  only provider registered for its enum or referenced entity, so the key is needed only to
  choose between several providers of one type.
- **Coverage is enforced.** In a `[FormSection]` entity, every mapped scalar property must carry
  `[FormField]` or `[FormIgnore(reason)]`.
- **Pages may span entities.** The same `[FormPage]` ID on properties of different entities
  produces one merged page. Metadata must agree.
- **Fail fast.** A scanner builds one form model per record type from the attributes and EF
  `IModel`, and a validator reports every problem at once. An invalid model stops the API
  starting.
- **Versioned.** A fingerprint of the model is pinned in a test next to `FormVersions.<Type>`, so
  any attribute change without a version bump fails the build.

The runtime from the brief is otherwise kept: the API contract, save pipeline, audit,
`formVersion` and `revisionVersion` concurrency, `Operation.EditRecordContent` and ADR-004
concealment.

### Alternatives considered

- **A fluent C# definition per form**, as in the brief. Easy to read as a whole form, and
  supports lambdas for bindings and options. Rejected because of the duplication and drift
  described above, and because nothing ties new columns to a decision.
- **A JSON or YAML definition file.** No compile-time link to entities at all; renames and type
  mismatches are only caught by startup checks. Rejected.
- **Runtime-editable definitions (database or CMS).** Out of scope: definitions must be
  versioned with the entities and migrations.

## Consequences

- **Positive:**
  - A column and its question sit together.
  - Adding a column forces a decision ("question" or "ignored, because…").
  - Renames cannot change IDs.
  - Each `[FormIgnore("No page designed yet")]` is a searchable to-do list.
- **Negative: no single file shows a whole form.** Seeing the whole form means reading several
  entities. Mitigation: a test or debug endpoint can print the scanned model (sections, pages,
  questions in order) for review.
- **Negative: layering.** Persistence entities now carry presentation text (labels, hints,
  messages). The attributes are kept free of Application types; option keys are plain strings.
- **Negative: one section per entity.** By default, an entity's pages all fall in its section for
  a given record type. A per-page section override may be needed for entities that span design
  sections.
- **Negative: shared entity wording.** Attributes apply to every record type the entity takes
  part in. If Medicine and Vaccine need different labels for a shared column, the attributes will
  need a record-type qualifier.
- **Attribute order is not reliable** in .NET, so rule evaluation uses a fixed order by kind,
  and pages and questions are ordered only by explicit `Order` values.
- **Not yet designed for the attribute approach:** conditional pages and questions, add-another
  groups and precision dates. These will need further attributes or providers, and will be
  covered by a later ADR or an amendment to this one.

## Future consideration

- A generated, human-readable "form map" document produced from the scanned model in CI, so
  that reviewers can see a whole form without opening every entity.
