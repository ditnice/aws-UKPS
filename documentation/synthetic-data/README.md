# Synthetic medicine records

Generates the medicine records, organisations and reference data that the API seeds locally when
`Seeding:ReseedOnStartup` is enabled.

| File                                     | Purpose                                                                                                                |
| ---------------------------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| `data.json.gz`                           | Source data: 500 synthetic records in the original flat format.                                                        |
| `generators/synthetic_reference_data.py` | Reference data, the therapeutic categories used to classify each indication, and the non-pharmaceutical organisations. |
| `generators/transform_synthetic_data.py` | Builds the seed data from `data.json.gz`.                                                                              |
| `generators/validate_synthetic_data.py`  | Checks the seed data against the model's enums, the reference data, and the conditional and date rules.                |

The output is written to
`backend/src/Persistence/Data/Seeding/SyntheticData/synthetic-medicine-records.json.gz`, which is
embedded in the API assembly and read by `SeedingDataPayloadFaker`.

Both data files are gzipped to stay within the repository's large-file limit. To read one, run
`zcat data.json.gz | less`.

## Regenerating

```sh
python3 documentation/synthetic-data/generators/transform_synthetic_data.py
python3 documentation/synthetic-data/generators/validate_synthetic_data.py
```

The scripts work from any directory.

Generation is deterministic, so rerunning it without changes produces an identical file. Commit
the regenerated file alongside any script changes.

## What is generated

The source supplies each record's products, indication, clinical trials, status and UK submission
date. The generator derives everything else so that answers agree with each other:

- The indication's disease is classified into a therapeutic category, which sets the therapeutic
  area, BNF chapter, cancer flag, mode of action, comparators and typical biomarkers.
- The mode of action sets the formulation and route of administration.
- Regulatory dates follow a timeline that suits the MHRA procedure. Dates on or before the record's
  last update are actual; later dates are estimates.
- Follow-up questions are only answered when their parent answer calls for them.
- 35 source companies become the pharmaceutical organisations, alongside 10 horizon scanning,
  4 strategic and 1 internal QA organisation.

All dates are relative to `TODAY` in `transform_synthetic_data.py`.

The seeder then gives each record a workflow history (drafts, QA reviews, publications, reviews
and status changes). Earlier revisions show estimates that later changed and answers that started
as Unknown, and each update records its field changes.
