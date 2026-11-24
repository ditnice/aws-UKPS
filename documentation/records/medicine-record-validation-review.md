# Medicine record creation: validation and error message review

Every validation a user can trigger while creating a medicine record, with its exact message. Covers the create form, all task list pages, cross-page date rules, Check your answers and Submit.

- **Based on:** `main` at `05bfcd4a`, read from the code on 24 November 2026. The app was not run.
- **Size:** 222 rows: 213 with a message (119 distinct) and 9 optional questions with no validation. 96 form questions are covered, counting each question inside a clinical trial once.
- **Not included:** messages a user cannot trigger through the screens, such as API-only errors. The vaccine form and status changes (discard, hold, archive) are out of scope. One rule that cannot currently be reached is kept and marked, IND-04.

## How to use this

Review each row by its ID. The [findings](#findings) are separate and refer to row IDs.

| Column | Meaning |
|---|---|
| ID | Task prefix and number. The numbers are only used in this document. |
| Proposed GDS error message | Suggested wording based on the [GOV.UK guidance](#govuk-guidance-used). Not applied to the code. "No change" means the current message already fits. Square brackets mark text filled in at runtime. "Not real: … Year not 4 digits: …" means the row needs separate messages. "Page, not error message" means GOV.UK says to use a page instead. |
| Shown when | The condition on the question. Conditions on a whole page are stated above its table. Hidden questions are never validated, and their saved answers are cleared. |
| Seen on | **Page**: beside the question when saving. **CYA**: in the Check your answers error summary. |
| Source | Where the message is defined. |

## How validation behaves

- The first check is on submit; after that, answers are rechecked when the user leaves a field.
- Page rules run in the browser and again in the API. Each question shows one message at a time: date checks first, then required, maximum length and maximum entries. For a prefixed number (such as the NCT number), required and length come before format.
- Text is trimmed, so spaces alone count as blank.
- Cross-page rules run only on Check your answers and Submit, and each question shows only its first error there.
- Where a rule applies to a date, "today" is the UTC date.

## Contents

- [GOV.UK guidance used](#govuk-guidance-used)
- [Findings](#findings)
- Sections 1 to 11: the inventory

## GOV.UK guidance used

Read on 24 November 2026 from the [error message](https://design-system.service.gov.uk/components/error-message/) page and the pages it links to. This project uses NICE Design System components; only the content guidance is applied here.

| Situation | GOV.UK template | Source | Applied to |
|---|---|---|---|
| Empty text | Enter [whatever it is] | [Text input](https://design-system.service.gov.uk/components/text-input/), [Textarea](https://design-system.service.gov.uk/components/textarea/) | Required text |
| Text too long | [whatever it is] must be [number] characters or less | Text input, Textarea | Maximum length |
| Characters not allowed | [whatever it is] must only include [allowed characters] | Text input | ID numbers |
| Yes or no radios | Select yes if [whatever it is is true] | [Radios](https://design-system.service.gov.uk/components/radios/) | CLT-01 only |
| Two other options | Select if [whatever it is] | Radios | PIM-23 only |
| More than two radios | Select [whatever it is] | Radios | All other radios. Yes, no and unknown is three options, so these use "Select whether…" |
| Checkboxes, options not in the question | Select [whatever it is] | [Checkboxes](https://design-system.service.gov.uk/components/checkboxes/) | IND-14, HTA-02, PRI-05 |
| Date empty | Enter [whatever it is] | [Date input](https://design-system.service.gov.uk/components/date-input/) | Dates |
| Date incomplete | [whatever it is] must include a [whatever is missing] | Date input | Dates |
| Year too short | Year must include 4 numbers | Date input | Dates |
| Date cannot be correct | [whatever it is] must be a real date | Date input | Dates |
| Date in the future | [whatever it is] must be today or in the past | Date input | Actual dates |
| Compared with another date | must be the same as or before / the same as or after / before / after | Date input | XPR rows |
| Service problem | Sorry, there is a problem with the service | [Problem with the service pages](https://design-system.service.gov.uk/patterns/problem-with-the-service-pages/) | CRE-08, SAV-01, CYA-11, LOD rows |
| Permission, or a problem the user cannot fix | Use a page, not an error message | Error message | SAV-02, CYA-04 to CYA-06, CYA-09, LOD-03 |

General rules applied: reuse the question's wording, be specific and plain, use the same message beside the field and in the summary, and avoid "please", "sorry", "valid" and "invalid" in error messages. GOV.UK lists "Select an option" by name as a message to avoid (F1).

No GOV.UK template was found for: repeated entries, too many entries, the choice of date precision, an estimated month or quarter that has ended, all-zero IDs, an archived option, or someone else changing the record. Those proposals are my own wording in the same style. The Select page has no wording template, so dropdowns follow the radios pattern.

## Findings

Grouped by how much they affect users. These are observations from the code. Items marked **Confirm** may be intended.

### High

**F1. Check your answers can list many identical errors.** "Select an option" is the required message on 42 questions, 29 of them in mandatory sections. On a new record, 17 of these apply straight away, so the error summary shows 17 links with the same text. "Enter details" is used 14 times (3 mandatory: DEV-04, DEV-07, HTA-03). Suggest naming the subject in each message, e.g. "Select whether this product is intended to treat cancer".

**F2. 34 text questions have no maximum length.** Only 7 have one (DEV-02, DEV-05, DEV-08, IND-02, MHR-14, NAM-07, MHR-02). Nothing in the browser or API limits the rest. **Confirm** the limits needed for:

  - Provide product record details: Company code; Branded name (optional); Other names and identifiers (optional); Active substance
  - Presentation: Describe the presentation of this product (optional)
  - Mode of action: What is the mode of action of this product? (optional)
  - Proposed dose regimen: What is the proposed dose regimen for this product? (optional)
  - Clinical trial details: Study name; Trial number from other registry (optional)
  - HTA submission: Add additional details or clarifications, such as any planned differences in the indications submitted to each HTA
  - Orphan medicine status: EU orphan status number
  - Non-genomic biomarker: Describe the non-genomic biomarker
  - Genomic target: What is the genomic target for this test?
  - Sample type: What sample type is needed?
  - Turnaround time: Describe any turnaround time requirements for this test that differ from the current routinely commissioned testing pathway (optional)
  - Patient pathway: Describe when in the care pathway the test is expected to take place
  - Genomic alterations: What genomic alterations determine patient eligibility?
  - Additional genomic factors: Are there additional genomic factors that affect treatment selection or sequencing?
  - Test used in trials: What test was used in trials?
  - Specificity and sensitivity: Enter the minimum specificity and sensitivity used
  - Mandatory testing: Enter any notes about uncertainties, assumptions, or service considerations relating to the tests
  - Monitoring tests: Describe any tests (genomic tests or otherwise) needed to monitor response to treatment beyond what is currently offered in NHS practice
  - Safety tests: Describe any tests needed to assess safety of the treatment that are additional to what is currently used in routine clinical practice
  - Screening or surveillance: Briefly describe the type of screening or surveillance needed
  - Urgent patient identification: Provide further details
  - Proposed place in therapy: Enter the proposed place in therapy and likely comparators
  - Estimated duration of treatment: Enter the estimated duration of treatment
  - NHS service changes: Describe any changes to staffing, equipment, pathway, or commissioning needed for delivery of this medicine.
  - Handling and storage: Provide further details
  - UK patient population: Additional details about the UK patient population
  - Enter the estimated UK eligible population range: Enter the estimated UK eligible population range
  - Estimated uptake: Enter the estimated uptake based on expected adoption patterns (optional)
  - Compassionate or early access: Provide further details
  - Indication specific pricing: Provide further details

**F3. Form pages and the create form have no error summary.** Errors show only beside each question, and only Check your answers has a summary. `documentation/records/validation-and-concurrency.md` says a failed page save shows an "Error summary".

### Medium

**F4. The same names questions behave differently on create and edit.**

| | Create form | Names and identifiers page |
|---|---|---|
| Blank extra active substance | Rejected (CRE-02) | Dropped without a message |
| Duplicate active substance | One message above the list (CRE-03) | Message beside each repeat (NAM-05) |

**F5. UK and international dates share messages.** Both submission dates use "submission" and both licence dates use "licence", so MHR-04 to MHR-12 match MHR-32 to MHR-40, and MHR-19 to MHR-27 match MHR-42 to MHR-50. The "must be in the future" ones can appear together on Check your answers.

**F6. "Must be in the future" is stricter than the rule.** An estimated month or quarter is accepted while it is still in progress, and rejected only once it has ended. Affects MHR-09, MHR-24, MHR-37, MHR-47, HTA-14, PIM-09, PIM-19. Saved estimates also go stale: they can pass when saved and fail at submission.

**F7. Fixed-format dates have weaker messages than the others.** For MHR-16, ORP-03 and ORP-08, a missing part and an impossible date both give "Enter a real date", and every part is marked invalid. Other dates have separate messages and mark only the parts at fault. MHR-17 does not name its date, and ORP-04 reads "Actual date of EU orphan medicine status", without "granted".

**F8. An ID type with no number is discarded silently.** On the NICE technology appraisal ID, choosing "GID-TA" or "GID-HST" and leaving the number blank saves as unanswered with no message. **Confirm**.

**F9. SAV-02 can be wrong.** It says the record "has been submitted", but also appears when the record is on hold or archived. CYA-06 covers all three.

**F10. Permission errors tell the user to try again.** SAV-01 and CYA-11 appear for no permission, where trying again will not help. CRE-08 gives no reason at all, including when the organisation is inactive.

**F11. "Required" in the optional service readiness section.** Pages cannot be saved without an answer (LAB, PAT and PRI rows), but the record can be submitted without them. The code has a TODO for the section becoming mandatory within 12 months of market authorisation, which is not built, although the screens already say so. **Confirm** the interim behaviour.

**F12. Duplicates are not checked in some lists.** The same NCT number can be used for two trials, and the same value entered twice in Other names and identifiers (NAM-02). Other registry numbers are only compared within one trial (CLT-07). **Confirm**.

**F13. Errors inside lists would not reach Check your answers.** `RevisionValidator` reports errors only for whole questions, so an error on one trial's field or one list entry is found and then dropped, and would not block Submit. Page saves prevent this today. It would matter for data written another way, or if a rule is tightened later.

### Low: wording and consistency

| # | Rows | Observation |
|---|---|---|
| F14 | CRE-01, CRE-02, CRE-04 | Articles vary: "Enter company code", "Enter generic name", "Enter **a** record title". CLT-03 also has none. |
| F15 | CRE-05, NAM-07 | "Record title must be between 1 and 100 characters"; other limits say "must be N characters or fewer". |
| F16 | CRE-03, CLT-07 | Two styles for the same rule: "must be distinct" and "must be different for each entry". |
| F17 | CRE-02, CRE-09 | The question is "Active substance" but messages say "generic name". |
| F18 | DEV-04, DEV-07 | "Enter details" for a company name field. |
| F19 | MHR-14 | "Global first submission must be 200 characters or fewer" is about its location. |
| F20 | DEV-02 | "Differences must be 2000 characters or fewer" is unclear without the question. |
| F21 | MHR-15 | Uses the acronym "GFS" alone. |
| F22 | IND-03, IND-09, PAT-11 | "Choose" and "Select" both used for dropdowns. |
| F23 | IND-14 | Only required message with a second sentence. |
| F24 | CLT-04 | Message says "ClinicalTrials.gov"; the label says "clinical trials.gov". |
| F25 | CLT-05, HTA-06 | "Must be a whole positive number" for an identifier; "must only contain numbers" may be clearer. |
| F26 | HTA-05 | "Select ID type" has no article. |
| F27 | SAV-04 | "This record was modified concurrently" is technical wording. |
| F28 | HTA-03, LAB-11, LAB-16, LAB-17 | Required, but the question reads as optional or may have no answer. **Confirm**. |
| F29 | XPR-07, XPR-08 | Message says "international"; the questions say "International Recognition Procedure". |
| F30 | CRE-04 and similar | The create form's wording is in `errorMessages.ts` and the page's is in `RecordProductDetail.cs`. They match now but can drift. |
| F31 | HTA-02 | Not an error message: this question's hint links to the placeholder `https://example.com`. |

### Differences from GOV.UK guidance

| # | Guidance | Current behaviour |
|---|---|---|
| F32 | Always show an error summary and move keyboard focus to it ([Error summary](https://design-system.service.gov.uk/components/error-summary/)) | Missing on form pages and the create form (F3) |
| F33 | Do not validate when the user moves away from a field ([Validation](https://design-system.service.gov.uk/patterns/validation/)) | After the first submit, fields are rechecked on leaving them |
| F34 | Ignore unwanted characters, such as spaces, in numbers and codes (Validation) | A space inside an NCT or NICE ID number is rejected (CLT-05, HTA-06) |
| F35 | Keep permission and service problems out of error messages (Error message) | Shown as inline errors on the form (CRE-08, SAV-01, SAV-02, CYA-11) |
| F36 | Avoid "please" and "An error occurred" (Error message) | Used in CRE-08, SAV-01, CYA-11 and most LOD rows |
| F37 | Add "Error: " to the page title after a failed submit (Validation) | No code for this was found in the form components read; confirm in the running app |

## 1. Create record form

Path: `/portal/organisations/{id}/records/create`. Company code, Active substance and Record title are required.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| CRE-01 | Company code | Required | `Enter company code` | `Enter a company code` | Always | Page | [errorMessages.ts:27](../../frontend/src/lib/form/errorMessages.ts#L27) |
| CRE-02 | Active substance (Generic name) | Required: every entry shown must be filled in | `Enter generic name` | `Enter a generic name` | Always | Page, beside each blank entry | [errorMessages.ts:28](../../frontend/src/lib/form/errorMessages.ts#L28) |
| CRE-03 | Active substance (Generic name) | Each entry must be different (ignoring case) | `Generic names must be distinct` | `Generic name must be different for each entry` | Always | Page, above the list | [errorMessages.ts:40](../../frontend/src/lib/form/errorMessages.ts#L40) |
| CRE-04 | Record title | Required | `Enter a record title` | _No change_ | Always | Page | [errorMessages.ts:29](../../frontend/src/lib/form/errorMessages.ts#L29) |
| CRE-05 | Record title | Maximum 100 characters | `Record title must be between 1 and 100 characters` | `Record title must be 100 characters or less` | Always | Page | [errorMessages.ts:41](../../frontend/src/lib/form/errorMessages.ts#L41) |
| CRE-06 | Other names and identifiers (optional) | Optional | _No validation_ | - | Always | - | [CreateMedicineRecordForm.tsx:137](../../frontend/src/app/portal/organisations/%5Bid%5D/records/create/CreateMedicineRecordForm.tsx#L137) |
| CRE-07 | Branded name (optional) | Optional | _No validation_ | - | Always | - | [CreateMedicineRecordForm.tsx:155](../../frontend/src/app/portal/organisations/%5Bid%5D/records/create/CreateMedicineRecordForm.tsx#L155) |
| CRE-08 | Whole form | Any failed request: no permission, organisation inactive, API validation, server or network error | `An error occurred when creating a new medicine record.` | `Page, not error message: Sorry, there is a problem with the service. Your record has not been created. Try again later.` | After submitting | Top of form | [errorMessages.ts:49](../../frontend/src/lib/form/errorMessages.ts#L49) |
| CRE-09 | Whole page | Back selected for the first time (a warning, not an error) | `If you continue, this record will not be saved` | _No change_ | Always | Top of page | [CreateRecordPageHeader.tsx:21](../../frontend/src/app/portal/organisations/%5Bid%5D/records/create/CreateRecordPageHeader.tsx#L21) |

The warning's body text is: "You must enter the company code, generic name, and record title for this record to be saved as a draft. You can update these answers later."

## 2. Provide product record details

### Names and identifiers

#### Provide product record details (`names-and-identifiers`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| NAM-01 | Company code | Required | `Enter company code` | `Enter a company code` | Always | Page, CYA | [RecordProductDetail.cs:35](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordProductDetail.cs#L35) |
| NAM-02 | Other names and identifiers (optional) | Optional | _No validation_ | - | Always | - | [RecordProductDetail.cs:74](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordProductDetail.cs#L74) |
| NAM-03 | Branded name (optional) | Optional | _No validation_ | - | Always | - | [RecordProductDetail.cs:39](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordProductDetail.cs#L39) |
| NAM-04 | Active substance | Required | `Enter generic name` | `Enter a generic name` | Always | Page, CYA | [RecordProductDetail.cs:100](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordProductDetail.cs#L100) |
| NAM-05 | Active substance | Each entry must be different (ignoring case) | `Generic names must be distinct` | `Generic name must be different for each entry` | Always | Page, beside each repeat | [RecordProductDetail.cs:101](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordProductDetail.cs#L101) |
| NAM-06 | Record title | Required | `Enter a record title` | _No change_ | Always | Page, CYA | [RecordProductDetail.cs:60](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordProductDetail.cs#L60) |
| NAM-07 | Record title | Maximum 100 characters | `Record title must be between 1 and 100 characters` | `Record title must be 100 characters or less` | Always | Page | [RecordProductDetail.cs:61](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordProductDetail.cs#L61) |

## 3. Provide indication and development information

### Indication details

#### Indication (`indication`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| IND-01 | What is the indication this product is seeking a licence for? | Required | `Enter the indication` | `Enter the indication this product is seeking a licence for` | Always | Page, CYA | [MedicinesIndicationDetail.cs:22](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesIndicationDetail.cs#L22) |
| IND-02 | What is the indication this product is seeking a licence for? | Maximum 2000 characters | `Indication must be 2000 characters or fewer` | `Indication must be 2,000 characters or less` | Always | Page | [MedicinesIndicationDetail.cs:23](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesIndicationDetail.cs#L23) |

#### BNF chapter (`bnf-chapter`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| IND-03 | Select the BNF chapter for this product | Required | `Choose a BNF chapter` | `Select the BNF chapter for this product` | Always | Page, CYA | [MedicinesIndicationDetail.cs:39](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesIndicationDetail.cs#L39) |

#### Therapeutic area (`therapeutic-area`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| IND-04 | Select the most appropriate therapeutic area(s) for this product (optional) | Maximum 3 entries | `Select up to 3 therapeutic areas` | `Therapeutic areas must be 3 or fewer` | Always | Not reachable: the add button is hidden at the limit | [MedicinesIndicationDetail.cs:158](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesIndicationDetail.cs#L158) |
| IND-05 | Select the most appropriate therapeutic area(s) for this product (optional) | The same option chosen in two entries | `You have selected the same option more than once` | `Therapeutic area must be different for each entry` | Always | Page | [answers.ts:28](../../frontend/src/lib/dynamic-form/answers.ts#L28) |

#### Treating children (`paediatric`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| IND-06 | Is this product intended to treat children? | Required | `Select an option` | `Select whether this product is intended to treat children` | Always | Page, CYA | [MedicinesIndicationDetail.cs:48](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesIndicationDetail.cs#L48) |

#### Treating cancer (`cancer`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| IND-07 | Is this a product intended to treat cancer? | Required | `Select an option` | `Select whether this product is intended to treat cancer` | Always | Page, CYA | [MedicinesIndicationDetail.cs:57](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesIndicationDetail.cs#L57) |

#### Treating a rare disease (`rare-disease`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| IND-08 | Is this a product intended to treat a rare disease? | Required | `Select an option` | `Select whether this product is intended to treat a rare disease` | Always | Page, CYA | [MedicinesIndicationDetail.cs:68](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesIndicationDetail.cs#L68) |

#### Formulation (`formulation`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| IND-09 | What is the formulation? | Required | `Choose a formulation` | `Select the formulation` | Always | Page, CYA | [MedicinesIndicationDetail.cs:80](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesIndicationDetail.cs#L80) |

#### Presentation (`presentation`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| IND-10 | Describe the presentation of this product (optional) | Optional | _No validation_ | - | Always | - | [MedicinesIndicationDetail.cs:84](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesIndicationDetail.cs#L84) |

#### Mode of action (`mode-of-action`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| IND-11 | What is the mode of action of this product? (optional) | Optional | _No validation_ | - | Always | - | [MedicinesIndicationDetail.cs:93](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesIndicationDetail.cs#L93) |

#### Proposed dose regimen (`dose-regimen`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| IND-12 | What is the proposed dose regimen for this product? (optional) | Optional | _No validation_ | - | Always | - | [MedicinesIndicationDetail.cs:102](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesIndicationDetail.cs#L102) |

#### Personalised medicine (`personalised-medicine`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| IND-13 | Is this product a personalised medicine? | Required | `Select an option` | `Select whether this product is a personalised medicine` | Always | Page, CYA | [MedicinesIndicationDetail.cs:117](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesIndicationDetail.cs#L117) |

#### Technology status (`technology-status`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| IND-14 | What is the technology status for this product? | Required | `Select the technology status of this product. You can select multiple statuses.` | `Select the technology status for this product` | Always | Page, CYA | [MedicinesIndicationDetail.cs:129](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesIndicationDetail.cs#L129) |

### Development background

#### Repurposed medicine (`repurposed-medicine`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| DEV-01 | Is this product a repurposed medicine? | Required | `Select an option` | `Select whether this product is a repurposed medicine` | Always | Page, CYA | [MedicinesDevelopmentBackground.cs:22](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesDevelopmentBackground.cs#L22) |
| DEV-02 | Enter the differences from the current licensed indications (optional) | Maximum 2000 characters | `Differences must be 2000 characters or fewer` | `Differences from the current licensed indications must be 2,000 characters or less` | "Is this product a repurposed medicine?" is **Yes** | Page | [MedicinesDevelopmentBackground.cs:35](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesDevelopmentBackground.cs#L35) |

#### Original developer (`originator`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| DEV-03 | Is your company the original developer of this product? | Required | `Select an option` | `Select whether your company is the original developer of this product` | Always | Page, CYA | [MedicinesDevelopmentBackground.cs:46](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesDevelopmentBackground.cs#L46) |
| DEV-04 | Originator company name | Required | `Enter details` | `Enter the originator company name` | "Is your company the original developer of this product?" is **No** | Page, CYA | [MedicinesDevelopmentBackground.cs:59](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesDevelopmentBackground.cs#L59) |
| DEV-05 | Originator company name | Maximum 200 characters | `Originator company name must be 200 characters or fewer` | `Originator company name must be 200 characters or less` | "Is your company the original developer of this product?" is **No** | Page | [MedicinesDevelopmentBackground.cs:60](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesDevelopmentBackground.cs#L60) |

#### Co-marketing (`co-marketing`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| DEV-06 | Is this product co-marketed? | Required | `Select an option` | `Select whether this product is co-marketed` | Always | Page, CYA | [MedicinesDevelopmentBackground.cs:71](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesDevelopmentBackground.cs#L71) |
| DEV-07 | Co-marketing company name | Required | `Enter details` | `Enter the co-marketing company name` | "Is this product co-marketed?" is **Yes** | Page, CYA | [MedicinesDevelopmentBackground.cs:84](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesDevelopmentBackground.cs#L84) |
| DEV-08 | Co-marketing company name | Maximum 200 characters | `Co-marketing company name must be 200 characters or fewer` | `Co-marketing company name must be 200 characters or less` | "Is this product co-marketed?" is **Yes** | Page | [MedicinesDevelopmentBackground.cs:85](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesDevelopmentBackground.cs#L85) |

## 4. Provide clinical trial information

### Clinical trials

#### Add a clinical trial (`add-clinical-trial`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| CLT-01 | Do you wish to add a clinical trial? | Required | `Select an option` | `Select yes if you wish to add a clinical trial` | Always | Page, CYA | [RecordClinicalTrialInformation.cs:38](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordClinicalTrialInformation.cs#L38) |

#### Clinical trial details (`clinical-trial-details`)

Page shown when: "Do you wish to add a clinical trial?" is **Yes**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| CLT-02 | Enter clinical trial details | Required: at least one trial | `Add at least one clinical trial` | _No change_ | Always | CYA | [RecordClinicalTrialInformation.cs:67](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordClinicalTrialInformation.cs#L67) |
| CLT-03 | Clinical trial > Study name | Required | `Enter study name` | `Enter a study name` | Always | Page | [RecordClinicalTrial.cs:30](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordClinicalTrial.cs#L30) |
| CLT-04 | Clinical trial > Trial number from clinical trials.gov | Required | `Enter the trial number from ClinicalTrials.gov` | _No change_ | Always | Page | [RecordClinicalTrial.cs:42](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordClinicalTrial.cs#L42) |
| CLT-05 | Clinical trial > Trial number from clinical trials.gov | Number contains anything other than digits, or is all zeros | `Trial number must be a whole positive number` | `Not only numbers: Trial number must only include numbers. All zeros: Trial number must not be all zeros` | Always | Page | [prefixedNumber.ts:6](../../frontend/src/lib/dynamic-form/prefixedNumber.ts#L6) |
| CLT-06 | Clinical trial > Trial number from clinical trials.gov | Number is not exactly 8 digits | `Trial number must be 8 digits long` | `Trial number must include 8 numbers` | Always | Page | [prefixedNumber.ts:8](../../frontend/src/lib/dynamic-form/prefixedNumber.ts#L8) |
| CLT-07 | Clinical trial > Trial number from other registry (optional) | Each entry must be different (ignoring case) | `Trial number from other registry must be different for each entry` | _No change_ | Always | Page, beside each repeat | [RecordClinicalTrial.cs:70](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordClinicalTrial.cs#L70) |

#### Recruiting in the UK (`recruiting-in-uk`)

Page shown when: "Do you wish to add a clinical trial?" is **Yes**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| CLT-08 | Are any clinical trials recruiting in the UK? | Required | `Select an option` | `Select whether any clinical trials are recruiting in the UK` | Always | Page, CYA | [RecordClinicalTrialInformation.cs:49](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordClinicalTrialInformation.cs#L49) |

## 5. Provide regulatory, access, and launch information

### Medicines and Healthcare products Regulatory Agency (MHRA) procedure and dates

#### UK market authorisation (`uk-market-authorisation`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| MHR-01 | Select the procedure you currently expect to use for UK authorisation | Required | `Select an option` | `Select the procedure you currently expect to use for UK authorisation` | Always | Page, CYA | [RecordMhraProcedure.cs:23](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordMhraProcedure.cs#L23) |
| MHR-02 | MHRA regulatory procedure details (optional) | Maximum 2000 characters | `MHRA regulatory procedure details must be 2000 characters or fewer` | `MHRA regulatory procedure details must be 2,000 characters or less` | Always | Page | [RecordMhraProcedure.cs:47](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordMhraProcedure.cs#L47) |

#### UK regulatory submission date (`uk-submission-date`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| MHR-03 | What is the UK regulatory submission date? | Required | `Select an option` | `Select an estimated quarter, estimated month or actual date for the UK regulatory submission` | Always | Page, CYA | [RecordMhraDate.cs:24](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordMhraDate.cs#L24) |
| MHR-04 | What is the UK regulatory submission date? | Estimated quarter: quarter and year both blank | `Enter the estimated submission date` | `Enter the estimated UK regulatory submission date` | Always | Page | [dates.ts:17](../../frontend/src/lib/dynamic-form/dates.ts#L17) |
| MHR-05 | What is the UK regulatory submission date? | Estimated quarter: quarter blank | `Select a quarter` | `Estimated UK regulatory submission date must include a quarter` | Always | Page | [dates.ts:19](../../frontend/src/lib/dynamic-form/dates.ts#L19) |
| MHR-06 | What is the UK regulatory submission date? | Estimated quarter: year blank | `Enter a year` | `Estimated UK regulatory submission date must include a year` | Always | Page | [dates.ts:20](../../frontend/src/lib/dynamic-form/dates.ts#L20) |
| MHR-07 | What is the UK regulatory submission date? | Estimated month: month or year (or both) blank | `Enter the estimated submission date` | `Both blank: Enter the estimated UK regulatory submission date. One blank: Estimated UK regulatory submission date must include a [month or year]` | Always | Page | [dates.ts:17](../../frontend/src/lib/dynamic-form/dates.ts#L17) |
| MHR-08 | What is the UK regulatory submission date? | Estimated quarter or month: not a real date, or year not 4 digits from 1000 | `Estimated date of submission must be a real date` | `Not real: Estimated UK regulatory submission date must be a real date. Year not 4 digits: Year must include 4 numbers` | Always | Page | [dates.ts:9](../../frontend/src/lib/dynamic-form/dates.ts#L9) |
| MHR-09 | What is the UK regulatory submission date? | Estimated quarter or month has already ended | `Estimated date of submission must be in the future` | `Quarter: Estimated UK regulatory submission date must be this quarter or in the future. Month: Estimated UK regulatory submission date must be this month or in the future` | Always | Page, CYA | [dates.ts:13](../../frontend/src/lib/dynamic-form/dates.ts#L13) |
| MHR-10 | What is the UK regulatory submission date? | Actual date: any part blank | `Enter the actual submission date` | `All blank: Enter the actual UK regulatory submission date. Some blank: Actual UK regulatory submission date must include a [missing parts]` | Always | Page | [dates.ts:17](../../frontend/src/lib/dynamic-form/dates.ts#L17) |
| MHR-11 | What is the UK regulatory submission date? | Actual date: not a real date, or year not 4 digits from 1000 | `Actual date of submission must be a real date` | `Not real: Actual UK regulatory submission date must be a real date. Year not 4 digits: Year must include 4 numbers` | Always | Page | [dates.ts:9](../../frontend/src/lib/dynamic-form/dates.ts#L9) |
| MHR-12 | What is the UK regulatory submission date? | Actual date is after today (UTC) | `Actual date of submission must be today or in the past` | `Actual UK regulatory submission date must be today or in the past` | Always | Page | [dates.ts:11](../../frontend/src/lib/dynamic-form/dates.ts#L11) |

#### Global first submission (GFS) (`global-first-submission`)

Page shown when: "What is the UK regulatory submission date?" is **an actual date**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| MHR-13 | Enter the location of the GFS of this product | Required | `Enter a location` | `Enter the location of the GFS of this product` | Always | Page, CYA | [MedicinesGlobalSubmission.cs:33](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesGlobalSubmission.cs#L33) |
| MHR-14 | Enter the location of the GFS of this product | Maximum 200 characters | `Global first submission must be 200 characters or fewer` | `Location of the GFS must be 200 characters or less` | Always | Page | [MedicinesGlobalSubmission.cs:34](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesGlobalSubmission.cs#L34) |
| MHR-15 | Enter the date of GFS of this product | Required | `Enter the date of GFS` | `Enter the date of GFS of this product` | Always | Page, CYA | [MedicinesGlobalSubmission.cs:47](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesGlobalSubmission.cs#L47) |
| MHR-16 | Enter the date of GFS of this product | Not a real month and year, or only one part filled in | `Enter a real date` | `Some blank: Date of GFS must include a [month or year]. Not real: Date of GFS must be a real date. Year not 4 digits: Year must include 4 numbers` | Always | Page | [dates.ts:7](../../frontend/src/lib/dynamic-form/dates.ts#L7) |
| MHR-17 | Enter the date of GFS of this product | Month is after this month (UTC) | `The date must be this month or in the past` | `Date of GFS must be this month or in the past` | Always | Page | [dates.ts:8](../../frontend/src/lib/dynamic-form/dates.ts#L8) |

#### UK regulatory licence date (`uk-licence-date`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| MHR-18 | What is the UK regulatory licence date? | Required | `Select an option` | `Select an estimated quarter, estimated month or actual date for the UK regulatory licence` | Always | Page, CYA | [RecordMhraDate.cs:35](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordMhraDate.cs#L35) |
| MHR-19 | What is the UK regulatory licence date? | Estimated quarter: quarter and year both blank | `Enter the estimated licence date` | `Enter the estimated UK regulatory licence date` | Always | Page | [dates.ts:17](../../frontend/src/lib/dynamic-form/dates.ts#L17) |
| MHR-20 | What is the UK regulatory licence date? | Estimated quarter: quarter blank | `Select a quarter` | `Estimated UK regulatory licence date must include a quarter` | Always | Page | [dates.ts:19](../../frontend/src/lib/dynamic-form/dates.ts#L19) |
| MHR-21 | What is the UK regulatory licence date? | Estimated quarter: year blank | `Enter a year` | `Estimated UK regulatory licence date must include a year` | Always | Page | [dates.ts:20](../../frontend/src/lib/dynamic-form/dates.ts#L20) |
| MHR-22 | What is the UK regulatory licence date? | Estimated month: month or year (or both) blank | `Enter the estimated licence date` | `Both blank: Enter the estimated UK regulatory licence date. One blank: Estimated UK regulatory licence date must include a [month or year]` | Always | Page | [dates.ts:17](../../frontend/src/lib/dynamic-form/dates.ts#L17) |
| MHR-23 | What is the UK regulatory licence date? | Estimated quarter or month: not a real date, or year not 4 digits from 1000 | `Estimated date of licence must be a real date` | `Not real: Estimated UK regulatory licence date must be a real date. Year not 4 digits: Year must include 4 numbers` | Always | Page | [dates.ts:9](../../frontend/src/lib/dynamic-form/dates.ts#L9) |
| MHR-24 | What is the UK regulatory licence date? | Estimated quarter or month has already ended | `Estimated date of licence must be in the future` | `Quarter: Estimated UK regulatory licence date must be this quarter or in the future. Month: Estimated UK regulatory licence date must be this month or in the future` | Always | Page, CYA | [dates.ts:13](../../frontend/src/lib/dynamic-form/dates.ts#L13) |
| MHR-25 | What is the UK regulatory licence date? | Actual date: any part blank | `Enter the actual licence date` | `All blank: Enter the actual UK regulatory licence date. Some blank: Actual UK regulatory licence date must include a [missing parts]` | Always | Page | [dates.ts:17](../../frontend/src/lib/dynamic-form/dates.ts#L17) |
| MHR-26 | What is the UK regulatory licence date? | Actual date: not a real date, or year not 4 digits from 1000 | `Actual date of licence must be a real date` | `Not real: Actual UK regulatory licence date must be a real date. Year not 4 digits: Year must include 4 numbers` | Always | Page | [dates.ts:9](../../frontend/src/lib/dynamic-form/dates.ts#L9) |
| MHR-27 | What is the UK regulatory licence date? | Actual date is after today (UTC) | `Actual date of licence must be today or in the past` | `Actual UK regulatory licence date must be today or in the past` | Always | Page | [dates.ts:11](../../frontend/src/lib/dynamic-form/dates.ts#L11) |

#### UK conditional approval (`uk-conditional-approval`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| MHR-28 | Is conditional approval anticipated in the UK? | Required | `Select an option` | `Select whether conditional approval is anticipated in the UK` | Always | Page, CYA | [RecordMhraDate.cs:45](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordMhraDate.cs#L45) |

#### International Recognition Procedure (IRP) (`international-recognition`)

Page shown when: "Select the procedure you currently expect to use for UK authorisation" is **International Recognition Procedure**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| MHR-29 | Select the reference regulator | Required | `Select an option` | `Select the reference regulator` | Always | Page, CYA | [RecordMhraProcedure.cs:36](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordMhraProcedure.cs#L36) |
| MHR-30 | Select the intended route | Required | `Select an option` | `Select the intended route` | Always | Page, CYA | [MedicinesIntlRecognition.cs:48](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesIntlRecognition.cs#L48) |

#### International regulatory submission date (`international-submission-date`)

Page shown when: "Select the procedure you currently expect to use for UK authorisation" is **International Recognition Procedure**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| MHR-31 | What is the International Recognition Procedure submission date? | Required | `Select an option` | `Select an estimated quarter, estimated month or actual date for the International Recognition Procedure submission` | Always | Page, CYA | [MedicinesIntlRecognition.cs:59](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesIntlRecognition.cs#L59) |
| MHR-32 | What is the International Recognition Procedure submission date? | Estimated quarter: quarter and year both blank | `Enter the estimated submission date` | `Enter the estimated International Recognition Procedure submission date` | Always | Page | [dates.ts:17](../../frontend/src/lib/dynamic-form/dates.ts#L17) |
| MHR-33 | What is the International Recognition Procedure submission date? | Estimated quarter: quarter blank | `Select a quarter` | `Estimated International Recognition Procedure submission date must include a quarter` | Always | Page | [dates.ts:19](../../frontend/src/lib/dynamic-form/dates.ts#L19) |
| MHR-34 | What is the International Recognition Procedure submission date? | Estimated quarter: year blank | `Enter a year` | `Estimated International Recognition Procedure submission date must include a year` | Always | Page | [dates.ts:20](../../frontend/src/lib/dynamic-form/dates.ts#L20) |
| MHR-35 | What is the International Recognition Procedure submission date? | Estimated month: month or year (or both) blank | `Enter the estimated submission date` | `Both blank: Enter the estimated International Recognition Procedure submission date. One blank: Estimated International Recognition Procedure submission date must include a [month or year]` | Always | Page | [dates.ts:17](../../frontend/src/lib/dynamic-form/dates.ts#L17) |
| MHR-36 | What is the International Recognition Procedure submission date? | Estimated quarter or month: not a real date, or year not 4 digits from 1000 | `Estimated date of submission must be a real date` | `Not real: Estimated International Recognition Procedure submission date must be a real date. Year not 4 digits: Year must include 4 numbers` | Always | Page | [dates.ts:9](../../frontend/src/lib/dynamic-form/dates.ts#L9) |
| MHR-37 | What is the International Recognition Procedure submission date? | Estimated quarter or month has already ended | `Estimated date of submission must be in the future` | `Quarter: Estimated International Recognition Procedure submission date must be this quarter or in the future. Month: Estimated International Recognition Procedure submission date must be this month or in the future` | Always | Page, CYA | [dates.ts:13](../../frontend/src/lib/dynamic-form/dates.ts#L13) |
| MHR-38 | What is the International Recognition Procedure submission date? | Actual date: any part blank | `Enter the actual submission date` | `All blank: Enter the actual International Recognition Procedure submission date. Some blank: Actual International Recognition Procedure submission date must include a [missing parts]` | Always | Page | [dates.ts:17](../../frontend/src/lib/dynamic-form/dates.ts#L17) |
| MHR-39 | What is the International Recognition Procedure submission date? | Actual date: not a real date, or year not 4 digits from 1000 | `Actual date of submission must be a real date` | `Not real: Actual International Recognition Procedure submission date must be a real date. Year not 4 digits: Year must include 4 numbers` | Always | Page | [dates.ts:9](../../frontend/src/lib/dynamic-form/dates.ts#L9) |
| MHR-40 | What is the International Recognition Procedure submission date? | Actual date is after today (UTC) | `Actual date of submission must be today or in the past` | `Actual International Recognition Procedure submission date must be today or in the past` | Always | Page | [dates.ts:11](../../frontend/src/lib/dynamic-form/dates.ts#L11) |

#### International regulatory licence date (`international-licence-date`)

Page shown when: "Select the procedure you currently expect to use for UK authorisation" is **International Recognition Procedure**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| MHR-41 | What is the International Recognition Procedure regulatory licence date? | Required | `Select an option` | `Select an estimated quarter, estimated month or actual date for the International Recognition Procedure regulatory licence` | Always | Page, CYA | [MedicinesIntlRecognition.cs:70](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesIntlRecognition.cs#L70) |
| MHR-42 | What is the International Recognition Procedure regulatory licence date? | Estimated quarter: quarter and year both blank | `Enter the estimated licence date` | `Enter the estimated International Recognition Procedure regulatory licence date` | Always | Page | [dates.ts:17](../../frontend/src/lib/dynamic-form/dates.ts#L17) |
| MHR-43 | What is the International Recognition Procedure regulatory licence date? | Estimated quarter: quarter blank | `Select a quarter` | `Estimated International Recognition Procedure regulatory licence date must include a quarter` | Always | Page | [dates.ts:19](../../frontend/src/lib/dynamic-form/dates.ts#L19) |
| MHR-44 | What is the International Recognition Procedure regulatory licence date? | Estimated quarter: year blank | `Enter a year` | `Estimated International Recognition Procedure regulatory licence date must include a year` | Always | Page | [dates.ts:20](../../frontend/src/lib/dynamic-form/dates.ts#L20) |
| MHR-45 | What is the International Recognition Procedure regulatory licence date? | Estimated month: month or year (or both) blank | `Enter the estimated licence date` | `Both blank: Enter the estimated International Recognition Procedure regulatory licence date. One blank: Estimated International Recognition Procedure regulatory licence date must include a [month or year]` | Always | Page | [dates.ts:17](../../frontend/src/lib/dynamic-form/dates.ts#L17) |
| MHR-46 | What is the International Recognition Procedure regulatory licence date? | Estimated quarter or month: not a real date, or year not 4 digits from 1000 | `Estimated date of licence must be a real date` | `Not real: Estimated International Recognition Procedure regulatory licence date must be a real date. Year not 4 digits: Year must include 4 numbers` | Always | Page | [dates.ts:9](../../frontend/src/lib/dynamic-form/dates.ts#L9) |
| MHR-47 | What is the International Recognition Procedure regulatory licence date? | Estimated quarter or month has already ended | `Estimated date of licence must be in the future` | `Quarter: Estimated International Recognition Procedure regulatory licence date must be this quarter or in the future. Month: Estimated International Recognition Procedure regulatory licence date must be this month or in the future` | Always | Page, CYA | [dates.ts:13](../../frontend/src/lib/dynamic-form/dates.ts#L13) |
| MHR-48 | What is the International Recognition Procedure regulatory licence date? | Actual date: any part blank | `Enter the actual licence date` | `All blank: Enter the actual International Recognition Procedure regulatory licence date. Some blank: Actual International Recognition Procedure regulatory licence date must include a [missing parts]` | Always | Page | [dates.ts:17](../../frontend/src/lib/dynamic-form/dates.ts#L17) |
| MHR-49 | What is the International Recognition Procedure regulatory licence date? | Actual date: not a real date, or year not 4 digits from 1000 | `Actual date of licence must be a real date` | `Not real: Actual International Recognition Procedure regulatory licence date must be a real date. Year not 4 digits: Year must include 4 numbers` | Always | Page | [dates.ts:9](../../frontend/src/lib/dynamic-form/dates.ts#L9) |
| MHR-50 | What is the International Recognition Procedure regulatory licence date? | Actual date is after today (UTC) | `Actual date of licence must be today or in the past` | `Actual International Recognition Procedure regulatory licence date must be today or in the past` | Always | Page | [dates.ts:11](../../frontend/src/lib/dynamic-form/dates.ts#L11) |

#### International conditional approval (`international-conditional-approval`)

Page shown when: "Select the procedure you currently expect to use for UK authorisation" is **International Recognition Procedure**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| MHR-51 | Is international conditional approval anticipated for this product? | Required | `Select an option` | `Select whether international conditional approval is anticipated for this product` | Always | Page, CYA | [MedicinesIntlRecognition.cs:79](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesIntlRecognition.cs#L79) |

### Health Technology Assessment (HTA) and launch

#### HTA submission (`hta-submission`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| HTA-01 | Do you intend to submit for a HTA? | Required | `Select an option` | `Select whether you intend to submit for a HTA` | Always | Page, CYA | [RecordHta.cs:41](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordHta.cs#L41) |
| HTA-02 | Select the submission bodies you intend to submit to for a HTA | Required | `Select if you will submit to NICE, the SMC, or AWMSG for a HTA` | `Select the submission bodies you intend to submit to for a HTA` | "Do you intend to submit for a HTA?" is **Yes** | Page, CYA | [RecordHta.cs:57](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordHta.cs#L57) |
| HTA-03 | Add additional details or clarifications, such as any planned differences in the indications submitted to each HTA | Required | `Enter details` | `Enter additional details or clarifications` | "Do you intend to submit for a HTA?" is **Yes** | Page, CYA | [RecordHta.cs:68](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordHta.cs#L68) |

#### MHRA/NICE aligned pathway (`nice-aligned-pathway`)

Page shown when: "Select the submission bodies you intend to submit to for a HTA" is **NICE**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| HTA-04 | Do you intend to follow the MHRA/NICE aligned pathway? | Required | `Select an option` | `Select whether you intend to follow the MHRA/NICE aligned pathway` | Always | Page, CYA | [RecordHta.cs:79](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordHta.cs#L79) |

#### NICE technology appraisal ID (`nice-ta-id`)

Page shown when: "Select the submission bodies you intend to submit to for a HTA" is **NICE**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| HTA-05 | Enter the NICE technology appraisal development ID for this product (optional) | Number entered without choosing an ID type | `Select ID type` | `Select the ID type for the NICE technology appraisal development ID` | Always | Page | [prefixedNumber.ts:5](../../frontend/src/lib/dynamic-form/prefixedNumber.ts#L5) |
| HTA-06 | Enter the NICE technology appraisal development ID for this product (optional) | Number contains anything other than digits, or is all zeros | `ID must be a whole positive number` | `Not only numbers: NICE technology appraisal development ID must only include numbers after the ID type. All zeros: NICE technology appraisal development ID must not be all zeros` | Always | Page | [prefixedNumber.ts:6](../../frontend/src/lib/dynamic-form/prefixedNumber.ts#L6) |
| HTA-07 | Enter the NICE technology appraisal development ID for this product (optional) | Number is longer than 5 digits | `ID must not be longer than 5 digits` | `NICE technology appraisal development ID must be 5 numbers or less` | Always | Page | [prefixedNumber.ts:7](../../frontend/src/lib/dynamic-form/prefixedNumber.ts#L7) |

#### UK commercial launch date (`uk-launch-date`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| HTA-08 | What is the UK commercial launch date? | Required | `Select an option` | `Select an estimated quarter, estimated month or actual date for the UK commercial launch` | Always | Page, CYA | [RecordHta.cs:112](../../backend/src/Persistence/Entities/SharedRevisionContent/RecordHta.cs#L112) |
| HTA-09 | What is the UK commercial launch date? | Estimated quarter: quarter and year both blank | `Enter the estimated launch date` | `Enter the estimated UK commercial launch date` | Always | Page | [dates.ts:17](../../frontend/src/lib/dynamic-form/dates.ts#L17) |
| HTA-10 | What is the UK commercial launch date? | Estimated quarter: quarter blank | `Select a quarter` | `Estimated UK commercial launch date must include a quarter` | Always | Page | [dates.ts:19](../../frontend/src/lib/dynamic-form/dates.ts#L19) |
| HTA-11 | What is the UK commercial launch date? | Estimated quarter: year blank | `Enter a year` | `Estimated UK commercial launch date must include a year` | Always | Page | [dates.ts:20](../../frontend/src/lib/dynamic-form/dates.ts#L20) |
| HTA-12 | What is the UK commercial launch date? | Estimated month: month or year (or both) blank | `Enter the estimated launch date` | `Both blank: Enter the estimated UK commercial launch date. One blank: Estimated UK commercial launch date must include a [month or year]` | Always | Page | [dates.ts:17](../../frontend/src/lib/dynamic-form/dates.ts#L17) |
| HTA-13 | What is the UK commercial launch date? | Estimated quarter or month: not a real date, or year not 4 digits from 1000 | `Estimated date of launch must be a real date` | `Not real: Estimated UK commercial launch date must be a real date. Year not 4 digits: Year must include 4 numbers` | Always | Page | [dates.ts:9](../../frontend/src/lib/dynamic-form/dates.ts#L9) |
| HTA-14 | What is the UK commercial launch date? | Estimated quarter or month has already ended | `Estimated date of launch must be in the future` | `Quarter: Estimated UK commercial launch date must be this quarter or in the future. Month: Estimated UK commercial launch date must be this month or in the future` | Always | Page, CYA | [dates.ts:13](../../frontend/src/lib/dynamic-form/dates.ts#L13) |
| HTA-15 | What is the UK commercial launch date? | Actual date: any part blank | `Enter the actual launch date` | `All blank: Enter the actual UK commercial launch date. Some blank: Actual UK commercial launch date must include a [missing parts]` | Always | Page | [dates.ts:17](../../frontend/src/lib/dynamic-form/dates.ts#L17) |
| HTA-16 | What is the UK commercial launch date? | Actual date: not a real date, or year not 4 digits from 1000 | `Actual date of launch must be a real date` | `Not real: Actual UK commercial launch date must be a real date. Year not 4 digits: Year must include 4 numbers` | Always | Page | [dates.ts:9](../../frontend/src/lib/dynamic-form/dates.ts#L9) |
| HTA-17 | What is the UK commercial launch date? | Actual date is after today (UTC) | `Actual date of launch must be today or in the past` | `Actual UK commercial launch date must be today or in the past` | Always | Page | [dates.ts:11](../../frontend/src/lib/dynamic-form/dates.ts#L11) |

## 6. Provide special designation information

### Orphan medicine status and Advanced Therapy Medicinal Product (ATMP) classification

#### Orphan medicine status (`orphan-status`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| ORP-01 | Have you or will you apply for orphan medicine status in the EU? | Required | `Select an option` | `Select whether you have applied or will apply for orphan medicine status in the EU` | Always | Page, CYA | [MedicinesEuStatus.cs:25](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesEuStatus.cs#L25) |
| ORP-02 | Date EU orphan medicine status granted | Required | `Enter the date that EU orphan medicine status was granted` | `Enter the date EU orphan medicine status was granted` | "Have you or will you apply for orphan medicine status in the EU?" is **Yes, orphan status granted** | Page, CYA | [MedicinesEuStatus.cs:52](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesEuStatus.cs#L52) |
| ORP-03 | Date EU orphan medicine status granted | Not a real date, or only some parts filled in | `Enter a real date` | `Some blank: Date EU orphan medicine status granted must include a [missing parts]. Not real: Date EU orphan medicine status granted must be a real date. Year not 4 digits: Year must include 4 numbers` | "Have you or will you apply for orphan medicine status in the EU?" is **Yes, orphan status granted** | Page | [dates.ts:7](../../frontend/src/lib/dynamic-form/dates.ts#L7) |
| ORP-04 | Date EU orphan medicine status granted | Date is after today (UTC) | `Actual date of EU orphan medicine status must be today or in the past` | `Date EU orphan medicine status granted must be today or in the past` | "Have you or will you apply for orphan medicine status in the EU?" is **Yes, orphan status granted** | Page | [dates.ts:11](../../frontend/src/lib/dynamic-form/dates.ts#L11) |
| ORP-05 | EU orphan status number | Required | `Enter the EU orphan status number` | _No change_ | "Have you or will you apply for orphan medicine status in the EU?" is **Yes, orphan status granted** | Page, CYA | [MedicinesEuStatus.cs:38](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesEuStatus.cs#L38) |

#### ATMP classification (`atmp-classification`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| ORP-06 | Is this product classified as an ATMP in the EU? | Required | `Select an option` | `Select whether this product is classified as an ATMP in the EU` | Always | Page, CYA | [MedicinesEuStatus.cs:65](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesEuStatus.cs#L65) |
| ORP-07 | Enter the date when the ATMP classification recommendation was provided | Required | `Enter the date the ATMP classification recommendation was provided` | `Enter the date when the ATMP classification recommendation was provided` | "Is this product classified as an ATMP in the EU?" is **Yes** | Page, CYA | [MedicinesEuStatus.cs:84](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesEuStatus.cs#L84) |
| ORP-08 | Enter the date when the ATMP classification recommendation was provided | Not a real date, or only some parts filled in | `Enter a real date` | `Some blank: Date of ATMP classification recommendation must include a [missing parts]. Not real: Date of ATMP classification recommendation must be a real date. Year not 4 digits: Year must include 4 numbers` | "Is this product classified as an ATMP in the EU?" is **Yes** | Page | [dates.ts:7](../../frontend/src/lib/dynamic-form/dates.ts#L7) |
| ORP-09 | Enter the date when the ATMP classification recommendation was provided | Date is after today (UTC) | `Actual date of ATMP classification recommendation must be today or in the past` | `Date of ATMP classification recommendation must be today or in the past` | "Is this product classified as an ATMP in the EU?" is **Yes** | Page | [dates.ts:11](../../frontend/src/lib/dynamic-form/dates.ts#L11) |
| ORP-10 | Select the category that best reflects the classification outcome | Required | `Select an option` | `Select the category that best reflects the classification outcome` | "Is this product classified as an ATMP in the EU?" is **Yes** | Page, CYA | [MedicinesEuStatus.cs:96](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesEuStatus.cs#L96) |

### Promising Innovative Medicine (PIM) and Early Access to Medicines Scheme (EAMS)

#### PIM designation (`pim-designation`)

Page shown when: "What is the technology status for this product?" does not include **Biosimilar**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| PIM-01 | Do you have, or are you seeking, Medicines and Healthcare Regulatory Agency (MHRA) PIM designation? | Required | `Select an option` | `Select whether you have, or are seeking, MHRA PIM designation` | Always | Page, CYA | [MedicinesEamsPim.cs:49](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesEamsPim.cs#L49) |
| PIM-02 | Have you already or do you plan to submit to EAMS? | Required | `Select an option` | `Select whether you have already submitted or plan to submit to EAMS` | "Do you have, or are you seeking, Medicines and Healthcare Regulatory Agency (MHRA) PIM designation?" is **Granted** | Page, CYA | [MedicinesEamsPim.cs:62](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesEamsPim.cs#L62) |

#### EAMS submission date (`eams-submission-date`)

Page shown when: "Have you already or do you plan to submit to EAMS?" is **Yes**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| PIM-03 | What is the EAMS submission date? | Required | `Select an option` | `Select an estimated quarter, estimated month or actual date for the EAMS submission` | Always | Page, CYA | [MedicinesEamsPim.cs:75](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesEamsPim.cs#L75) |
| PIM-04 | What is the EAMS submission date? | Estimated quarter: quarter and year both blank | `Enter the estimated EAMS submission date` | _No change_ | Always | Page | [dates.ts:17](../../frontend/src/lib/dynamic-form/dates.ts#L17) |
| PIM-05 | What is the EAMS submission date? | Estimated quarter: quarter blank | `Select a quarter` | `Estimated EAMS submission date must include a quarter` | Always | Page | [dates.ts:19](../../frontend/src/lib/dynamic-form/dates.ts#L19) |
| PIM-06 | What is the EAMS submission date? | Estimated quarter: year blank | `Enter a year` | `Estimated EAMS submission date must include a year` | Always | Page | [dates.ts:20](../../frontend/src/lib/dynamic-form/dates.ts#L20) |
| PIM-07 | What is the EAMS submission date? | Estimated month: month or year (or both) blank | `Enter the estimated EAMS submission date` | `Both blank: Enter the estimated EAMS submission date. One blank: Estimated EAMS submission date must include a [month or year]` | Always | Page | [dates.ts:17](../../frontend/src/lib/dynamic-form/dates.ts#L17) |
| PIM-08 | What is the EAMS submission date? | Estimated quarter or month: not a real date, or year not 4 digits from 1000 | `Estimated date of EAMS submission must be a real date` | `Not real: Estimated EAMS submission date must be a real date. Year not 4 digits: Year must include 4 numbers` | Always | Page | [dates.ts:9](../../frontend/src/lib/dynamic-form/dates.ts#L9) |
| PIM-09 | What is the EAMS submission date? | Estimated quarter or month has already ended | `Estimated date of EAMS submission must be in the future` | `Quarter: Estimated EAMS submission date must be this quarter or in the future. Month: Estimated EAMS submission date must be this month or in the future` | Always | Page, CYA | [dates.ts:13](../../frontend/src/lib/dynamic-form/dates.ts#L13) |
| PIM-10 | What is the EAMS submission date? | Actual date: any part blank | `Enter the actual EAMS submission date` | `All blank: Enter the actual EAMS submission date. Some blank: Actual EAMS submission date must include a [missing parts]` | Always | Page | [dates.ts:17](../../frontend/src/lib/dynamic-form/dates.ts#L17) |
| PIM-11 | What is the EAMS submission date? | Actual date: not a real date, or year not 4 digits from 1000 | `Actual date of EAMS submission must be a real date` | `Not real: Actual EAMS submission date must be a real date. Year not 4 digits: Year must include 4 numbers` | Always | Page | [dates.ts:9](../../frontend/src/lib/dynamic-form/dates.ts#L9) |
| PIM-12 | What is the EAMS submission date? | Actual date is after today (UTC) | `Actual date of EAMS submission must be today or in the past` | `Actual EAMS submission date must be today or in the past` | Always | Page | [dates.ts:11](../../frontend/src/lib/dynamic-form/dates.ts#L11) |

#### EAMS scientific opinion date (`eams-opinion-date`)

Page shown when: "Have you already or do you plan to submit to EAMS?" is **Yes**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| PIM-13 | Date of EAMS scientific opinion | Required | `Select an option` | `Select an estimated quarter, estimated month or actual date for the EAMS scientific opinion` | Always | Page, CYA | [MedicinesEamsPim.cs:87](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesEamsPim.cs#L87) |
| PIM-14 | Date of EAMS scientific opinion | Estimated quarter: quarter and year both blank | `Enter the estimated EAMS scientific opinion date` | _No change_ | Always | Page | [dates.ts:17](../../frontend/src/lib/dynamic-form/dates.ts#L17) |
| PIM-15 | Date of EAMS scientific opinion | Estimated quarter: quarter blank | `Select a quarter` | `Estimated EAMS scientific opinion date must include a quarter` | Always | Page | [dates.ts:19](../../frontend/src/lib/dynamic-form/dates.ts#L19) |
| PIM-16 | Date of EAMS scientific opinion | Estimated quarter: year blank | `Enter a year` | `Estimated EAMS scientific opinion date must include a year` | Always | Page | [dates.ts:20](../../frontend/src/lib/dynamic-form/dates.ts#L20) |
| PIM-17 | Date of EAMS scientific opinion | Estimated month: month or year (or both) blank | `Enter the estimated EAMS scientific opinion date` | `Both blank: Enter the estimated EAMS scientific opinion date. One blank: Estimated EAMS scientific opinion date must include a [month or year]` | Always | Page | [dates.ts:17](../../frontend/src/lib/dynamic-form/dates.ts#L17) |
| PIM-18 | Date of EAMS scientific opinion | Estimated quarter or month: not a real date, or year not 4 digits from 1000 | `Estimated date of EAMS scientific opinion must be a real date` | `Not real: Estimated EAMS scientific opinion date must be a real date. Year not 4 digits: Year must include 4 numbers` | Always | Page | [dates.ts:9](../../frontend/src/lib/dynamic-form/dates.ts#L9) |
| PIM-19 | Date of EAMS scientific opinion | Estimated quarter or month has already ended | `Estimated date of EAMS scientific opinion must be in the future` | `Quarter: Estimated EAMS scientific opinion date must be this quarter or in the future. Month: Estimated EAMS scientific opinion date must be this month or in the future` | Always | Page, CYA | [dates.ts:13](../../frontend/src/lib/dynamic-form/dates.ts#L13) |
| PIM-20 | Date of EAMS scientific opinion | Actual date: any part blank | `Enter the actual EAMS scientific opinion date` | `All blank: Enter the actual EAMS scientific opinion date. Some blank: Actual EAMS scientific opinion date must include a [missing parts]` | Always | Page | [dates.ts:17](../../frontend/src/lib/dynamic-form/dates.ts#L17) |
| PIM-21 | Date of EAMS scientific opinion | Actual date: not a real date, or year not 4 digits from 1000 | `Actual date of EAMS scientific opinion must be a real date` | `Not real: Actual EAMS scientific opinion date must be a real date. Year not 4 digits: Year must include 4 numbers` | Always | Page | [dates.ts:9](../../frontend/src/lib/dynamic-form/dates.ts#L9) |
| PIM-22 | Date of EAMS scientific opinion | Actual date is after today (UTC) | `Actual date of EAMS scientific opinion must be today or in the past` | `Actual EAMS scientific opinion date must be today or in the past` | Always | Page | [dates.ts:11](../../frontend/src/lib/dynamic-form/dates.ts#L11) |

#### EAMS scientific opinion decision (`eams-opinion-decision`)

Page shown when: "Date of EAMS scientific opinion" is **an actual date**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| PIM-23 | What was the EAMS scientific opinion decision? | Required | `Select an option` | `Select if the EAMS scientific opinion decision was positive or negative` | Always | Page, CYA | [MedicinesEamsPim.cs:97](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesEamsPim.cs#L97) |

## 7. Provide service readiness information (optional)

> Nothing here is required to submit. The "Required" rules run only when a page is saved, so these messages never reach Check your answers (an answer that was given is still checked there).

### Laboratory testing

#### Diagnostic test (`diagnostic-test-required`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| LAB-01 | Does the product require a specific biomarker or diagnostic test result before prescribing? | Required | `Select an option` | `Select whether the product requires a specific biomarker or diagnostic test result before prescribing` | Always | Page | [MedicinesLaboratoryTesting.cs:83](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesLaboratoryTesting.cs#L83) |

#### Biomarker type (`biomarker-type`)

Page shown when: "Does the product require a specific biomarker or diagnostic test result before prescribing?" is **Yes**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| LAB-02 | What type of biomarker is required for this test? | Required | `Select an option` | `Select the type of biomarker required for this test` | Always | Page | [MedicinesLaboratoryTesting.cs:97](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesLaboratoryTesting.cs#L97) |

#### Non-genomic biomarker (`non-genomic-biomarker`)

Page shown when: "What type of biomarker is required for this test?" is **Non-genomic biomarker**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| LAB-03 | Describe the non-genomic biomarker | Required | `Enter details of the non-genomic biomarker` | `Enter a description of the non-genomic biomarker` | Always | Page | [MedicinesLaboratoryTesting.cs:111](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesLaboratoryTesting.cs#L111) |

#### Genomic target (`genomic-target`)

Page shown when: "What type of biomarker is required for this test?" is **Genomic biomarker**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| LAB-04 | What is the genomic target for this test? | Required | `Enter the genomic target for this test` | _No change_ | Always | Page | [MedicinesLaboratoryTesting.cs:125](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesLaboratoryTesting.cs#L125) |

#### NGTD (`ngtd-relationship`)

Page shown when: "What type of biomarker is required for this test?" is **Genomic biomarker**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| LAB-05 | How does this test relate to what's currently in the National Genomic Test Directory (NGTD)? | Required | `Select an option` | `Select how this test relates to what's currently in the NGTD` | Always | Page | [MedicinesLaboratoryTesting.cs:139](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesLaboratoryTesting.cs#L139) |

#### Sample type (`sample-type`)

Page shown when: "What type of biomarker is required for this test?" is **Genomic biomarker**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| LAB-06 | What sample type is needed? | Required | `Enter details of the sample type required for the test` | `Enter the sample type needed` | Always | Page | [MedicinesLaboratoryTesting.cs:150](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesLaboratoryTesting.cs#L150) |

#### Turnaround time (`turnaround-time`)

Page shown when: "What type of biomarker is required for this test?" is **Genomic biomarker**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| LAB-07 | Describe any turnaround time requirements for this test that differ from the current routinely commissioned testing pathway (optional) | Optional | _No validation_ | - | Always | - | [MedicinesLaboratoryTesting.cs:158](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesLaboratoryTesting.cs#L158) |

#### Patient pathway (`pathway-point`)

Page shown when: "What type of biomarker is required for this test?" is **Genomic biomarker**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| LAB-08 | At which point of the patient pathway should the test be conducted? | Required | `Select an option` | `Select the point of the patient pathway when the test should be conducted` | Always | Page | [MedicinesLaboratoryTesting.cs:174](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesLaboratoryTesting.cs#L174) |
| LAB-09 | Describe when in the care pathway the test is expected to take place | Required | `Enter details` | `Enter when in the care pathway the test is expected to take place` | "At which point of the patient pathway should the test be conducted?" is **Other** | Page | [MedicinesLaboratoryTesting.cs:192](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesLaboratoryTesting.cs#L192) |

#### Genomic alterations (`genomic-alterations`)

Page shown when: "What type of biomarker is required for this test?" is **Genomic biomarker**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| LAB-10 | What genomic alterations determine patient eligibility? | Required | `Enter the genomic alterations that determine patient eligibility` | _No change_ | Always | Page | [MedicinesLaboratoryTesting.cs:203](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesLaboratoryTesting.cs#L203) |

#### Additional genomic factors (`additional-genomic-factors`)

Page shown when: "What type of biomarker is required for this test?" is **Genomic biomarker**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| LAB-11 | Are there additional genomic factors that affect treatment selection or sequencing? | Required | `Enter details of genomic factors that affect treatment selection or sequencing` | `Enter any additional genomic factors that affect treatment selection or sequencing` | Always | Page | [MedicinesLaboratoryTesting.cs:218](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesLaboratoryTesting.cs#L218) |

#### Test used in trials (`test-used-in-trials`)

Page shown when: "What type of biomarker is required for this test?" is **Genomic biomarker**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| LAB-12 | What test was used in trials? | Required | `Enter details of the test used in trials` | `Enter the test used in trials` | Always | Page | [MedicinesLaboratoryTesting.cs:228](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesLaboratoryTesting.cs#L228) |

#### Specificity and sensitivity (`specificity-sensitivity`)

Page shown when: "What type of biomarker is required for this test?" is **Genomic biomarker**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| LAB-13 | Enter the minimum specificity and sensitivity used | Required | `Enter details of the minimum specificity and sensitivity used in testing` | `Enter the minimum specificity and sensitivity used` | Always | Page | [MedicinesLaboratoryTesting.cs:238](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesLaboratoryTesting.cs#L238) |

#### Mandatory testing (`test-mandatory`)

Page shown when: "Does the product require a specific biomarker or diagnostic test result before prescribing?" is **Yes**.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| LAB-14 | Is this test mandatory before treatment can start? | Required | `Select an option` | `Select whether this test is mandatory before treatment can start` | Always | Page | [MedicinesLaboratoryTesting.cs:249](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesLaboratoryTesting.cs#L249) |
| LAB-15 | Enter any notes about uncertainties, assumptions, or service considerations relating to the tests | Required | `Enter details` | `Enter any notes about uncertainties, assumptions, or service considerations relating to the tests` | "Is this test mandatory before treatment can start?" is **Mandatory, no alternatives** or **Mandatory, alternatives may exist** | Page | [MedicinesLaboratoryTesting.cs:269](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesLaboratoryTesting.cs#L269) |

#### Monitoring tests (`monitoring-tests`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| LAB-16 | Describe any tests (genomic tests or otherwise) needed to monitor response to treatment beyond what is currently offered in NHS practice | Required | `Enter details of tests needed to monitor a response to treatment beyond what is offered by the NHS` | `Enter any tests needed to monitor response to treatment` | Always | Page | [MedicinesLaboratoryTesting.cs:283](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesLaboratoryTesting.cs#L283) |

#### Safety tests (`safety-tests`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| LAB-17 | Describe any tests needed to assess safety of the treatment that are additional to what is currently used in routine clinical practice | Required | `Enter details` | `Enter any tests needed to assess safety of the treatment` | Always | Page | [MedicinesLaboratoryTesting.cs:299](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesLaboratoryTesting.cs#L299) |

### Patient and clinical requirements

#### Screening or surveillance (`screening-programme`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| PAT-01 | Is a screening or surveillance programme required to identify eligible patients? | Required | `Select an option` | `Select whether a screening or surveillance programme is required to identify eligible patients` | Always | Page | [MedicinesPatientIdentification.cs:23](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesPatientIdentification.cs#L23) |
| PAT-02 | Briefly describe the type of screening or surveillance needed | Required | `Enter details` | `Enter the type of screening or surveillance needed` | "Is a screening or surveillance programme required to identify eligible patients?" is **Yes** | Page | [MedicinesPatientIdentification.cs:37](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesPatientIdentification.cs#L37) |

#### Urgent patient identification (`urgent-identification`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| PAT-03 | Do patients need to be identified for treatment more quickly than standard practice? | Required | `Select an option` | `Select whether patients need to be identified for treatment more quickly than standard practice` | Always | Page | [MedicinesPatientIdentification.cs:48](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesPatientIdentification.cs#L48) |
| PAT-04 | Provide further details | Required | `Enter details` | `Enter further details about why patients need to be identified more quickly` | "Do patients need to be identified for treatment more quickly than standard practice?" is **Yes** | Page | [MedicinesPatientIdentification.cs:65](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesPatientIdentification.cs#L65) |

#### Proposed place in therapy (`place-in-therapy`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| PAT-05 | Enter the proposed place in therapy and likely comparators | Required | `Enter the proposed place in therapy for your product` | `Enter the proposed place in therapy and likely comparators` | Always | Page | [MedicinesTreatmentDetail.cs:22](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesTreatmentDetail.cs#L22) |

#### Estimated duration of treatment (`treatment-duration`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| PAT-06 | Enter the estimated duration of treatment | Required | `Enter the estimated duration of treatment for your product` | `Enter the estimated duration of treatment` | Always | Page | [MedicinesTreatmentDetail.cs:33](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesTreatmentDetail.cs#L33) |

#### NHS service changes (`nhs-service-changes`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| PAT-07 | Are any changes to NHS services required for the delivery of this product? | Required | `Select an option` | `Select whether any changes to NHS services are required for the delivery of this product` | Always | Page | [MedicinesServiceImpact.cs:25](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesServiceImpact.cs#L25) |
| PAT-08 | Describe any changes to staffing, equipment, pathway, or commissioning needed for delivery of this medicine. | Required | `Enter details` | `Enter any changes to staffing, equipment, pathway, or commissioning needed` | "Are any changes to NHS services required for the delivery of this product?" is **Complete transformation** or **Some change** | Page | [MedicinesServiceImpact.cs:44](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesServiceImpact.cs#L44) |

#### Handling and storage (`handling-storage`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| PAT-09 | Are there specific requirements for the handling or storage of this product that may cause service issues? | Required | `Select an option` | `Select whether there are specific requirements for the handling or storage of this product` | Always | Page | [MedicinesServiceImpact.cs:59](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesServiceImpact.cs#L59) |
| PAT-10 | Provide further details | Required | `Enter details` | `Enter further details about the handling or storage requirements` | "Are there specific requirements for the handling or storage of this product that may cause service issues?" is **Yes** | Page | [MedicinesServiceImpact.cs:72](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesServiceImpact.cs#L72) |

#### UK patient population (`population-range`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| PAT-11 | UK patient population range for this condition | Required | `Select a population size` | `Select the UK patient population range for this condition` | Always | Page | [MedicinesServiceImpact.cs:84](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesServiceImpact.cs#L84) |
| PAT-12 | Additional details about the UK patient population | Required | `Enter details` | `Enter additional details about the UK patient population` | Always | Page | [MedicinesServiceImpact.cs:95](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesServiceImpact.cs#L95) |

#### Enter the estimated UK eligible population range (`eligible-population`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| PAT-13 | Enter the estimated UK eligible population range | Required | `Enter details` | `Enter the estimated UK eligible population range` | Always | Page | [MedicinesServiceImpact.cs:108](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesServiceImpact.cs#L108) |

### Pricing and budget impact

#### Estimated uptake (`estimated-uptake`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| PRI-01 | Enter the estimated uptake based on expected adoption patterns (optional) | Optional | _No validation_ | - | Always | - | [MedicinesBudgetImpact.cs:18](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesBudgetImpact.cs#L18) |

#### Compassionate or early access (`compassionate-access`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| PRI-02 | Will UK patients be able to access this product on a compassionate or early access basis, outside of clinical trials? | Required | `Select an option` | `Select whether UK patients will be able to access this product on a compassionate or early access basis` | Always | Page | [MedicinesBudgetImpact.cs:37](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesBudgetImpact.cs#L37) |
| PRI-03 | Provide further details | Required | `Enter details` | `Enter further details about compassionate or early access` | "Will UK patients be able to access this product on a compassionate or early access basis, outside of clinical trials?" is **Yes** | Page | [MedicinesBudgetImpact.cs:51](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesBudgetImpact.cs#L51) |

#### Patient Access Scheme (`patient-access-scheme`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| PRI-04 | Is a Patient Access Scheme or alternative discount arrangement planned for this indication? | Required | `Select an option` | `Select whether a Patient Access Scheme or alternative discount arrangement is planned` | Always | Page | [MedicinesBudgetImpact.cs:66](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesBudgetImpact.cs#L66) |
| PRI-05 | Select all regions where a PAS or alternative discount arrangement is planned | Required | `Select all planned locations of discount arrangements` | `Select all regions where a PAS or alternative discount arrangement is planned` | "Is a Patient Access Scheme or alternative discount arrangement planned for this indication?" is **Yes** | Page | [MedicinesBudgetImpact.cs:79](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesBudgetImpact.cs#L79) |

#### Indication specific pricing (`indication-specific-pricing`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| PRI-06 | Are there plans for indication specific pricing? | Required | `Select an option` | `Select whether there are plans for indication specific pricing` | Always | Page | [MedicinesBudgetImpact.cs:91](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesBudgetImpact.cs#L91) |
| PRI-07 | Provide further details | Required | `Enter details` | `Enter further details about indication specific pricing` | "Are there plans for indication specific pricing?" is **Yes** | Page | [MedicinesBudgetImpact.cs:105](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesBudgetImpact.cs#L105) |

#### Net budget impact (`net-budget-impact`)

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| PRI-08 | What is the estimated net budget impact for the UK over the first 3 years of NHS use? | Required | `Select an option` | `Select the estimated net budget impact for the UK over the first 3 years of NHS use` | Always | Page | [MedicinesBudgetImpact.cs:118](../../backend/src/Persistence/Entities/MedicinesRevisionContent/MedicinesBudgetImpact.cs#L118) |

## 8. Cross-page date rules

Checked only on Check your answers and on Submit; they never block saving a page. Estimated dates are compared as the whole month or quarter. A rule is skipped if either date is unanswered, not applicable, or already has a page error.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| XPR-01 | What is the UK regulatory licence date? | CR-01: on or before the other date | `UK licence date must be on or before the UK commercial launch date` | `UK regulatory licence date must be the same as or before the UK commercial launch date` | Both dates answered and "Have you already or do you plan to submit to EAMS?" is not **Yes** | CYA | [MedicineCrossPageRules.cs:25](../../backend/src/Application/RecordForms/CrossPageRules/MedicineCrossPageRules.cs#L25) |
| XPR-02 | What is the UK commercial launch date? | CR-01: on or after the other date | `UK commercial launch date must be on or after the UK licence date` | `UK commercial launch date must be the same as or after the UK regulatory licence date` | Both dates answered and "Have you already or do you plan to submit to EAMS?" is not **Yes** | CYA | [MedicineCrossPageRules.cs:25](../../backend/src/Application/RecordForms/CrossPageRules/MedicineCrossPageRules.cs#L25) |
| XPR-03 | What is the UK regulatory submission date? | CR-02: before the other date | `UK regulatory submission date must be before the UK licence date` | `UK regulatory submission date must be before the UK regulatory licence date` | Both dates answered | CYA | [MedicineCrossPageRules.cs:34](../../backend/src/Application/RecordForms/CrossPageRules/MedicineCrossPageRules.cs#L34) |
| XPR-04 | What is the UK regulatory licence date? | CR-02: after the other date | `UK licence date must be after the UK regulatory submission date` | `UK regulatory licence date must be after the UK regulatory submission date` | Both dates answered | CYA | [MedicineCrossPageRules.cs:34](../../backend/src/Application/RecordForms/CrossPageRules/MedicineCrossPageRules.cs#L34) |
| XPR-05 | What is the UK regulatory submission date? | CR-03: before the other date | `UK regulatory submission date must be before the UK commercial launch date` | _No change_ | Both dates answered | CYA | [MedicineCrossPageRules.cs:42](../../backend/src/Application/RecordForms/CrossPageRules/MedicineCrossPageRules.cs#L42) |
| XPR-06 | What is the UK commercial launch date? | CR-03: after the other date | `UK commercial launch date must be after the UK regulatory submission date` | _No change_ | Both dates answered | CYA | [MedicineCrossPageRules.cs:42](../../backend/src/Application/RecordForms/CrossPageRules/MedicineCrossPageRules.cs#L42) |
| XPR-07 | What is the International Recognition Procedure submission date? | CR-04: before the other date | `International regulatory submission date must be before the international licence date` | `International Recognition Procedure submission date must be before the International Recognition Procedure regulatory licence date` | Both dates answered | CYA | [MedicineCrossPageRules.cs:50](../../backend/src/Application/RecordForms/CrossPageRules/MedicineCrossPageRules.cs#L50) |
| XPR-08 | What is the International Recognition Procedure regulatory licence date? | CR-04: after the other date | `International licence date must be after the international regulatory submission date` | `International Recognition Procedure regulatory licence date must be after the International Recognition Procedure submission date` | Both dates answered | CYA | [MedicineCrossPageRules.cs:50](../../backend/src/Application/RecordForms/CrossPageRules/MedicineCrossPageRules.cs#L50) |
| XPR-09 | Enter the date of GFS of this product | CR-05: on or before the other date | `Global first submission date must be on or before the UK regulatory submission date` | `Date of GFS must be the same as or before the UK regulatory submission date` | Both dates answered | CYA | [MedicineCrossPageRules.cs:58](../../backend/src/Application/RecordForms/CrossPageRules/MedicineCrossPageRules.cs#L58) |
| XPR-10 | What is the UK regulatory submission date? | CR-05: on or after the other date | `UK regulatory submission date must be on or after the global first submission date` | `UK regulatory submission date must be the same as or after the date of GFS` | Both dates answered | CYA | [MedicineCrossPageRules.cs:58](../../backend/src/Application/RecordForms/CrossPageRules/MedicineCrossPageRules.cs#L58) |
| XPR-11 | Enter the date of GFS of this product | CR-06: on or before the other date | `Global first submission date must be on or before the international regulatory submission date` | `Date of GFS must be the same as or before the International Recognition Procedure submission date` | Both dates answered | CYA | [MedicineCrossPageRules.cs:66](../../backend/src/Application/RecordForms/CrossPageRules/MedicineCrossPageRules.cs#L66) |
| XPR-12 | What is the International Recognition Procedure submission date? | CR-06: on or after the other date | `International regulatory submission date must be on or after the global first submission date` | `International Recognition Procedure submission date must be the same as or after the date of GFS` | Both dates answered | CYA | [MedicineCrossPageRules.cs:66](../../backend/src/Application/RecordForms/CrossPageRules/MedicineCrossPageRules.cs#L66) |
| XPR-13 | What is the EAMS submission date? | CR-07: before the other date | `EAMS submission date must be before the EAMS scientific opinion date` | _No change_ | Both dates answered | CYA | [MedicineCrossPageRules.cs:74](../../backend/src/Application/RecordForms/CrossPageRules/MedicineCrossPageRules.cs#L74) |
| XPR-14 | Date of EAMS scientific opinion | CR-07: after the other date | `EAMS scientific opinion date must be after the EAMS submission date` | _No change_ | Both dates answered | CYA | [MedicineCrossPageRules.cs:74](../../backend/src/Application/RecordForms/CrossPageRules/MedicineCrossPageRules.cs#L74) |

## 9. Saving a page

Messages about the save itself, not one answer. Conflicts (SAV-02 to SAV-05) are followed by "Reload this page to see the latest answers."

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| SAV-01 | Any page | Save failed: no permission, not found, server or network error | `There was a problem saving your answers. Please try again later.` | `Page, not error message: Sorry, there is a problem with the service. Your answers have not been saved. Try again later.` | - | Top of page | [FormPage.tsx:163](../../frontend/src/components/DynamicForm/FormPage.tsx#L163) |
| SAV-02 | Any page | Record submitted, on hold or archived | `This record can no longer be edited because it has been submitted.` | `Page, not error message: You cannot edit this record because it has been submitted, or is on hold or archived` | - | Top of page | [RecordFormController.cs:119](../../backend/src/WebApi/Controllers/RecordFormController.cs#L119) |
| SAV-03 | Any page | Form version changed | `This form has changed since the page was loaded.` | `This form has changed since you opened this page` | - | Top of page | [RecordFormController.cs:124](../../backend/src/WebApi/Controllers/RecordFormController.cs#L124) |
| SAV-04 | Any page | Another save landed at the same moment | `This record was modified concurrently.` | `Someone else saved changes to this record at the same time. Your answers have not been saved` | - | Top of page | [RecordFormController.cs:127](../../backend/src/WebApi/Controllers/RecordFormController.cs#L127) |
| SAV-05 | Any page | Someone else changed an answer on this page | `This page has changed since it was loaded.` | `Someone else has changed the answers on this page since you opened it` | - | Top of page | [RecordFormController.cs:131](../../backend/src/WebApi/Controllers/RecordFormController.cs#L131) |
| SAV-06 | Any choice question | The option was archived between loading and saving the page | `Select a valid option` | `The answer you selected for [question] is no longer available. Select another answer` | - | Page | [RecordFormMessages.cs:11](../../backend/src/Application/RecordForms/Rules/RecordFormMessages.cs#L11) |

## 10. Check your answers and submit

The error summary lists one message per question, using the messages marked "CYA" above.

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| CYA-01 | Check your answers | At least one answer fails a rule | `There is a problem` | _No change_ | - | Error summary | [page.tsx:87](../../frontend/src/app/portal/records/%5BrecordId%5D/check-your-answers/page.tsx#L87) |
| CYA-02 | Check your answers | Text under the summary heading | `Change the information before you submit the record for quality assurance (QA) review:` | _No change_ | - | Error summary | [page.tsx:89](../../frontend/src/app/portal/records/%5BrecordId%5D/check-your-answers/page.tsx#L89) |
| CYA-03 | Each row or group with an error | The question has an error | `There is a problem` | _No change_ | - | Tag | [page.tsx:161](../../frontend/src/app/portal/records/%5BrecordId%5D/check-your-answers/page.tsx#L161) |
| CYA-04 | Check your answers | Heading shown with CYA-05 or CYA-06 | `This record can't be submitted` | `Page, not error message: You cannot submit this record` | - | Whole page | [page.tsx:61](../../frontend/src/app/portal/records/%5BrecordId%5D/check-your-answers/page.tsx#L61) |
| CYA-05 | Check your answers | User cannot submit the record | `You are not authorised to submit this record.` | `Page, not error message: You do not have permission to submit this record` | - | Whole page | [RecordSubmissionController.cs:139](../../backend/src/WebApi/Controllers/RecordSubmissionController.cs#L139) |
| CYA-06 | Check your answers | Record is not a submittable draft | `This record can't be submitted because it has already been submitted, or is on hold or archived.` | `Page, not error message: You cannot submit this record because it has already been submitted, or is on hold or archived` | - | Whole page | [RecordSubmissionController.cs:145](../../backend/src/WebApi/Controllers/RecordSubmissionController.cs#L145) |
| CYA-07 | Submit | Form version changed | `This form has changed since check your answers was loaded.` | `This form has changed since you opened check your answers` | - | Above Submit | [RecordSubmissionController.cs:122](../../backend/src/WebApi/Controllers/RecordSubmissionController.cs#L122) |
| CYA-08 | Submit | Any answer changed since the page loaded | `This record has changed since check your answers was loaded.` | `This record has changed since you opened check your answers` | - | Above Submit | [RecordSubmissionController.cs:127](../../backend/src/WebApi/Controllers/RecordSubmissionController.cs#L127) |
| CYA-09 | Submit | Status changed since the page loaded | `This record can't be submitted because it has already been submitted, or is on hold or archived.` | `Page, not error message: You cannot submit this record because it has already been submitted, or is on hold or archived` | - | Above Submit | [RecordSubmissionController.cs:145](../../backend/src/WebApi/Controllers/RecordSubmissionController.cs#L145) |
| CYA-10 | Submit | New errors found on submit, e.g. a date has passed | `There are problems with your answers. Fix the errors listed at the top of this page, then submit again.` | `There is a problem with your answers. Fix the errors at the top of this page, then submit again.` | - | Above Submit | [CheckAnswersActions.tsx:94](../../frontend/src/app/portal/records/%5BrecordId%5D/check-your-answers/_components/CheckAnswersActions.tsx#L94) |
| CYA-11 | Submit | Any other failure, including no permission | `There was a problem submitting your record. Please try again later.` | `Page, not error message: Sorry, there is a problem with the service. Your record has not been submitted. Try again later.` | - | Above Submit | [CheckAnswersActions.tsx:99](../../frontend/src/app/portal/records/%5BrecordId%5D/check-your-answers/_components/CheckAnswersActions.tsx#L99) |

CYA-07 to CYA-09 are followed by "Reload this page to see the latest answers."

## 11. Pages that fail to load

| ID | Question | Rule | Error message | Proposed GDS error message | Shown when | Seen on | Source |
|---|---|---|---|---|---|---|---|
| LOD-01 | Create record: user lookup | Heading: "Unable to load current user" | `There was a problem retrieving the current user. Please try again later.` | `Page, not error message: Sorry, there is a problem with the service. Try again later.` | - | Whole page | [page.tsx:23](../../frontend/src/app/portal/organisations/%5Bid%5D/records/create/page.tsx#L23) |
| LOD-02 | Create record: organisation lookup | Heading: "Unable to load organisation" | `There was a problem retrieving the organisation. Please try again later.` | `Page, not error message: Sorry, there is a problem with the service. Try again later.` | - | Whole page | [OrganisationPageWrapper.tsx:55](../../frontend/src/app/portal/organisations/%5Bid%5D/_components/OrganisationPageWrapper.tsx#L55) |
| LOD-03 | Create record: Super user | Heading: "User is not permitted to view this page" | `The current user is not permitted to view this page.` | `Page, not error message: You do not have permission to view this page` | - | Whole page | [NotPermitted.tsx:11](../../frontend/src/app/portal/organisations/%5Bid%5D/_components/NotPermitted.tsx#L11) |
| LOD-04 | Task list | Heading: "Unable to load task list" | `There was a problem retrieving this task list. Please try again later.` | `Page, not error message: Sorry, there is a problem with the service. Try again later.` | - | Whole page | [page.tsx:70](../../frontend/src/app/portal/records/%5BrecordId%5D/task-list/page.tsx#L70) |
| LOD-05 | Form page | Heading: "Unable to load page" | `There was a problem retrieving this page. Please try again later.` | `Page, not error message: Sorry, there is a problem with the service. Try again later.` | - | Whole page | [page.tsx:40](../../frontend/src/app/portal/records/%5BrecordId%5D/%5BpageId%5D/page.tsx#L40) |
| LOD-06 | Check your answers | Heading: "Unable to load check your answers" | `There was a problem retrieving your answers. Please try again later.` | `Page, not error message: Sorry, there is a problem with the service. Try again later.` | - | Whole page | [page.tsx:73](../../frontend/src/app/portal/records/%5BrecordId%5D/check-your-answers/page.tsx#L73) |
