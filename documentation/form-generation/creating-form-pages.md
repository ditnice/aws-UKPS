# Creating form pages

> **Status: implemented** for the first delivery (Medicine form, Indication details). See
> [ADR-006](../ADR/ADR-006-attribute-annotated-record-content-forms.md).

This guide explains how to put questions on a record content form by annotating EF entities. The
worked example is the Indication details section of the Medicine form. The designs are in
[`screenshots/`](screenshots/):
- [indication](screenshots/screen-one.png);
- BNF chapter: [chapter](screenshots/screen-two-part-1.png),
  [chapter list](screenshots/screen-two-part-2.png),
  [subchapter](screenshots/screen-two-part-3.png),
  [subchapter list](screenshots/screen-two-part-4.png);
- therapeutic area: [area list](screenshots/screen-three-part-1.png),
  [disease or condition](screenshots/screen-three-part-2.png),
  [chosen pair](screenshots/screen-three-part-3.png);
- [treating children](screenshots/screen-four.png);
- [treating cancer](screenshots/screen-five.png).

For how the pieces fit together, read the [high-level design](high-level-design.md) first.

If a page only uses existing controls, rules and option providers, adding it is a **backend-only
change** made entirely in the entity file. The frontend renders whatever the form model
describes. You only touch the frontend to add a new control.

## Before you start

| What | Where |
|---|---|
| Attributes | `backend/src/Persistence/FormAnnotations/` |
| Content entities | `backend/src/Persistence/Entities/MedicinesRevisionContent/`, `VaccinesRevisionContent/`, `SharedRevisionContent/` |
| Option keys (provider names, for registration and `[FormOptions]` overrides) | `backend/src/Persistence/FormAnnotations/OptionKeys.cs` |
| Option providers | `backend/src/Application/RecordForms/Options/` (`MedicineOptionProviders.cs`) |
| Form versions | `backend/src/Application/RecordForms/FormVersions.cs` |
| Tests | `backend/tests/Application/RecordForms/` |
| Question components | `frontend/src/components/DynamicForm/questions/` |

## Attribute reference

| Attribute | On | Arguments | Notes |
|---|---|---|---|
| `[FormSection(recordType, id, title)]` | class | `Order`, `RowSource` | One per record type. Every entity declaring the same section ID must agree on title and `Order` |
| `[FormPage(id, title)]` | property | `Order` | Order within the section. Every declaration of the same page ID must agree |
| `[FormField(id, label, control)]` | property | `Hint`, `Display`, `Order`, `Placeholder`, `ChildLabel`, `ChildPlaceholder`, `AddAnotherLabel` | `Order` only matters on pages with several questions. `Placeholder` is for `Select` and `HierarchicalSelect`; the `Child…` properties for `HierarchicalSelect` (`ChildLabel` is required); `AddAnotherLabel` for a multi-value `HierarchicalSelect` |
| `[FormRequired(message)]` | property | | |
| `[FormMaxLength(max, message)]` | property | | `Text`, `Textarea` |
| `[FormMaxItems(max, message)]` | property | | `Checkbox` |
| `[FormOptions(key)]` | property | | Chooses a provider other than the default for the property's type. Only needed when several providers offer the same type |
| `[FormIgnore(reason)]` | property | | Not a question. The reason is required |

`FormControl`: `Text`, `Textarea`, `Radio`, `Select`, `Checkbox`, `HierarchicalSelect`.
`FormRowSource`: `FirstSave` (default), `RecordCreation`.

## Worked example: the Indication details section

This is the whole of `MedicinesIndicationDetail` as it would be annotated for the first
delivery. Every mapped scalar property has either `[FormField]` or `[FormIgnore]`; that is the
[coverage rule](#step-1-account-for-every-property).

```csharp
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Persistence.Entities.MedicinesRevisionContent;

[FormSection(RecordType.Medicine, "indication-details", "Indication details", Order = 20)]
internal sealed class MedicinesIndicationDetail
{
    [FormIgnore("Database identity")]
    public int Id { get; set; }

    [FormIgnore("Revision ownership is supplied by the server")]
    public int RevisionId { get; set; }

    [FormPage("indication", "Indication", Order = 10)]
    [FormField(
        "medicines_indication_detail.indication",
        "What is the indication this product is seeking a licence for?",
        FormControl.Textarea,
        Hint = "Describe the indication as you intend to apply for licensing in the UK. ...\n\nYou can edit these details at any time throughout product development.")]
    [FormRequired("Enter the indication this product is seeking a licence for")]
    [FormMaxLength(2000, "Indication must be 2000 characters or fewer")]
    public string? Indication { get; set; }

    // The most specific BNF entry chosen: a subchapter, or the chapter if none is picked.
    [FormPage("bnf-chapter", "BNF chapter", Order = 20)]
    [FormField(
        "medicines_indication_detail.bnf_chapter_id",
        "Select the BNF chapter for this product",
        FormControl.HierarchicalSelect,
        Placeholder = "Choose BNF chapter",
        ChildLabel = "Select the BNF subchapter (optional)",
        ChildPlaceholder = "Choose BNF subchapter")]
    [FormRequired("Choose a BNF chapter")]
    public int? BnfChapterId { get; set; }

    [FormPage("paediatric", "Treating children", Order = 40)]
    [FormField(
        "medicines_indication_detail.indication_is_paediatric",
        "Is this product intended to treat children?",
        FormControl.Radio)]
    [FormRequired("Select whether this product is intended to treat children")]
    public IndicationPaediatricStatus? IndicationIsPaediatric { get; set; }

    [FormPage("cancer", "Treating cancer", Order = 50)]
    [FormField(
        "medicines_indication_detail.indication_is_cancer",
        "Is this a product intended to treat cancer?",
        FormControl.Radio)]
    [FormRequired("Select whether this product is intended to treat cancer")]
    public YesNoUnknown? IndicationIsCancer { get; set; }

    [FormPage("rare-disease", "Treating a rare disease", Order = 60)]
    [FormField(
        "medicines_indication_detail.indication_is_rare_disease",
        "Is this product intended to treat a rare disease?",
        FormControl.Radio,
        Hint = "A disease is rare if fewer than 5 in 10,000 people have it.")]
    [FormRequired("Select whether this product treats a rare disease")]
    public YesNoUnknown? IndicationIsRareDisease { get; set; }

    [FormIgnore("No page designed yet")]
    public int? FormulationTypeId { get; set; }

    [FormIgnore("No page designed yet")]
    public string? Presentation { get; set; }

    [FormIgnore("No page designed yet")]
    public string? ModeOfAction { get; set; }

    [FormIgnore("No page designed yet")]
    public string? ProposedDoseRegimen { get; set; }

    [FormIgnore("No page designed yet")]
    public YesNoUnknown? IsPersonalisedMedicine { get; set; }

    [FormIgnore("No page designed yet")]
    public MedicineTechnologyStatus? MedicineTechnologyStatus { get; set; }

    // Navigation: no annotation needed unless it backs a question.
    public RecordWorkflow.RecordRevision? Revision { get; set; }
    public ReferenceData.BnfChapter? BnfChapter { get; set; }
    public ReferenceData.FormulationType? FormulationType { get; set; }

    // Junction navigation backing a multi-select question.
    [FormPage("therapeutic-area", "Therapeutic area", Order = 30)]
    [FormField(
        "medicines_indication_detail_therapeutic_area",
        "Select the most appropriate therapeutic area for this product (optional)",
        FormControl.HierarchicalSelect,
        Display = "combobox",
        Hint = "Select up to 3 therapeutic areas. If you are unsure, you can move on and answer this question later.",
        Placeholder = "Choose a therapeutic area",
        ChildLabel = "Select the disease or condition (optional)",
        ChildPlaceholder = "Choose a disease or condition",
        AddAnotherLabel = "Add another therapy area")]
    [FormMaxItems(3, "Select up to 3 therapeutic areas")]
    public ICollection<MedicinesIndicationDetailTherapeuticArea> TherapeuticAreas { get; set; } = [];
}
```

The scanner turns this into the following pages, in this order:

| Order | Page ID | Question ID | Control | Binding |
|---|---|---|---|---|
| 10 | `indication` | `medicines_indication_detail.indication` | `Textarea` (explicit) | Column, string |
| 20 | `bnf-chapter` | `medicines_indication_detail.bnf_chapter_id` | `HierarchicalSelect` (explicit) | Column, reference ID |
| 30 | `therapeutic-area` | `medicines_indication_detail_therapeutic_area` | `HierarchicalSelect` (explicit) | Junction |
| 40 | `paediatric` | `medicines_indication_detail.indication_is_paediatric` | `Radio` | Column, enum |
| 50 | `cancer` | `medicines_indication_detail.indication_is_cancer` | `Radio` | Column, enum |
| 60 | `rare-disease` | `medicines_indication_detail.indication_is_rare_disease` | `Radio` | Column, enum |

Declaration order in the file does not matter; only `Order` does. For example, the therapeutic
area page is declared last but sorts third.

## Step 1: Account for every property

In a `[FormSection]` entity, every mapped scalar property needs `[FormField]` or `[FormIgnore]`.
If you add a column to a content entity, the API will not start until you have made that
choice. Use `[FormIgnore]` with a reason that a reviewer can understand:

- `"Database identity"`, `"Revision ownership is supplied by the server"`: never on a form.
- `"No page designed yet"`: intended for a form, but not designed. Search for this reason to
  find work still to do.
- `"Set by record creation"`: collected elsewhere, for example by the create record form.

Navigation properties are exempt, unless they back a junction question.

## Step 2: Choose the section and its row source

Put `[FormSection]` on the entity if it isn't already there:

```csharp
[FormSection(RecordType.Medicine, "indication-details", "Indication details", Order = 20)]
```

- **Section ID**: a lowercase slug. Sections are ordered by `Order` within the form.
- **Shared entities**: add one `[FormSection]` per record type.
- **`RowSource`** says how the revision's row for this entity comes to exist:
  - **`FirstSave`** (the default): the first save that writes to the entity creates the row.
    This is only allowed if the entity has no non-nullable columns besides `Id` and
    `RevisionId`. `MedicinesIndicationDetail` qualifies.
  - **`RecordCreation`**: record creation inserts the row, and the form only updates it. Use
    this for entities with required columns, for example:

    ```csharp
    [FormSection(RecordType.Medicine, "product-details", "Product details",
        Order = 10, RowSource = FormRowSource.RecordCreation)]
    internal sealed class RecordProductDetail { ... }
    ```

The entity must have an `int RevisionId` foreign key to `RecordRevision`. That is how the form
finds the revision's row.

## Step 3: Choose the IDs

**Page ID**: a lowercase slug used in the URL, for example `bnf-chapter`. It must match
`^[a-z0-9]+(-[a-z0-9]+)*$` and be unique in the form, across all entities. If you reuse an
existing page ID, your question joins that page (see [Pages that span
entities](#pages-that-span-entities)).

**Question ID**: snake_case `entity.property`, for example
`medicines_indication_detail.bnf_chapter_id`.
- Use the singular entity name, not the database table name.
- For a junction, use the junction entity name alone, for example
  `medicines_indication_detail_therapeutic_area`.
- The ID must match `^[a-z0-9_]+(\.[a-z0-9_]+)?$` and be unique across the whole form.

The question ID is used as:
- the payload key;
- the validation error key;
- the frontend field name;
- the audit `field_path`.

It is written out by hand, so renaming a C# property does not change it. **Never change an ID
once it has been released.**

## Step 4: Add the page and the field

```csharp
[FormPage("bnf-chapter", "BNF chapter", Order = 20)]
[FormField(
    "medicines_indication_detail.bnf_chapter_id",
    "Select the BNF chapter for this product",
    FormControl.Select)]
```

- **Page title**: a short name for the page.
- **Single-question page**: the question's label becomes the `<h1>`, and the section title is
  shown as the caption above it.
- **Page with several questions**: the page title is the `<h1>`, each question shows its own
  label, and `FormField.Order` sets their order.
- **Gaps in `Order`**: leave gaps (10, 20, 30) so that pages can be inserted later without
  renumbering.

### Choose a control

Every `[FormField]` names its control as the third argument. It must be one of the allowed
controls for the property type:

| Property type | Allowed | Answer on the wire | Frontend component |
|---|---|---|---|
| `string` | `Text`, `Textarea` | `string \| null` | `Input` / `Textarea` |
| non-flags enum | `Radio`, `Select` | `string \| null` (member name) | NDS radios / `Select` |
| `int?` reference FK | `Radio`, `Select`, `HierarchicalSelect` | `string \| null` (ID) | NDS radios / `Select` / parent and child `Select`s |
| `[Flags]` enum | `Checkbox` | `string[]` (member names) | NDS checkboxes |
| junction navigation | `Checkbox`, `HierarchicalSelect` | `string[]` (IDs) | NDS checkboxes / repeated parent and child `Select`s |

`HierarchicalSelect` is for reference data with a `ParentId`: the user picks a parent, then
optionally a child, and the answer is the most specific choice. It needs a tree provider (see
step 5). On a junction it repeats, with an **Add another** button up to the `MaxItems` rule. See
[Hierarchical selects](high-level-design.md#hierarchical-selects).

`Display = "combobox"` asks the frontend for a searchable combobox. Until that is built, it is
ignored and the control renders as plain dropdowns or checkboxes.

### Hints

- Pass hint text with `Hint = "..."`. A blank line (`\n\n`) starts a new paragraph.
- Hints are shown above the input and linked to it with `aria-describedby`.
- On a single-question page, the label is visually hidden on the field because it is already
  the page heading. Screen readers still announce it.

## Step 5: Add options

Choice controls (`Radio`, `Select`, `Checkbox`, `HierarchicalSelect`) take their options from a
provider registered in C#. Attributes cannot hold queries, so the list itself lives in code.

You don't usually name the provider. A question uses the one provider registered for its type:
- an enum property uses the enum provider of that enum;
- a reference ID or junction uses the reference provider of the referenced entity, or the
  reference tree provider for a `HierarchicalSelect`.

Add `[FormOptions(key)]` only when several providers offer the same type, for example a second
label set for `YesNoUnknown`. Startup fails if a question has no matching provider, or several
and no `[FormOptions]`.

### Using an existing provider

| Key (`OptionKeys`) | Kind | Offers |
|---|---|---|
| `yes-no-unknown` (`YesNoUnknown`) | Enum `YesNoUnknown` | Yes, No, Unknown at this stage |
| `paediatric-status` (`PaediatricStatus`) | Enum `IndicationPaediatricStatus` | Exclusively for children, Both children and adults, Exclusively for adults, Unknown at this stage |
| `bnf-chapters` (`BnfChapters`) | Reference tree `BnfChapter` | Chapters with their sections, labelled `"1: Gastro-Intestinal System"` and `"1.3: Antisecretory drugs…"` |
| `therapeutic-areas` (`TherapeuticAreas`) | Reference tree `TherapeuticArea` | Areas with their diseases or conditions, labelled `"1: Addiction"` and `"1.3: Smoking cessation"` |

### Adding a provider

Add its key to `OptionKeys` in `Persistence/FormAnnotations/` (it is only a string, so entities can
use it in `[FormOptions]` without depending on Application). Then register the provider in
`MedicineOptionProviders.cs`, or a vaccine equivalent:

```csharp
// Enum: values in display order, with labels.
registry.AddEnum(OptionKeys.YesNoUnknown,
    (YesNoUnknown.Yes, "Yes"),
    (YesNoUnknown.No, "No"),
    (YesNoUnknown.Unknown, "Unknown at this stage"));

// Flat reference data (for Radio, Select, Checkbox): a query (filter and order) and a
// projection. Both must translate to SQL. The last argument is the parent ID, null here.
registry.AddReference<FormulationType>(OptionKeys.FormulationTypes,
    q => q.OrderBy(x => x.Label),
    x => new ReferenceOptionRow(x.Id, x.Label, x.IsArchived, null));

// Two-level reference data (for HierarchicalSelect). ParentId links children to parents; the
// query's order is kept within each level. More than two levels throws.
registry.AddReferenceTree<BnfChapter>(OptionKeys.BnfChapters,
    q => q.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id),
    x => new ReferenceOptionRow(x.Id, x.Code + ": " + x.Label, x.IsArchived, x.ParentId));
```

(`FormulationTypes` is illustrative; it is not registered yet.)

The query and projection are described for the form fingerprint by `ExpressionDescription`, so
they must not capture variables: use literals in the lambdas. Startup fails on a captured
variable, or on an expression node the describer does not support yet; extend it if a provider
needs one.

The startup check fails in these cases:
- no provider is registered for the question's type, or several are and the question has no
  `[FormOptions]`;
- the key named by `[FormOptions]` does not exist;
- an enum provider's enum differs from the property's enum;
- an enum provider is used on a reference ID or junction question, or a reference provider on
  an enum question;
- a `HierarchicalSelect` uses a flat provider, or another choice control uses a tree provider.

An archived reference item is not offered, unless it is the value already saved for this record.
In that case it is still shown and accepted. See [Reference data](reference-data.md).

## Step 6: Add validation

```csharp
[FormRequired("Enter the indication this product is seeking a licence for")]
[FormMaxLength(2000, "Indication must be 2000 characters or fewer")]
```

| Attribute | `RuleKind` | Applies to | Passes when |
|---|---|---|---|
| `FormRequired(message)` | `Required` | All controls | Text is not blank, an option is chosen, or at least one checkbox is ticked |
| `FormMaxLength(max, message)` | `MaxLength` | `Text`, `Textarea` | Text is at most `max` UTF-16 code units, after trimming |
| `FormMaxItems(max, message)` | `MaxItems` | `Checkbox` | At most `max` options are ticked |

How rules are applied:

- **Trimming.** Text is trimmed first, and whitespace-only counts as unanswered. `"   abc   "` is
  checked as `"abc"`.
- **Fixed order.** Rules run in a fixed order by kind: `Required`, then `MaxLength`, then
  `MaxItems`. Only the first failing rule is reported. The order of the attributes in the file
  does not matter, because .NET does not guarantee attribute order.
- **Optional questions.** A question without `[FormRequired]` is optional. Write "(optional)" in
  the label if the design shows it, as the therapeutic area page does.
- **One of each.** Each rule kind may appear once per question. A rule on a control it does not
  apply to fails the startup check.

The server also checks things that the attributes don't declare. You do not need rules for
these:
- every page question is present;
- no other keys are sent;
- each value has the right JSON shape;
- every selected value is an offered option.

Write messages in the GOV.UK error message style used in the screenshots: say what to do, for
example "Choose a BNF chapter" or "Enter the indication…".

### Adding a new rule kind

Only needed if the existing rules can't express what a page needs. Agree it with the team first.

1. Add the value to `RuleKind`.
2. Add a rule attribute in `Persistence/FormAnnotations/`, and its evaluation in `RuleEvaluator`.
   Give it a place in the fixed evaluation order, and list the controls it applies to.
3. Add the matching case to `isSatisfied` in `frontend/src/lib/dynamic-form/answers.ts`.
4. Add passing and failing cases to `rule-cases.json`. `Fixture_CoversEveryRuleKind` fails until
   you do.
5. Regenerate the OpenAPI spec (build the backend) and the client (`pnpm generate:api`), because
   `RuleKind` is part of the API.

## Pages that span entities

A page can hold questions from several entities. Declare the same `[FormPage]` ID, with the
same title and `Order`, on each property. The entities must be in the same section. The example
below is illustrative and is not a page in the current design:

```csharp
[FormSection(RecordType.Medicine, "product-details", "Product details",
    Order = 10, RowSource = FormRowSource.RecordCreation)]
internal sealed class RecordProductDetail
{
    [FormPage("names", "Product names", Order = 20)]
    [FormField("record_product_detail.branded_name", "Branded name (optional)", FormControl.Text, Order = 10)]
    [FormMaxLength(200, "Branded name must be 200 characters or fewer")]
    public string? BrandedName { get; set; }
    // ...
}

[FormSection(RecordType.Medicine, "product-details", "Product details", Order = 10)]
internal sealed class SomeOtherMedicineEntity
{
    [FormPage("names", "Product names", Order = 20)]
    [FormField("some_other_medicine_entity.development_code", "Development code", FormControl.Text, Order = 20)]
    public string? DevelopmentCode { get; set; }
    // ...
}
```

The `names` page shows both questions, ordered by `FormField.Order`. One save writes both
tables, and the audit records them as one event.

Keep the page metadata in one place to avoid mismatches: declare the page ID, title and order as
constants (for example in a `MedicinePages` static class) and use the constants in every
`[FormPage]`.

## Step 7: Bump the form version

Any change to a form entity's attributes needs a version bump: pages, IDs, labels, hints,
rules, option keys, order, or adding or ignoring a property. The bump tells clients holding an
old page to reload instead of saving against the new one.

1. Change `FormVersions.Medicine`, for example `2026.10.1` to `2026.10.2`.
2. Run the form model tests. `Model_ChangesRequireAVersionBump` fails and prints the new
   fingerprint.
3. Update `ExpectedVersion` and `ExpectedFingerprint` in the test.

If you change an attribute but forget the version, step 2 fails, which is the point of the test.

Changing an option provider's **query** changes the fingerprint (its description is included).
Changing reference data **rows** or labels does not.

## Startup checks

`FormModelValidator` runs when the API starts and in `MedicineFormModelTests.Model_IsValid`. It
reports every problem in one exception.

| Area | Fails when |
|---|---|
| Coverage | A mapped scalar property has neither `[FormField]` nor `[FormIgnore]`, or has both. A `[FormIgnore]` reason is blank. A form attribute is on an unmapped property or a non-junction navigation |
| Pairing | `[FormField]` has no `[FormPage]`, or `[FormPage]` has no `[FormField]` |
| IDs | A page or question ID is badly formed or duplicated within the form |
| Agreement | Declarations of one section ID disagree on title or order, or declarations of one page ID disagree on title or order. A page spans entities in different sections |
| Order | Two sections, two pages in a section, or two questions on a page have the same `Order` |
| Structure | A section has no pages, or a page has no questions |
| Controls | The control does not suit the property type (see the table in step 4). A non-nullable column has no `[FormRequired]` |
| Presentation | `Placeholder` on a control other than `Select` or `HierarchicalSelect`; `ChildLabel` or `ChildPlaceholder` on a control other than `HierarchicalSelect`; `AddAnotherLabel` on anything but a multi-value `HierarchicalSelect`; a `HierarchicalSelect` without `ChildLabel` |
| Options | A choice control has no provider registered for its type, or several and no `[FormOptions]`. A text control has `[FormOptions]`. The key is unknown. The enum provider's enum does not match the property, or the provider kind (enum, flat or tree) does not match the binding and control. A `HierarchicalSelect`'s reference entity has no `int? ParentId` |
| Rules | A rule is on a control it does not apply to, or a rule kind appears twice |
| Rows | The entity has no `int RevisionId` FK to `RecordRevision`. `RowSource = FirstSave` on an entity with other non-nullable columns |

## Step 8: Test

- **`MedicineFormModelTests`**:
  - `Model_IsValid`;
  - the page order test (`Model_HasTheIndicationDetailsPagesInOrder`, or an equivalent for your
    section);
  - the fingerprint test (`Model_ChangesRequireAVersionBump`), which prints the whole scanned
    model when it fails.
- **`FormModelValidatorTests`**: if you add a startup check. It scans small test-only entities
  (`Scanning/ScanningTestEntities.cs`) against a test EF model.
- **`RecordFormServiceTests`**: database tests of the page query and save services, following
  the [walkthrough](record-creation-walkthrough.md).
- **`MedicineOptionProviderTests`**: if you added a provider.
- **`NextAndPreviousPage_FollowFormOrder`**: if you added a page at either end.

```sh
cd backend
dotnet test --filter-class "*RecordForms*"  # form model, scanner, rules and service tests
dotnet test                                 # everything
```

(This repo uses xUnit v3 on Microsoft Testing Platform: `--filter` silently runs no tests; use
`--filter-class` or `--filter-method`.)

You do not need to regenerate the OpenAPI spec or the frontend client for a new page: the page
travels as data through the existing DTOs.

Finally, try it locally. Create a medicine record, or open an existing one from the organisation
records table and select **Update record**. Then step through to your page.

## Adding a new control

Needed when a page uses an input that no existing control covers, for example a date. This
touches both backend and frontend.

**Backend**

1. Add the value to `FormControl` (`Persistence/FormAnnotations/FormControl.cs`).
2. Add it to the control/type table in the scanner (`ControlRules.Allowed`) and, if needed,
   add a binding.
3. Describe its answer: the JSON shape it accepts, how it is normalised, and whether it has
   options or multiple values.
4. Add it to the controls that each relevant rule applies to.
5. Build the backend to regenerate the OpenAPI spec, then add fixture cases.

**Frontend**

1. Run `pnpm generate:api`.
2. Add a component in `components/DynamicForm/questions/` that takes `QuestionProps`:
   - `question`, `value`, `onChange`, `onBlur`, `error`, `disabled`;
   - `labelAsHeading`.
3. Register it in `questionRegistry.tsx`.
4. Describe its form value in `lib/dynamic-form/answers.ts` (`toFormValue` and `toAnswer`).
5. Add rendering, error and payload tests.

## Checklist

- [ ] Every mapped scalar property has `[FormField]` or `[FormIgnore("reason")]`
- [ ] `[FormSection]` present, with the right `RowSource`
- [ ] Page ID is a unique slug; question IDs follow `entity.property` and are new
- [ ] `Order` values leave gaps and don't clash
- [ ] Control suits the property type; choice controls have a provider registered for their type (or `[FormOptions]` naming one)
- [ ] Rules added, with user-facing messages
- [ ] Hint added if the design has one
- [ ] `FormVersions.Medicine` bumped; `ExpectedVersion` and `ExpectedFingerprint` updated
- [ ] Form model and provider tests updated; `dotnet test` passes
- [ ] Page checked locally against the design
