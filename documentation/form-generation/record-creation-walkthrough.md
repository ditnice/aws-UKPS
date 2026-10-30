# Record creation walkthrough: database changes per save

> **Status: implemented.** This shows what the attribute-driven form
> ([high-level design](high-level-design.md)) writes. `RecordFormServiceTests` checks the same
> steps against a real database.

This walks through the happy path of creating a medicine record and answering the six
Indication details pages. For each save it lists the tables written and the values to expect,
with SQL to check them. Use it to confirm the flow behaves as intended.

For the design behind it, see the [high-level design](high-level-design.md).

The record task list is available at `GET /records/{recordId}/task-list`. It reads the latest revision
and returns the form's sections and tasks, including each task's first page and a status derived
from saved answers. This read does not write to the database. A task is `Completed` when all its
required questions have answers, `InProgress` when it has at least one answer but required
answers remain, and `NotStarted` when it has no saved answers.

## Conventions

- All tables are in the `ukps` schema.
- Enums are stored as integers. The values used here:

  | Enum | Values |
  |---|---|
  | `RecordType` | `0` Medicine, `1` Vaccine |
  | `RecordStatus` | `0` Unpublished, `1` Published, `2` OnHold, `3` Archived |
  | `WorkflowStatus` | `0` Draft, `1` QAReview, `2` Published, `3` Rejected |
  | `RecordEventType` | `0` RecordCreated |
  | `NameAndIdentifierType` | `0` GenericName, `1` OtherIdentifier |
  | `IndicationPaediatricStatus` | `0` Unknown, `1` ExclusivelyChildren, `2` ExclusivelyAdults, `3` BothChildrenAndAdults |
  | `YesNoUnknown` | `0` Unknown, `1` No, `2` Yes |

- IDs in this document are examples: organisation `3`, user `12`, record `5`, revision `7`,
  record product detail `8`, medicines indication detail `9`, BNF section `40`, therapeutic area
  `51`. Reference data IDs depend on the order the seeder inserts rows (see
  [Reference data](reference-data.md)), so look yours up. Substitute your own; the queries
  below use `:record_id` and `:revision_id`.
- `T0`, `T1`, … are the times of each save (UTC).
- **Loading a page (GET) writes nothing.** Only the create request and each **Save and continue**
  write to the database.
- Every save that changes an answer also updates the revision's `xmin` (its concurrency token).
  It is a Postgres system column, so select it explicitly: `select xmin, * from ...`.

## The example answers

| Step | Page | Answer entered |
|---|---|---|
| 0 | Create record | Company code `GS-7977`, branded name `Sovaldi`, generic name `sofosbuvir`, other identifier `PSI-7977`, record title `Chronic hepatitis C in adults` |
| 1 | Indication | `Chronic hepatitis C infection in adults with genotype 1` |
| 2 | BNF chapter | Chapter `5: Infections`, subchapter `5.3: Antiviral drugs` (ID `40`) |
| 3 | Therapeutic area | Area `10: Viral infectious disease`, condition `10.x: Hepatitis C virus (HCV)` (ID `51`) |
| 4 | Treating children | This product is exclusively for adults |
| 5 | Treating cancer | No |
| 6 | Treating a rare disease | No |

## Step 0: Create record

**Request:** `POST /records` from `/portal/organisations/3/records/create`.

**Response:** `200 { "recordId": 5, "canEditContent": true }`, then the
browser goes to the task list at `/portal/records/5/task-list`. Selecting the **Indication
details** task opens `/portal/records/5/indication`. (`canEditContent` is `false` for a record type
with no form, or for a user who cannot edit record content, and the browser then goes to the
records list.)

All of the following are written in one transaction at `T0`.

### `records` (1 row inserted)

| Column | Value |
|---|---|
| `id` | `5` |
| `organisation_id` | `3` |
| `record_type` | `0` (Medicine) |
| `record_status` | `0` (Unpublished) |
| `created_at` | `T0` |
| `created_by` | `12` |
| `reviewed_at` | `null` |

### `record_status_histories` (1 row inserted)

| Column | Value |
|---|---|
| `record_id` | `5` |
| `from_status` | `null` |
| `to_status` | `0` (Unpublished) |
| `reason`, `note` | `null` |
| `updated_by` | `12` |
| `updated_at` | `T0` |

### `record_revisions` (1 row inserted)

| Column | Value |
|---|---|
| `id` | `7` |
| `record_id` | `5` |
| `based_on_revision_id` | `null` |
| `workflow_status` | `0` (Draft) |
| `created_by` / `created_at` | `12` / `T0` |
| `xmin` | Set by Postgres; note it, it changes on every later save |

### `record_events` (1 row inserted)

| Column | Value |
|---|---|
| `record_id` | `5` |
| `revision_id` | `7` |
| `event_type` | `0` (RecordCreated) |
| `performed_by` / `performed_at` | `12` / `T0` |
| `note` | `null` |

No `record_field_audit` rows are written for record creation.

### `record_product_details` (1 row inserted)

| Column | Value |
|---|---|
| `id` | `8` |
| `revision_id` | `7` |
| `company_code` | `GS-7977` |
| `branded_name` | `Sovaldi` (`null` if left blank) |
| `record_title` | `Chronic hepatitis C in adults` |

### `record_names_and_identifiers` (1 row per name inserted)

Generic names come first, then other identifiers. `display_order` starts at `1` within each
name type.

| `record_product_detail_id` | `name` | `name_type` | `display_order` |
|---|---|---|---|
| `8` | `sofosbuvir` | `0` (GenericName) | `1` |
| `8` | `PSI-7977` | `1` (OtherIdentifier) | `1` |

### Not written yet

**`medicines_indication_details` has no row for the revision yet.** Record creation does not
insert one; the form creates it on the first save that writes an answer (step 1 here). Until
then, the form pages show every question as unanswered.

### Check

```sql
select * from ukps.records where id = :record_id;
select * from ukps.record_status_histories where record_id = :record_id;
select xmin, * from ukps.record_revisions where record_id = :record_id;
select * from ukps.record_events where record_id = :record_id order by id;
select * from ukps.record_product_details where revision_id = :revision_id;
select * from ukps.record_names_and_identifiers
where record_product_detail_id =
  (select id from ukps.record_product_details where revision_id = :revision_id)
order by name_type, display_order;
-- Expect no rows:
select * from ukps.medicines_indication_details where revision_id = :revision_id;
```

## What every page save writes

Steps 1 to 6 each write the same three things, at that step's time `Tn`, in one transaction:

1. **The answer** in its content table (see each step). The first save that writes an answer
   also inserts the `medicines_indication_details` row.
2. **`record_revisions`**, row `7` updated so that `xmin` changes. No column value changes: the
   save rewrites `workflow_status` with its current value, checked against the `xmin` the page
   was loaded with.
3. **`record_field_audit`**, 1 row inserted per changed question: `record_id = 5`,
   `revision_id = 7`, `field_path` = the question ID, `old_value` = the previous answer,
   `new_value` = the new answer, `changed_by = 12`, `changed_at = Tn`. Single values are stored
   as plain text and lists as JSON; no answer is `null`.

No `record_events` row is written for a page save.

`records`, `record_status_histories`, `record_product_details` and `record_names_and_identifiers`
are not touched by any page save.

Each save's response is `200 { "nextPageId": "..." }` and the browser moves to that page.

## Step 1: Indication

**Request:** `PUT /records/5/pages/indication`

```json
{
  "formVersion": "2026.10.1",
  "revisionVersion": <xmin from step 0>,
  "answers": {
    "medicines_indication_detail.indication": "Chronic hepatitis C infection in adults with genotype 1"
  }
}
```

**Response:** `{ "nextPageId": "bnf-chapter" }`

| Table | Change |
|---|---|
| `medicines_indication_details` | **1 row inserted**: `id = 9`, `revision_id = 7`, `indication = Chronic hepatitis C infection in adults with genotype 1`; every other column `null` |
| `record_revisions` (row `7`) | new `xmin` |
| `record_field_audit` | `changed_at = T1`, `field_path = medicines_indication_detail.indication`, `old_value = null`, `new_value = Chronic hepatitis C infection in adults with genotype 1` |

Leading and trailing spaces are trimmed before saving, so typing `"  text  "` stores `text`.

## Step 2: BNF chapter

**Request:** `PUT /records/5/pages/bnf-chapter`, with answers
`{ "medicines_indication_detail.bnf_chapter_id": "40" }`. Its `revisionVersion` is the new `xmin`
from step 1, which the browser received when it loaded the BNF chapter page. Each page load picks
up the latest value in the same way.

**Response:** `{ "nextPageId": "therapeutic-area" }`

| Table | Change |
|---|---|
| `medicines_indication_details` (row `9`) | `bnf_chapter_id`: `null` → `40` |
| `record_revisions` (row `7`) | new `xmin` |
| `record_field_audit` | `changed_at = T2`, `field_path = medicines_indication_detail.bnf_chapter_id`, `old_value = null`, `new_value = 40` |

The answer is the most specific choice: the subchapter's ID here. Choosing only the chapter
would store the chapter's ID. The audit stores the ID, not its label.

## Step 3: Therapeutic area

**Request:** `PUT /records/5/pages/therapeutic-area`, with answers
`{ "medicines_indication_detail_therapeutic_area": ["51"] }`. Each entry is the most specific
choice: the condition if one is picked, otherwise the area.

**Response:** `{ "nextPageId": "paediatric" }`

| Table | Change |
|---|---|
| `medicines_indication_detail_therapeutic_areas` | 1 row inserted: (`medicines_indication_detail_id = 9`, `therapeutic_area_id = 51`) |
| `medicines_indication_details` | Not changed |
| `record_revisions` (row `7`) | new `xmin` |
| `record_field_audit` | `changed_at = T3`, `field_path = medicines_indication_detail_therapeutic_area`, `old_value = null`, `new_value = ["51"]` |

Selections are sorted as text before they are compared and audited, so choosing `["7","10"]`
would be stored in the audit as `["10","7"]`. Choosing the same option twice is rejected with
"You have selected the same option more than once.". If nothing is ticked (the question is optional), no
junction rows are written; a first save with nothing ticked is not a change, so it writes no
audit rows and does not touch the revision.

## Step 4: Treating children

**Request:** `PUT /records/5/pages/paediatric`, with answers
`{ "medicines_indication_detail.indication_is_paediatric": "ExclusivelyAdults" }`.

**Response:** `{ "nextPageId": "cancer" }`

| Table | Change |
|---|---|
| `medicines_indication_details` (row `9`) | `indication_is_paediatric`: `null` → `2` (ExclusivelyAdults) |
| `record_revisions` (row `7`) | new `xmin` |
| `record_field_audit` | `changed_at = T4`, `field_path = medicines_indication_detail.indication_is_paediatric`, `old_value = null`, `new_value = ExclusivelyAdults` |

The content table stores the enum's integer; the audit stores its name.

## Step 5: Treating cancer

**Request:** `PUT /records/5/pages/cancer`, with answers
`{ "medicines_indication_detail.indication_is_cancer": "No" }`.

**Response:** `{ "nextPageId": "rare-disease" }`

| Table | Change |
|---|---|
| `medicines_indication_details` (row `9`) | `indication_is_cancer`: `null` → `1` (No) |
| `record_revisions` (row `7`) | new `xmin` |
| `record_field_audit` | `changed_at = T5`, `field_path = medicines_indication_detail.indication_is_cancer`, `old_value = null`, `new_value = No` |

## Step 6: Treating a rare disease

**Request:** `PUT /records/5/pages/rare-disease`, with answers
`{ "medicines_indication_detail.indication_is_rare_disease": "No" }`.

**Response:** `{ "nextPageId": null }`. This is the last page of the task, so its button reads
**Save and return to task list**, and the browser goes back to `/portal/records/5/task-list`.

| Table | Change |
|---|---|
| `medicines_indication_details` (row `9`) | `indication_is_rare_disease`: `null` → `1` (No) |
| `record_revisions` (row `7`) | new `xmin` |
| `record_field_audit` | `changed_at = T6`, `field_path = medicines_indication_detail.indication_is_rare_disease`, `old_value = null`, `new_value = No` |

## Final state

After step 6, for record `5`:

| Table | Rows for this record |
|---|---|
| `records` | 1, unchanged since step 0 |
| `record_status_histories` | 1, unchanged since step 0 |
| `record_revisions` | 1, `workflow_status = 0` (still Draft), `xmin` changed by every save |
| `record_product_details` | 1, unchanged since step 0 |
| `record_names_and_identifiers` | 2, unchanged since step 0 |
| `medicines_indication_details` | 1 (created in step 1), with `indication`, `bnf_chapter_id = 40`, `indication_is_paediatric = 2`, `indication_is_cancer = 1`, `indication_is_rare_disease = 1` |
| `medicines_indication_detail_therapeutic_areas` | 1 (`51`) |
| `record_events` | 1, the `event_type = 0` creation event |
| `record_field_audit` | 6, one per page, in page order |

The record stays `Unpublished` and the revision stays `Draft`; answering the pages does not
submit anything.

### Check

```sql
-- Revision
select xmin, workflow_status
from ukps.record_revisions where id = :revision_id;

-- Answers
select indication, bnf_chapter_id, indication_is_paediatric, indication_is_cancer,
       indication_is_rare_disease
from ukps.medicines_indication_details where revision_id = :revision_id;

select t.therapeutic_area_id, a.label
from ukps.medicines_indication_detail_therapeutic_areas t
join ukps.therapeutic_areas a on a.id = t.therapeutic_area_id
where t.medicines_indication_detail_id =
  (select id from ukps.medicines_indication_details where revision_id = :revision_id);

-- Field audit, in order
select field_path, old_value, new_value, changed_by, changed_at
from ukps.record_field_audit
where revision_id = :revision_id
order by id;
```

The audit query should return six rows, one per page in order.

## Coming back through Update record

Open the record from **View** in the organisation records table, select **Update record** to open the
task list, then choose the task and step through its pages:

- **Saving a page without changing it** writes nothing at all: no answer update, no audit rows,
  and `record_revisions.xmin` stays the same.
- **Changing an answer** writes the same three things as above, with `old_value` set to the
  previous answer. For example, changing cancer from No to Yes writes
  `old_value = No`, `new_value = Yes`.
- **Removing a therapeutic area** deletes its junction row; adding one inserts a row. Areas that
  stay selected are left alone. The audit row holds the whole old and new lists.
- **The indication row is never duplicated**: later saves update the row created in step 1.
