# UKPS dynamic form builder: implementation brief

> **Status: historical, superseded.** This was the original brief for a form builder in which
> each record type's form was a separate C# definition. That approach has been replaced by
> **attributes on the EF content entities**
> ([ADR-006](../ADR/ADR-006-attribute-annotated-record-content-forms.md)), and this brief is kept
> only as background.
>
> - **Still the basis of the design:** question IDs as a public contract, typed tables with
>   bindings, the whole-page save, server-authoritative validation mirrored on the client, the
>   shared rule fixture, `formVersion` and `revisionVersion` concurrency, field-level audit,
>   draft-only edits and ADR-004 concealment.
> - **Replaced:** the fluent `Form.For(...)` builder (§4 "Sketch") and per-form definition
>   classes, by annotated entities, a startup scanner and named option providers.
> - **Not yet designed for the attribute approach:** conditions and stale-answer clearing,
>   add-another groups, precision dates and cross-field rules.
>
> Current documentation: [high-level design](high-level-design.md),
> [creating form pages](creating-form-pages.md),
> [reference data](reference-data.md).

> Brief for Claude Code. Source of truth for the design is the "UKPS dynamic form builder: design" doc (5 Oct 2026). Where this file and the code disagree on an existing convention, follow the code and flag it.

## 1. What we are building

Record content pages (medicines and vaccines revisions) are described by **C# form definitions** served by the backend. The frontend renders each page from that description, validates it client-side, and posts every answer on the page to **one generic save endpoint**. The endpoint re-validates, maps each answer onto its typed column/table, clears answers that branching has made irrelevant, audits every changed field, and returns the next page.

### In scope (v1)
- Record content forms only (Medicine and Vaccine `RecordType`s).
- Question types: `text`, `textarea`, `number`, `radio`, `checkbox`, `select`, `precisionDate`, `group` (add-another, with at most one level of nesting).
- Rules: single-field, conditional required, cross-field comparisons, server-only rules.
- Cross-page branching (pages and questions shown/skipped by earlier answers).
- Field-level audit on every save.

### Out of scope (v1)
- Auth, user, organisation and registration forms stay hand-built. Do not touch them.
- Runtime-editable definitions (DB or Payload CMS authoring).
- Form overview / task list / "check your answers" page. (Decided: not v1.)
- Submission, QA review and publishing workflow changes.
- Groups nested more than one level deep (a group inside a group inside a group). One level is in scope: see decision 17.

## 2. Agreed decisions (do not re-litigate)

| # | Topic | Decision |
|---|---|---|
| 1 | Scope | Record content only |
| 2 | Definition source | C# code, versioned with entities/migrations |
| 3 | Page shape | Free grouping: a page may mix fields from several tables; each question carries its own binding |
| 4 | GET response | One call per page: structure, rules, inline options, current answers, plus read-only `context` answers from earlier pages that this page's conditions and cross-field rules reference |
| 5 | Save unit | Whole page. Client validates every question on Save; payload includes **every** question on the page, optional blanks as `null` |
| 6 | Validation authority | Server re-runs every rule; client mirrors the client-safe subset with Zod |
| 7 | Conditionals | Cross-page branching |
| 8 | Navigation | Backend computes next page and returns it in the save response |
| 9 | Stale answers | Cleared on save when branching makes them no longer applicable |
| 10 | Revision state | Saves allowed on `WorkflowStatus.Draft` only |
| 11 | Audit | Every save writes one `RecordEvent` + a `RecordEventFieldChange` per changed question |
| 12 | Repeating data | Add-another groups on the same page |
| 13 | Options | Reference data inline in the page schema; archived items excluded unless already selected |
| 14 | Payload keys | Stable question IDs matching the audit `FieldPath` convention. Exception: group items are addressed by 0-based index in payload/error keys and by `[id=…]` in audit paths (§4) |
| 15 | Vaccines | Need no binding kinds beyond those listed for medicines |
| 16 | Group audit | **Both**, keyed by `itemId` (not index, so removing an item does not shift every later item): one row per changed child field (`medicines_active_substance.generic_name[id=81].name`) **and** one item-level row when an item is added or removed (`medicines_active_substance.generic_name[id=81]`, value = whole item JSON). New items use the DB ID assigned on save |
| 17 | Nested groups | One level allowed: a group child may itself be a group (e.g. clinical trial → other registry numbers; vaccine technology → antigens). Max depth 2, enforced by a startup check |
| 18 | Edit permission | New `Operation.EditRecordContent`, granted to Standard, Champion and Super users. `Operation.Update` is unchanged (Standard users still cannot use it) |
| 19 | Regulatory date rows | One row per `(revision, event)` in practice. Changing precision **replaces** the row (delete old precision row, insert new). `IsConfidential` is derived: true unless precision is `ActualDate` |

## 3. Repo orientation

Monorepo: `backend/` (.NET 10, EF Core, Postgres), `frontend/` (Next.js 16, Payload CMS 3, TanStack Form, Zod, NICE design system, hey-api client), `infrastructure/` (Terraform).

### Backend conventions to follow
- Layering: `backend/src/Application/<Domain>/` (services, `Dtos/`, `Errors/`), `backend/src/Persistence/` (entities, configurations, enums, migrations), `backend/src/WebApi/Controllers/`.
- Entities are `internal sealed`. Top-level content tables hang off `RevisionId`: see `Persistence/Entities/MedicinesRevisionContent/`, `VaccinesRevisionContent/`, `SharedRevisionContent/`. **Child collections hang off a parent content row instead**: `MedicinesActiveSubstance` → `MedicinesProductDetailId`, `VaccinesAntigen`/`VaccinesAdjuvant`/`VaccinesAdministrationRoute` → `VaccinesTechnologyId`, `OtherClinicalTrialNumber` → `ClinicalTrialId`.
- Some content columns are non-nullable (`MedicinesProductDetail.RecordTitle`, `VaccinesProductDetail.RecordTitle`/`CompanyCode`, `VaccinesTechnology.VaccinePlatformId`/`HasAdjuvant`, `RecordClinicalTrial.StudyName`/`ClinicalTrialsGovNumber`, child `Name`-style columns). These constrain row creation and stale clearing (§4 startup checks).
- Enums are stored as `integer` columns and serialised as names on the wire (`JsonStringEnumConverter(allowIntegerValues: false)`, `JsonNumberHandling.Strict` in `Program.cs`).
- Optimistic concurrency already has a convention: `public uint Version { get; set; }` + `builder.Property(x => x.Version).IsRowVersion();` (Npgsql maps this to `xmin`); see `UserRegistrationRequestConfiguration.cs`.
- Reference data derives from `ReferenceDataBase` (`Id`, `Label`, `IsArchived`).
- Services return `Result<T, TError>` (`Application/Common/Result.cs`). Errors are `abstract record` with nested `sealed record` variants and an internal `Match` method — copy `Application/Records/Errors/CreateRecordError.cs`.
- Controllers map errors to `Problem(...)` / `ValidationProblem(ModelState)`; see `WebApi/Controllers/RecordController.cs`. Authorisation via `IOrganisationAuthoriser.CanPerformOperationOnOrganisation`. Not-found vs forbidden concealment follows `documentation/ADR/ADR-004-*`.
- Time via `IDateTimeProvider`; current user via `CurrentDbUserEntityService`.
- Audit entities already exist: `RecordEvent`, `RecordEventFieldChange` (`FieldPath` = `section.field`, e.g. `medicines_product_detail.indication`).
- Tests: xUnit in `backend/tests/`, with `ServiceTestHarness`, `PostgresFixture`, `ApiFactory`, `FakeDateTimeProvider`.
- Commands (`backend/justfile`): `just format`, `just build`, `just test`, `just generate-migration <Name>`. Run `dotnet csharpier format .` before finishing.
- OpenAPI spec is checked in at `backend/openapi/UKPS.Api.json`; regenerate it when endpoints change.

### Frontend conventions to follow
- Example hand-built form to mirror: `frontend/src/app/portal/user/me/edit-details/_components/EditDetailsForm.tsx` (TanStack `useForm` + `revalidateLogic({ mode: 'submit', modeAfterSubmission: 'blur' })` + Zod + `getFieldErrorMessage` + `updateFormApiErrors`).
- Components: `frontend/src/components/` (`Input`, `Textarea`, `Select`, `Button`, `PageHeader`, `Placeholder/ErrorState`); NDS radio/checkbox via `@nice-digital/nds-radio` / `nds-checkbox` (see `app/portal/components/(examples)/`).
- API client is generated: `pnpm generate:api` → `src/client/generated/`. Server-side calls via `src/client/server-api.ts`.
- Error helpers: `src/lib/form/` (`errorMessages.ts`, `formErrorHandling.ts`, `getFieldErrorMessage.ts`), `src/lib/responses/typeGuards.ts`.
- Tests: `pnpm test:unit` (Vitest, `*.spec.tsx` beside source), `pnpm test:e2e` (Playwright). Lint/format: `pnpm lint:check`.
- Existing record UI lives under `/portal/organisations/[id]/records` (list only). There is **no record detail page** yet.

### Prerequisites (not yet in the code)
These are needed before or alongside the phases in §10.
- **Authorisation**: add `Operation.EditRecordContent` (decision 18) to `Application/InternalServices/Authorisation/Operation.cs` and to `_allowedOperationsForStandardUser` in `OrganisationAuthoriser`. Without it, Standard users (the people who create records) get refused.
- **Vaccine record creation**: `Record.CreateInitial` hardcodes `RecordType.Medicine` and `RecordCreationService` only creates a `MedicinesProductDetail`. Vaccine forms cannot be exercised until vaccine records can be created. Required before delivery step 8 (vaccines).
- **Record landing page**: the page the form returns to when `nextPageId` is `null` (§8) does not exist yet. Until it does, return to the organisation's records list.

## 4. Form definition model (backend)

Suggested location: `backend/src/Application/Forms/` (`Definitions/`, `Bindings/`, `Rules/`, `Conditions/`, `Medicines/MedicineFormDefinition.cs`, `Vaccines/VaccineFormDefinition.cs`).

Hierarchy, built once at startup and registered as a singleton. The singleton holds structure only: bindings receive the scoped `AppDbContext` as a method argument, and reference-data options are loaded per request.

- `FormDefinition` — one per `RecordType`; ordered sections; `Version` string (e.g. `"2026.10.1"`). Add a unit test that snapshots a hash of the serialised definition, so changing the definition without bumping `Version` fails the build. A manual bump that someone forgets would let old clients through.
- `SectionDefinition` — `Id`, `Title`, ordered pages.
- `PageDefinition` — `Id` (URL slug, e.g. `product-detail`), `Title`, optional `Caption`, ordered questions, optional `ShowWhen`.
- `QuestionDefinition` — `Id`, `Type`, `Label`, `Hint`, `Rules`, optional `ShowWhen`, optional `Options`, `Binding`. Group questions also have `ItemLabel` and `Children`.

### Question IDs
- `<table>.<field>` in snake_case, identical to `RecordEventFieldChange.FieldPath`, e.g. `medicines_detail.is_repurposed_medicine`.
- Junction/collection questions use the table name, e.g. `medicines_product_detail_therapeutic_area`, `record_clinical_trial`.
- **Filtered collections** (several groups over one table, split by a discriminator column) append the discriminator value in snake_case: `medicines_active_substance.generic_name` and `medicines_active_substance.development_name` (`NameType`). Without this, the two groups would share an ID.
- **Payload and error keys** address group children by **0-based** index, matching TanStack array fields: `medicines_active_substance.generic_name[0].name`. Nested: `record_clinical_trial[0].other_clinical_trial_number[1].other_registry_number`.
- **Audit `FieldPath`s** address group items by DB ID instead (decision 16): `medicines_active_substance.generic_name[id=81].name`.
- IDs are a public contract (payload keys, error keys, audit history). Never change a released ID.

### Bindings (the only code that touches EF entities)

| Binding | Example | Read | Write |
|---|---|---|---|
| `ColumnBinding<TEntity, TValue>` | `MedicinesProductDetail.Indication` | Read property from the revision's row | Set property; create the row if missing |
| `FlagsBinding<TEntity, TEnum>` | `MedicineTechnologyStatus` | Split flags → `string[]` | OR selected values |
| `JunctionBinding<TJunction>` | `MedicinesProductDetailTherapeuticArea` | Selected reference IDs | Diff; insert/delete junction rows |
| `CollectionBinding<TParent, TChild>` | `MedicinesActiveSubstance` under `MedicinesProductDetail`, filtered by `NameType` | Ordered child rows → group items | Resolve the parent row (create it if missing, or fail the startup check if the parent has non-nullable columns not on this page); replace the collection; `DisplayOrder` = index; keep DB IDs via `itemId`; set the discriminator on new rows. Children may be another `CollectionBinding` (one level) |
| `PrecisionDateBinding` | `RegulatoryDate` where `DateEvent = UkLicence` | `{ precision, year, quarter?, month?, day? }` | Replace the row for that event (delete any row with a different precision, decision 19); store first day of period; set `IsConfidential` = precision ≠ `ActualDate` |

`RegulatoryDate.ConditionalApprovalAnticipated` is a separate `ColumnBinding`-style question on the same row (only relevant for `UkLicence`/`IntlLicence`). It is not part of the date value.

Build bindings from typed expressions (`b => b.Column<MedicinesDetail>(x => x.ModeOfAction)`) so renames break the build.

### Conditions
Small serialisable expression tree over question IDs: `Equals`, `In`, `IsAnswered`, `And`, `Or`, `Not`. Data, not lambdas — evaluated in C# and serialised to the frontend.

### Sketch
```csharp
Form.For(RecordType.Medicine, version: "2026.10.1")
  .Section("product", "Product", s => s
    .Page("product-detail", "Product details", p => p
      .Text("medicines_product_detail.record_title", "Record title",
            b => b.Column<MedicinesProductDetail>(x => x.RecordTitle),
            r => r.Required("Enter a record title").MaxLength(200))
      .Select("medicines_product_detail.bnf_chapter_id", "BNF chapter",
            b => b.Column<MedicinesProductDetail>(x => x.BnfChapterId),
            options: Options.Reference<BnfChapter>())
      .Checkbox("medicines_product_detail_therapeutic_area", "Therapeutic areas",
            b => b.Junction<MedicinesProductDetailTherapeuticArea>(),
            options: Options.Reference<TherapeuticArea>(),
            r => r.MinItems(1).MaxItems(3, "Select up to 3 therapeutic areas")))
    .Page("repurposed-details", "Repurposed medicine", p => p
      .ShowWhen(Q("medicines_detail.is_repurposed_medicine").Equals(YesNoUnknown.Yes))
      .Textarea("medicines_detail.repurposed_medicine_details", "Details",
            b => b.Column<MedicinesDetail>(x => x.RepurposedMedicineDetails),
            r => r.Required().MaxLength(2000))));
```

### Startup / unit-test checks (fail fast)
- Duplicate question or page IDs.
- Conditions that reference unknown questions or questions on **later** pages.
- Binding CLR type incompatible with question type.
- Pages with no questions.
- Groups nested deeper than one level (decision 17).
- Any question that can be hidden (it or its page has a `ShowWhen`) must bind to a nullable column or a collection, because stale clearing has to set it to `null`/empty.
- If writing a question can create a content row, every non-nullable column of that row must be bound by a **required** question on the same page. Otherwise first write cannot succeed.
- Two `CollectionBinding`s over the same table must have different discriminator values.
- The first page of the first section must have no `ShowWhen` (it is the form's entry point, since there is no overview endpoint in v1).
- Every `pattern` regex compiles and behaves the same in .NET and JS (covered by the shared fixture, §8).

## 5. Question types and rules

| Type | JSON value | Frontend component | Typical binding |
|---|---|---|---|
| `text` | `string \| null` | `Input` | String column |
| `textarea` | `string \| null` | `Textarea` | String column |
| `number` | `number \| null` | `Input` (numeric) | Int/decimal column |
| `radio` | `string \| null` | NDS radio group | Enum name or reference ID |
| `checkbox` | `string[]` (empty = none) | NDS checkbox group | Flags enum or junction |
| `select` | `string \| null` | `Select` | Reference ID |
| `precisionDate` | `{ precision, year, quarter?, month?, day? } \| null` | New `PrecisionDateInput` | `RegulatoryDate` |
| `group` | `Array<{ itemId?: number, [childId]: value }>` (a child value may itself be a group array, one level) | New `AddAnotherGroup` (supports one nested `AddAnotherGroup`) | Child collection |

- Option values are always strings on the wire (enum names, or reference IDs as strings).
- `precision` is a `DatePrecision` name: `EstimatedQuarter` (needs `year`, `quarter`), `EstimatedMonth` (needs `year`, `month`) or `ActualDate` (needs `year`, `month`, `day`). There is no year-only precision.

| Category | Rules | Client | Server |
|---|---|---|---|
| Single-field | `required`, `minLength`, `maxLength`, `pattern`, `min`, `max`, `integer`, `minItems`, `maxItems`, `dateRange` | ✅ | ✅ |
| Conditional required | `requiredWhen(condition)` (this or earlier page) | ✅ | ✅ |
| Cross-field | `compare(otherId, op)` with `< <= > >= == !=` for numbers and precision dates | ✅ | ✅ |
| Server-only | reference exists and not archived (unless already saved), revision is Draft, uniqueness, every submitted `itemId` belongs to this revision's collection (and to the right parent item when nested) | ❌ never serialised | ✅ |

- `dateRange(min?, max?)` applies to `precisionDate`: the period start must be on or after `min` and on or before `max`, each given as an ISO date. (Proposed definition: confirm before implementing.)
- Every rule carries its own message, defined in C#; the client shows the same text.
- Precision-date comparisons compare period starts; undecidable mixed-precision comparisons pass.
- Rules on a question hidden by `ShowWhen` never run.

## 6. API contract

| Method | Route | Purpose |
|---|---|---|
| GET | `/records/{recordId}/pages/{pageId}` | Page structure, rules, options, current answers, context |
| PUT | `/records/{recordId}/pages/{pageId}` | Validate and save the whole page; returns next page |

(No form-overview endpoint in v1.)

### GET — 200
```json
{
  "formVersion": "2026.10.1",
  "revisionVersion": 4711,
  "section": { "id": "product", "title": "Product" },
  "page": { "id": "product-detail", "title": "Product details" },
  "questions": [
    {
      "id": "medicines_product_detail.record_title",
      "type": "text",
      "label": "Record title",
      "hint": "A short label to identify this record",
      "rules": [
        { "kind": "required", "message": "Enter a record title" },
        { "kind": "maxLength", "value": 200, "message": "Record title must be 200 characters or fewer" }
      ]
    },
    {
      "id": "medicines_product_detail_therapeutic_area",
      "type": "checkbox",
      "label": "Therapeutic areas",
      "options": [ { "value": "4", "label": "Oncology" }, { "value": "7", "label": "Respiratory" } ],
      "rules": [ { "kind": "maxItems", "value": 3, "message": "Select up to 3 therapeutic areas" } ]
    },
    {
      "id": "medicines_active_substance.generic_name",
      "type": "group",
      "label": "Generic names",
      "itemLabel": "Generic name",
      "children": [ { "id": "name", "type": "text", "label": "Name", "rules": [ { "kind": "required", "message": "Enter a name" } ] } ]
    }
  ],
  "answers": {
    "medicines_product_detail.record_title": "Chronic hepatitis C in adults",
    "medicines_product_detail_therapeutic_area": ["4"],
    "medicines_active_substance.generic_name": [ { "itemId": 81, "name": "sofosbuvir" } ]
  },
  "context": { "medicines_detail.is_repurposed_medicine": "Yes" }
}
```
- `answers` has a key for every page question (`null` / `[]` when empty).
- Question-level `showWhen` included when present; page-level `ShowWhen` is not (server never serves a non-applicable page).
- `context` holds every earlier-page answer that this page needs on the client: questions referenced by `showWhen`, `requiredWhen` **and** `compare(otherId)`.
- `revisionVersion` is the revision's concurrency token (`RecordRevision.Version`, i.e. `xmin`). The client echoes it on PUT so a save based on stale data is rejected, not just simultaneous saves.

### GET — status codes
| Status | When |
|---|---|
| 200 | Page served |
| 404 | Record/revision/page not found, record in another organisation, or page does not currently apply (ADR-004 concealment) |

### PUT — request
```json
{
  "formVersion": "2026.10.1",
  "revisionVersion": 4711,
  "answers": {
    "medicines_product_detail.record_title": "Chronic hepatitis C in adults",
    "medicines_product_detail.branded_name": null,
    "medicines_product_detail_therapeutic_area": ["4", "7"],
    "medicines_active_substance.generic_name": [ { "itemId": 81, "name": "sofosbuvir" }, { "name": "velpatasvir" } ]
  }
}
```
C# body type: `Dictionary<string, JsonElement>` for `answers` (one generic OpenAPI type, not one per page).

### Responses
| Status | When | Body |
|---|---|---|
| 200 | Saved (or nothing changed) | `{ "nextPageId": "repurposed-details" }` or `{ "nextPageId": null }` |
| 400 | Rule failures, unknown/missing question IDs, wrong value shape | `ValidationProblemDetails`, `errors` keyed by question ID (indexed for group children) |
| 403 | Caller can read the record (same organisation) but lacks `EditRecordContent` | `ProblemDetails` |
| 404 | Record/revision/page not found, record in another organisation (concealed per ADR-004), or page does not currently apply | `ProblemDetails`, neutral text |
| 409 | Not Draft / `revisionVersion` stale or concurrent change / definitions changed | `ProblemDetails` with `code`: `revision_not_draft`, `revision_changed`, `form_version_changed` |

## 7. Save pipeline (`RecordPageSaveService`)

One DB transaction. Returns `Result<SavePageDto, SavePageError>`.

1. **Authorise and load** revision: `Read` on the organisation (else 404, ADR-004), then `EditRecordContent` (else 403); 409 unless Draft; 409 `revision_changed` if the submitted `revisionVersion` ≠ current; resolve page (404 if unknown).
2. **Snapshot**: read every bound value for the revision into an `AnswerSet` keyed by question ID (one query per entity type). This is "before".
3. **Applicability**: evaluate page `ShowWhen` against snapshot; 404 if it does not apply.
4. **Shape check**: every page question present, no unknown keys, values match type shape, option values exist, every `itemId` exists in this revision's collection (under the right parent item for nested groups). Fail → 400 before rules.
5. **Merge** submitted answers over the snapshot → "after".
6. **Validate** visible questions against "after": single-field → conditional → cross-field → server-only. Return **all** errors at once.
7. **Clear stale**: re-evaluate every `ShowWhen` across the whole form against "after"; set now-inapplicable stored answers to `null`/empty.
8. **Write**: diff after vs before; call each changed binding's `Write`. Bindings create missing content rows on first write.
9. **Audit**: one `RecordEvent` with new `RecordEventType.RecordContentUpdated` (`RecordId` and `RevisionId` set, `Note` = page ID) + one `RecordEventFieldChange` per changed question, including cleared ones. Scalar values (text, number, enum name, reference ID) are stored as the raw string (`Unknown`, not `"Unknown"`), so the "fields leaving unknown" KPI can query them directly. Arrays, precision dates and group items are stored as wire JSON. `null` for empty. Groups: keyed by `itemId`, see decision 16; write the audit rows after `SaveChangesAsync` assigns IDs to new items (still inside the transaction).
10. **Stamp and commit**: set `RecordRevision.UpdatedAt` / `UpdatedBy`; `SaveChangesAsync`; commit.
11. **Next page**: first page after the current one (form order) whose `ShowWhen` passes, else `null`.

- **Concurrency**: add `public uint Version` + `.IsRowVersion()` to `RecordRevision` (existing convention, see `UserRegistrationRequestConfiguration.cs`). Compare the submitted `revisionVersion` up front, and also map `DbUpdateConcurrencyException` → 409 `revision_changed` for races. Every non-empty save updates the revision row (step 10), so the token changes.
- **Empty diff**: write nothing, create no event, still return next page.
- New enum value and concurrency token need a migration: `just generate-migration AddRecordContentUpdatedAndRevisionConcurrency`.

## 8. Frontend renderer

- Route: `frontend/src/app/portal/records/[recordId]/[pageId]/page.tsx` (server component) → fetch GET via `server-api` → `<FormPage>` (client). 404 → `not-found`. This is the first `/portal/records/...` route; existing record UI is under `/portal/organisations/[id]/records`.
- Suggested shared code: `frontend/src/lib/dynamic-form/` (`buildZodSchema.ts`, `evaluateCondition.ts`, `buildPayload.ts`, `applyServerErrors.ts`) and `frontend/src/components/DynamicForm/` (`FormPage.tsx`, `questionRegistry.tsx`, `PrecisionDateInput/`, `AddAnotherGroup/`).
- `questionRegistry` maps `type` → component wrapping existing design-system components.
- Each question renders in `form.Field name={question.id}`; group children use TanStack array fields (0-based) so names match server error keys. Nested groups render an `AddAnotherGroup` inside an item (one level only).
- `showWhen` evaluated against form values + `context`; hidden questions unmounted and submitted as `null`.
- `buildZodSchema(questions, context)`: rule → Zod (`required` → non-empty, `maxLength` → `.max()`, `maxItems` → array `.max()`, …) using server message text; conditional + cross-field as one page-level `superRefine` with `path: [questionId]`.
- Validation timing: `revalidateLogic({ mode: 'submit', modeAfterSubmission: 'blur' })`.
- On 400: generalise `updateFormApiErrors` to loop over every key in `errors` and `setFieldMeta`.
- Payload: every page question ID present; empty text → `null`; empty checkbox → `[]`; echo `formVersion` and `revisionVersion`.
- On 409 `revision_changed` / `form_version_changed`: show a "this page has changed, reload" message; do not retry automatically.
- After 200: `router.push` to `nextPageId`, or, when `null`, the record page. That page doesn't exist yet (see §3 Prerequisites), so until it does, go to `/portal/organisations/[id]/records`.
- Error summary at top of page on failed submit; `PageHeader` with section title as caption.

### Contract-drift guard
Shared JSON fixture (e.g. `backend/tests/Application/Forms/Fixtures/rule-cases.json`, copied or referenced by the frontend tests) listing every rule kind with passing and failing values. Run it through both the C# validator (xUnit) and `buildZodSchema` (Vitest). New rule kinds must pass in both.

## 9. Testing

| Layer | What | Where |
|---|---|---|
| Definitions | Startup checks in §4 | xUnit |
| Rules | Shared fixture, C# and Zod | xUnit + Vitest |
| Bindings | Read/write per binding kind incl. first-write row creation, parent-row resolution, junction diff, collection replace, nested collection, discriminator filtering, precision-date replace on precision change | xUnit + `PostgresFixture` |
| Save pipeline | Draft-only, stale clearing, audit rows (itemId-keyed, raw scalar values), next page, stale `revisionVersion` 409, concurrency 409, empty diff, foreign `itemId` rejected | `ServiceTestHarness` |
| API | Status codes incl. ADR-004 (other org → 404, same org without `EditRecordContent` → 403), `ValidationProblemDetails` keys | controller tests via `ApiFactory` |
| UI | Each question type renders, shows errors, submits correct shape | Vitest + Testing Library |
| E2E | Fill medicines form through a branch and back; cleared answers gone | Playwright |

## 10. Suggested delivery order

Work in small, reviewable steps; stop and summarise after each phase.

0. **Prerequisites** — `Operation.EditRecordContent` + authoriser tests (§3 Prerequisites).
1. **Backend core model** — definitions, conditions, rules, bindings incl. nested collections (no endpoints). Startup checks + unit tests.
2. **Product details page definition** — text, select, reference data, flags multi-select, generic-names and development-names groups (`MinItems(1)` on both, matching `CreateRecordCommand`), and a later page whose `ShowWhen` depends on a question here.
3. **GET endpoint** — `RecordFormController` (or extend `RecordController`), DTOs, OpenAPI regen.
4. **Save pipeline + PUT endpoint** — migration for `RecordContentUpdated` + `RecordRevision.Version` (`xmin`), audit, stale clearing, next page. Full test coverage per §9.
5. **Shared rule fixture** — before a second page is added.
6. **Frontend** — `pnpm generate:api`, renderer, registry, `buildZodSchema`, the two new components, route, unit tests.
7. **E2E** — Playwright happy path + branch/clear path.
8. Remaining medicines pages, then vaccine record creation (§3 Prerequisites), then vaccines.

## 11. Guardrails

- Do not introduce a generic key/value answers table; answers stay in the existing typed tables.
- Do not change released question IDs.
- Do not add runtime authoring (DB/CMS) for definitions.
- Do not modify hand-built auth/user/org forms.
- Keep `RecordCreationService` working; the first content row still comes from record creation. Form rules for fields it also collects (record title, branded name, generic/development names) must not be looser than `CreateRecordCommand`'s.
- Follow existing formatting (CSharpier, Prettier, ESLint) and run the relevant test commands before declaring a phase done.
- Ask before making decisions not covered here (e.g. a new question type or rule kind).
