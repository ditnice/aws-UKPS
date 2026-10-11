# Creating form pages

This guide explains how to add a page to a record content form using the components and rules
that exist today. It uses the five Indication details pages as the worked example; their designs
are in [`screenshots/`](screenshots/) ([indication](screenshots/screen-one.png),
[BNF chapter](screenshots/screen-two.png), [therapeutic area](screenshots/screen-three.png),
[treating children](screenshots/screen-four.png), [treating cancer](screenshots/screen-five.png)).
For how the pieces fit together, read the [high-level design](high-level-design.md) first.

Adding a page that uses existing question types is a **backend-only change**: the frontend
renders whatever the definition describes. You only touch the frontend to add a new question
type or component.

## Before you start

| What | Where |
|---|---|
| Medicine form definition | `backend/src/Application/Forms/Medicines/MedicineFormDefinition.cs` |
| Builder (`Form`, `SectionBuilder`, `PageBuilder`, `QuestionBuilder`) | `backend/src/Application/Forms/Definitions/` |
| Bindings (`Bind`, `ContentRows`) | `backend/src/Application/Forms/Bindings/` |
| Options (`Choices`) | `backend/src/Application/Forms/Options/` |
| Rules | `backend/src/Application/Forms/Rules/` |
| Definition tests | `backend/tests/Application/Forms/` |
| Question components | `frontend/src/components/DynamicForm/questions/` |

## Step 1: Choose the IDs

**Page ID**: a lowercase slug, used in the URL, for example `bnf-chapter`. It must match
`^[a-z0-9]+(-[a-z0-9]+)*$` and be unique in the form.

**Question ID**: snake_case `entity.field`, for example
`medicines_product_detail.bnf_chapter_id`. Use the singular entity name, not the database
table name (the table is `medicines_product_details`). For a junction or collection, use the
entity name alone, for example `medicines_product_detail_therapeutic_area`. The ID must match
`^[a-z0-9_]+(\.[a-z0-9_]+)?$` and be unique across the whole form.

The question ID is the payload key, the validation error key, the frontend field name and the
audit `field_path`. **Never change an ID once it has been released.**

## Step 2: Add the page

Pages are added to a section in `Create()`, in display order. Give each page its own private
method so `Create()` stays readable (and under the analyser's method length limit):

```csharp
public static FormDefinition Create() =>
    Form.For(RecordType.Medicine, Version)
        .Section(
            "indication-details",
            "Indication details",
            s =>
                s.Page("indication", "Indication", IndicationPage)
                    .Page("bnf-chapter", "BNF chapter", BnfChapterPage)
                    // ...
        )
        .Build();
```

The page title (second argument) is a short name for the page. On a page with one question, the
question's label becomes the `<h1>` and the section title is shown as the caption above it. On a
page with several questions, the page title is the `<h1>` and each question shows its own label.

## Step 3: Pick a question type

| Builder method | `QuestionType` | Answer on the wire | Frontend component | Allowed bindings |
|---|---|---|---|---|
| `Textarea` | `Textarea` | `string \| null` | `TextareaQuestion` (NDS textarea) | Column, string property |
| `Select` | `Select` | `string \| null` | `SelectQuestion` | Column, enum or reference ID property |
| `Radio` | `Radio` | `string \| null` | `RadioQuestion` (NDS radios) | Column, enum or reference ID property |
| `Checkbox` | `Checkbox` | `string[]` (empty for none) | `CheckboxQuestion` (NDS checkboxes) | Junction |
| `Checkbox` + `.Display("combobox")` | `Checkbox` | `string[]` | `ComboboxSlot` (checkbox list until the combobox is built) | Junction |

`Radio`, `Select` and `Checkbox` need options. `Textarea` must not have any.

## Step 4: Bind the question

Bindings use typed expressions, so renaming an entity property breaks the build rather than
silently breaking the form.

**A column on the revision's content row**: the property type decides how the answer is
converted.

```csharp
Bind.Column((MedicinesProductDetail x) => x.Indication)          // string  -> "text"
Bind.Column((MedicinesProductDetail x) => x.IndicationIsCancer)  // enum?   -> "Yes" / "No" / "Unknown"
Bind.Column((MedicinesProductDetail x) => x.BnfChapterId)        // int?    -> "3"
```

**A junction table** (multi-select of reference data):

```csharp
Bind.Junction<MedicinesProductDetail, MedicinesProductDetailTherapeuticArea>(
    x => x.MedicinesProductDetailId,
    x => x.TherapeuticAreaId
)
```

The entity must be registered in `ContentRows` (`Bindings/ContentRows.cs`), which says how to
find the revision's row. Only `MedicinesProductDetail` is registered today. Content rows are
expected to exist already: `MedicinesProductDetail` is created by record creation, and the form
never creates rows. Binding to a table whose row does not yet exist needs row creation to be
added first (see the brief's notes on non-nullable columns).

## Step 5: Add options

**Enum options**: list the values in the order they should be shown, with their labels. The
enum type must match the bound property's enum, which the startup check enforces.

```csharp
Choices.Enum(
    (YesNoUnknown.Yes, "Yes"),
    (YesNoUnknown.No, "No"),
    (YesNoUnknown.Unknown, "Unknown at this stage")
)
```

**Reference data options**: give a query (filter and order) and a projection to
`ReferenceOptionRow`. Both are expressions and must be translatable by EF Core.

```csharp
// Top-level BNF chapters, labelled "1: Gastro-intestinal system".
Choices.Reference<BnfChapter>(
    q => q.Where(x => x.ParentId == null).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Code),
    x => new ReferenceOptionRow(x.Id, x.Code + ": " + x.Label, x.IsArchived)
)

// Leaf therapeutic areas, labelled "Parent › Child" if they have a parent.
Choices.Reference<TherapeuticArea>(
    q => q.Where(x => !x.Children.Any())
        .OrderBy(x => x.Parent!.DisplayOrder)
        .ThenBy(x => x.DisplayOrder)
        .ThenBy(x => x.Label),
    x => new ReferenceOptionRow(
        x.Id,
        x.Parent == null ? x.Label : x.Parent.Label + " › " + x.Label,
        x.IsArchived
    )
)
```

Archived reference data is not offered, unless it is the value already saved for this record, in
which case it is still shown and accepted. See [Reference data](reference-data.md) for seeding
and archiving.

## Step 6: Add validation

Rules are added in the question's configure callback. Each rule carries the exact message shown
to the user, on both the client and the server.

```csharp
q => q.Hint("...")
    .Required("Enter the indication this product is seeking a licence for")
    .MaxLength(2000, "Indication must be 2000 characters or fewer")
```

| Builder method | `RuleKind` | Applies to | Passes when |
|---|---|---|---|
| `Required(message)` | `Required` | All types | Text is not blank, an option is chosen, or at least one checkbox is ticked |
| `MaxLength(max, message)` | `MaxLength` | `Textarea` | Text is at most `max` UTF-16 code units, after trimming |
| `MaxItems(max, message)` | `MaxItems` | `Checkbox` | At most `max` options are ticked |

How rules are applied:

- Text is trimmed first, and whitespace-only counts as unanswered. `"   abc   "` is checked as
  `"abc"`.
- Rules run in the order you declare them, and only the first failing rule is reported. Declare
  `Required` before `MaxLength`.
- A question without `Required` is optional. Write "(optional)" in the label if the design shows
  it, as the therapeutic area page does.
- Each rule kind may appear once per question. Attaching a rule to a type it does not apply to
  fails the startup check.

The server also checks things the definition does not declare: every page question is present,
no other keys are sent, each value has the right JSON shape, and every selected value is an
offered option. You do not need to add rules for these.

Write messages in the GOV.UK error message style used in the screenshots: say what to do, for
example "Select a BNF chapter" or "Enter the indication…".

### Adding a new rule kind

Only needed if the existing rules cannot express what a page needs. Agree it with the team first.

1. Add the value to `RuleKind` (`backend/src/Application/Forms/Rules/RuleKind.cs`).
2. Add a `QuestionRule` subclass with `Kind`, `Value`, `AppliesTo` and `IsSatisfiedBy`, and a
   `QuestionBuilder` method.
3. Add the matching case to `isSatisfied` in `frontend/src/lib/dynamic-form/answers.ts`.
4. Add passing and failing cases to `backend/tests/Application/Forms/Fixtures/rule-cases.json`,
   and the new kind to `CreateRule` in `RuleFixtureTests`. `Fixture_CoversEveryRuleKind` fails
   until you do.
5. Regenerate the OpenAPI spec (build the backend) and the client (`pnpm generate:api`), because
   `RuleKind` is part of the API.

## Step 7: Hints and headings

- Pass hint text with `.Hint("...")`. A blank line (`\n\n`) starts a new paragraph, as on the
  indication page.
- Hints are shown above the input and linked to it with `aria-describedby`.
- On a single-question page, the label is visually hidden on the field because it is already the
  page heading; screen readers still announce it.

## Step 8: Bump the form version

Any change to a definition (pages, IDs, labels, hints, rules, options or bindings) must bump the
version, so clients holding an old page are told to reload instead of saving against the new one.

1. Change `MedicineFormDefinition.Version`, for example `2026.10.1` to `2026.10.2`.
2. Run `MedicineFormDefinitionTests`. `Definition_ChangesRequireAVersionBump` fails and prints the
   new fingerprint.
3. Update `ExpectedVersion` and `ExpectedFingerprint` in the test to match.

If you change the definition but forget the version, step 2 fails, which is the point of the
test.

## Step 9: Test

The startup check (`FormDefinitionValidator`) runs when the API starts and in
`MedicineFormDefinitionTests.Definition_IsValid`. It catches:

- duplicate page or question IDs, and badly formed IDs;
- sections with no pages, and pages with no questions;
- a binding that does not suit the question type (for example a string column behind a
  checkbox);
- missing options on a choice question, or options on a textarea;
- enum options for a different enum than the bound property, or enum options on a non-enum
  binding;
- a rule on a type it does not apply to, or the same rule declared twice.

Also update:

- `Definition_HasTheIndicationDetailsPagesInOrder` (or an equivalent test for your section) with
  the new page order;
- `MedicineFormOptionsTests` if you added reference data options;
- `NextAndPreviousPage_FollowFormOrder` if you added a page at either end.

Run the tests:

```sh
cd backend
dotnet test --filter-class "*MedicineForm*"   # definition and options tests
dotnet test                                   # everything
```

(This repo uses xUnit v3 on Microsoft Testing Platform: `--filter` silently runs no tests; use
`--filter-class` or `--filter-method`.)

You do not need to regenerate the OpenAPI spec or the frontend client for a new page: the page
travels as data through the existing DTOs.

Finally, try it locally: create a medicine record, or use **View or edit** in the organisation
records table, and step through to your page.

## Adding a new question type

Needed when a page uses an input that none of the existing types cover, for example a date. This
touches both backend and frontend.

**Backend**

1. Add the value to `QuestionType` (`Definitions/QuestionType.cs`).
2. Add a builder method to `PageBuilder`.
3. Describe its answer in `QuestionTypeExtensions`: `IsValidShape` (the JSON it accepts),
   `Normalise` (trimming, sorting), and `HasOptions` / `IsMultiValue` if relevant.
4. Add the binding kinds it accepts to `_allowedBindings` in `FormDefinitionValidator`, adding a
   binding if no existing one fits.
5. Add it to the `AppliesTo` set of each rule that should work with it.
6. Build the backend to regenerate the OpenAPI spec, then add fixture cases for it.

**Frontend**

1. Run `pnpm generate:api`.
2. Add a component in `components/DynamicForm/questions/` that takes `QuestionProps`: `question`,
   `value`, `onChange`, `onBlur`, `error`, `disabled` and `labelAsHeading`. Follow
   `RadioQuestion` or `TextareaQuestion`, including `QuestionHint` and the `labelAsHeading` class.
3. Register it in `components` in `questionRegistry.tsx`, and in `getQuestionTargetId` if the
   error summary should link somewhere other than an element with the question's ID.
4. Describe its form value in `lib/dynamic-form/answers.ts` (`toFormValue`, `normaliseAnswer`).
5. Add rendering, error and payload tests to `FormPage.spec.tsx`.

### Plugging in the combobox

The therapeutic area question has `display: "combobox"`, so `questionRegistry` renders it with
`ComboboxSlot`, which shows a checkbox list for now. The combobox should accept `ComboboxProps`
(exported from `questions/ComboboxSlot.tsx`):

```ts
type ComboboxProps = {
  id: string
  label: string
  hint?: string | null
  options: FormOptionDto[]
  value: string[]
  onChange: (value: string[]) => void
  onBlur: () => void
  error?: string
  disabled: boolean
}
```

Replace the body of `ComboboxSlot` with the new component. Map `QuestionProps` to
`ComboboxProps` (`id = question.id`, `options = question.options ?? []`, and so on). Nothing else
needs to change: the answer is still a `string[]` and `MaxItems` still applies.

## Checklist

- [ ] Page ID is a unique slug; question IDs follow `entity.field` and are new
- [ ] Page added in display order with its own private method
- [ ] Question type, binding and options suit each other (startup check passes)
- [ ] Rules added with user-facing messages, `Required` first
- [ ] Hint added if the design has one
- [ ] `MedicineFormDefinition.Version` bumped; `ExpectedVersion` and `ExpectedFingerprint` updated
- [ ] Definition and options tests updated; `dotnet test` passes
- [ ] Page checked locally against the design
