# Record creation walkthrough: database changes per save

This walks through the happy path of creating a medicine record and answering the five
Indication details pages. For each save it lists the tables written and the values to expect,
with SQL to check them. Use it to confirm the flow behaves as intended.

For the design behind it, see the [high-level design](high-level-design.md).

## Conventions

- All tables are in the `ukps` schema.
- Enums are stored as integers. The values used here:

  | Enum | Values |
  |---|---|
  | `RecordType` | `0` Medicine, `1` Vaccine |
  | `RecordStatus` | `0` Unpublished, `1` Active, `2` OnHold, `3` Archived |
  | `WorkflowStatus` | `0` Draft, `1` InReview, `2` Published, `3` Rejected |
  | `RecordEventType` | `0` RecordCreated, `7` RecordContentUpdated |
  | `SubstanceNameType` | `0` GenericName, `1` DevelopmentName |
  | `IndicationPaediatricStatus` | `0` Unknown, `1` ExclusivelyChildren, `2` ExclusivelyAdults, `3` BothChildrenAndAdults |
  | `YesNoUnknown` | `0` Unknown, `1` No, `2` Yes |

- IDs in this document are examples: organisation `3`, user `12`, record `5`, revision `7`,
  product detail `9`. Substitute your own; the queries below use `:record_id` and
  `:revision_id`.
- `T0`, `T1`, … are the times of each save (UTC).
- **Loading a page (GET) writes nothing.** Only the create request and each **Save and continue**
  write to the database.
- Every save that changes an answer also updates the revision's `xmin` (its concurrency token).
  It is a Postgres system column, so select it explicitly: `select xmin, * from ...`.

## The example answers

| Step | Page | Answer entered |
|---|---|---|
| 0 | Create record | Development name `GS-7977`, branded name `Sovaldi`, generic name `sofosbuvir`, record title `Chronic hepatitis C in adults` |
| 1 | Indication | `Chronic hepatitis C infection in adults with genotype 1` |
| 2 | BNF chapter | `5: Infections` (ID `5`) |
| 3 | Therapeutic area | `7: Gastroenterology` and `10: Viral infectious disease` (IDs `7`, `10`) |
| 4 | Treating children | This product is exclusively for adults |
| 5 | Treating cancer | No |

## Step 0: Create record

**Request:** `POST /records` from `/portal/organisations/3/records/create`.

**Response:** `200 { "recordId": 5, "revisionId": 7, "firstPageId": "indication" }`, then the
browser goes to `/portal/records/5/revisions/7/indication`.

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
| `revision_no` | `1` |
| `based_on_revision_id` | `null` |
| `workflow_status` | `0` (Draft) |
| `created_by` / `created_at` | `12` / `T0` |
| `updated_by` / `updated_at` | `null` / `null` |
| `submitted_by` / `submitted_at` | `null` / `null` |
| `xmin` | Set by Postgres; note it, it changes on every later save |

### `record_events` (1 row inserted)

| Column | Value |
|---|---|
| `record_id` | `5` |
| `revision_id` | `7` |
| `event_type` | `0` (RecordCreated) |
| `performed_by` / `performed_at` | `12` / `T0` |
| `note` | `null` |

No `record_event_field_changes` rows are written for record creation.

### `medicines_product_details` (1 row inserted)

| Column | Value |
|---|---|
| `id` | `9` |
| `revision_id` | `7` |
| `record_title` | `Chronic hepatitis C in adults` |
| `branded_name` | `Sovaldi` (`null` if left blank) |
| `indication` | `null` |
| `bnf_chapter_id` | `null` |
| `indication_is_paediatric` | `null` |
| `indication_is_cancer` | `null` |
| All other columns | `null` |

### `medicines_active_substances` (1 row per name inserted)

Development names come first, then generic names. `display_order` starts at `0` within each name
type.

| `medicines_product_detail_id` | `name` | `name_type` | `display_order` |
|---|---|---|---|
| `9` | `GS-7977` | `1` (DevelopmentName) | `0` |
| `9` | `sofosbuvir` | `0` (GenericName) | `0` |

### Check

```sql
select * from ukps.records where id = :record_id;
select * from ukps.record_status_histories where record_id = :record_id;
select xmin, * from ukps.record_revisions where record_id = :record_id;
select * from ukps.record_events where record_id = :record_id order by id;
select * from ukps.medicines_product_details where revision_id = :revision_id;
select * from ukps.medicines_active_substances
where medicines_product_detail_id =
  (select id from ukps.medicines_product_details where revision_id = :revision_id)
order by name_type desc, display_order;
```

## What every page save writes

Steps 1 to 5 each write the same four things, at that step's time `Tn`, in one transaction:

1. **The answer** in its content table (the table differs by page, see each step).
2. **`record_revisions`**, row `7` updated: `updated_by = 12`, `updated_at = Tn`, and `xmin`
   changes. Nothing else on the row changes.
3. **`record_events`**, 1 row inserted: `record_id = 5`, `revision_id = 7`, `event_type = 7`
   (RecordContentUpdated), `performed_by = 12`, `performed_at = Tn`, `note` = the page ID,
   `qa_review_id = null`.
4. **`record_event_field_changes`**, 1 row inserted per changed question, linked by
   `record_event_id`: `field_path` = the question ID, `old_value` = the previous answer,
   `new_value` = the new answer. Single values are stored as plain text and lists as JSON;
   no answer is `null`.

`records`, `record_status_histories` and `medicines_active_substances` are not touched by any page
save.

Each save's response is `200 { "nextPageId": "..." }` and the browser moves to that page.

## Step 1: Indication

**Request:** `PUT /records/5/revisions/7/pages/indication`

```json
{
  "formVersion": "2026.10.1",
  "revisionVersion": <xmin from step 0>,
  "answers": {
    "medicines_product_detail.indication": "Chronic hepatitis C infection in adults with genotype 1"
  }
}
```

**Response:** `{ "nextPageId": "bnf-chapter" }`

| Table | Change |
|---|---|
| `medicines_product_details` (row `9`) | `indication`: `null` → `Chronic hepatitis C infection in adults with genotype 1` |
| `record_revisions` (row `7`) | `updated_by = 12`, `updated_at = T1`, new `xmin` |
| `record_events` | Insert, `event_type = 7`, `note = indication` |
| `record_event_field_changes` | `field_path = medicines_product_detail.indication`, `old_value = null`, `new_value = Chronic hepatitis C infection in adults with genotype 1` |

Leading and trailing spaces are trimmed before saving, so typing `"  text  "` stores `text`.

## Step 2: BNF chapter

**Request:** `PUT /records/5/revisions/7/pages/bnf-chapter`, with answers
`{ "medicines_product_detail.bnf_chapter_id": "5" }`. Its `revisionVersion` is the new `xmin`
from step 1, which the browser received when it loaded the BNF chapter page. Each page load picks
up the latest value in the same way.

**Response:** `{ "nextPageId": "therapeutic-area" }`

| Table | Change |
|---|---|
| `medicines_product_details` (row `9`) | `bnf_chapter_id`: `null` → `5` |
| `record_revisions` (row `7`) | `updated_by = 12`, `updated_at = T2`, new `xmin` |
| `record_events` | Insert, `event_type = 7`, `note = bnf-chapter` |
| `record_event_field_changes` | `field_path = medicines_product_detail.bnf_chapter_id`, `old_value = null`, `new_value = 5` |

The audit stores the chapter ID, not its label.

## Step 3: Therapeutic area

**Request:** `PUT /records/5/revisions/7/pages/therapeutic-area`, with answers
`{ "medicines_product_detail_therapeutic_area": ["7", "10"] }`.

**Response:** `{ "nextPageId": "paediatric" }`

| Table | Change |
|---|---|
| `medicines_product_detail_therapeutic_areas` | 2 rows inserted: (`medicines_product_detail_id = 9`, `therapeutic_area_id = 7`) and (`9`, `10`) |
| `medicines_product_details` | Not changed |
| `record_revisions` (row `7`) | `updated_by = 12`, `updated_at = T3`, new `xmin` |
| `record_events` | Insert, `event_type = 7`, `note = therapeutic-area` |
| `record_event_field_changes` | `field_path = medicines_product_detail_therapeutic_area`, `old_value = null`, `new_value = ["10","7"]` |

The selection is sorted as text before it is compared and audited, which is why `new_value` is
`["10","7"]` rather than `["7","10"]`. If nothing is ticked (the question is optional), no
junction rows are written; a first save with nothing ticked is not a change, so it writes no
audit rows and does not touch the revision.

## Step 4: Treating children

**Request:** `PUT /records/5/revisions/7/pages/paediatric`, with answers
`{ "medicines_product_detail.indication_is_paediatric": "ExclusivelyAdults" }`.

**Response:** `{ "nextPageId": "cancer" }`

| Table | Change |
|---|---|
| `medicines_product_details` (row `9`) | `indication_is_paediatric`: `null` → `2` (ExclusivelyAdults) |
| `record_revisions` (row `7`) | `updated_by = 12`, `updated_at = T4`, new `xmin` |
| `record_events` | Insert, `event_type = 7`, `note = paediatric` |
| `record_event_field_changes` | `field_path = medicines_product_detail.indication_is_paediatric`, `old_value = null`, `new_value = ExclusivelyAdults` |

The content table stores the enum's integer; the audit stores its name.

## Step 5: Treating cancer

**Request:** `PUT /records/5/revisions/7/pages/cancer`, with answers
`{ "medicines_product_detail.indication_is_cancer": "No" }`.

**Response:** `{ "nextPageId": null }`, and the browser goes to `/portal/organisations/3/records`.

| Table | Change |
|---|---|
| `medicines_product_details` (row `9`) | `indication_is_cancer`: `null` → `1` (No) |
| `record_revisions` (row `7`) | `updated_by = 12`, `updated_at = T5`, new `xmin` |
| `record_events` | Insert, `event_type = 7`, `note = cancer` |
| `record_event_field_changes` | `field_path = medicines_product_detail.indication_is_cancer`, `old_value = null`, `new_value = No` |

## Final state

After step 5, for record `5`:

| Table | Rows for this record |
|---|---|
| `records` | 1, unchanged since step 0 |
| `record_status_histories` | 1, unchanged since step 0 |
| `record_revisions` | 1, `updated_at = T5`, `workflow_status = 0` (still Draft) |
| `medicines_product_details` | 1, with `indication`, `bnf_chapter_id = 5`, `indication_is_paediatric = 2`, `indication_is_cancer = 1` |
| `medicines_active_substances` | 2, unchanged since step 0 |
| `medicines_product_detail_therapeutic_areas` | 2 (`7`, `10`) |
| `record_events` | 6: one `event_type = 0`, then five `event_type = 7` with notes `indication`, `bnf-chapter`, `therapeutic-area`, `paediatric`, `cancer` |
| `record_event_field_changes` | 5, one per page |

The record stays `Unpublished` and the revision stays `Draft`; answering the pages does not
submit anything.

### Check

```sql
-- Answers
select xmin, updated_at, updated_by, workflow_status
from ukps.record_revisions where id = :revision_id;

select record_title, branded_name, indication, bnf_chapter_id,
       indication_is_paediatric, indication_is_cancer
from ukps.medicines_product_details where revision_id = :revision_id;

select t.therapeutic_area_id, a.label
from ukps.medicines_product_detail_therapeutic_areas t
join ukps.therapeutic_areas a on a.id = t.therapeutic_area_id
where t.medicines_product_detail_id =
  (select id from ukps.medicines_product_details where revision_id = :revision_id);

-- Audit trail, in order
select e.id, e.event_type, e.note, e.performed_by, e.performed_at,
       c.field_path, c.old_value, c.new_value
from ukps.record_events e
left join ukps.record_event_field_changes c on c.record_event_id = e.id
where e.record_id = :record_id
order by e.id, c.id;
```

The audit query should return six rows: the creation event (with no field change) followed by one
row per page in order.

## Coming back through View or edit

Opening the record from **View or edit** in the records table and stepping through the pages:

- **Saving a page without changing it** writes nothing at all: no answer update, no audit rows,
  and `record_revisions.updated_at` and `xmin` stay the same.
- **Changing an answer** writes the same four things as above, with `old_value` set to the
  previous answer. For example, changing cancer from No to Yes writes
  `old_value = No`, `new_value = Yes`.
- **Removing a therapeutic area** deletes its junction row; adding one inserts a row. Areas that
  stay selected are left alone. The audit row holds the whole old and new lists.
