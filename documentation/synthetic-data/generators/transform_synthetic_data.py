"""Builds sensible, internally consistent synthetic medicine records from data.json.

The source supplies each record's products, indication, trials, status and UK submission date.
Everything else is derived from those so that clinically linked answers agree (therapeutic area,
BNF chapter, cancer flag, mode of action, formulation, biomarkers), dates follow a plausible
regulatory timeline, and conditional questions are only answered where their parent answer
allows it.

Usage: python3 transform_synthetic_data.py [input] [output]

By default this reads ../data.json.gz and writes the seeder's embedded data file.
Files ending in .gz are read and written gzipped.
"""

import calendar
import gzip
import json
import random
import re
import sys
from datetime import date, timedelta
from pathlib import Path

sys.dont_write_bytecode = True

import synthetic_reference_data as ref  # noqa: E402

SCRIPT_DIRECTORY = Path(__file__).resolve().parent
REPOSITORY_ROOT = SCRIPT_DIRECTORY.parents[2]
DEFAULT_INPUT = SCRIPT_DIRECTORY.parent / "data.json.gz"
DEFAULT_OUTPUT = (
    REPOSITORY_ROOT
    / "backend/src/Persistence/Data/Seeding/SyntheticData/synthetic-medicine-records.json.gz"
)

TODAY = date(2026, 10, 1)
EARLIEST_RECORD_DATE = date(2024, 1, 15)
PHARMA_ORGANISATION_COUNT = 35
UNPUBLISHED_RECORD_COUNT = 20
# Matches RecordService: medicine records are due an update 3 months after review, or 6 months
# when on hold.
UPDATE_DUE_DAYS = {"Active": 91, "OnHold": 182}
MONTHS = {name: number for number, name in enumerate(calendar.month_name) if name}

# --- Helpers ----------------------------------------------------------------------------------


def weighted(rng, options):
    """Picks a key from a {value: weight} mapping."""
    values = list(options)
    return rng.choices(values, weights=[options[value] for value in values])[0]


def days(rng, low, high):
    return timedelta(days=rng.randint(low, high))


def clamp(value, low, high):
    return max(low, min(high, value))


def month_start(value):
    return value.replace(day=1)


def quarter_start(value):
    return date(value.year, 3 * ((value.month - 1) // 3) + 1, 1)


def regulatory_date(value, as_of, rng):
    """Dates the record knew had happened are actual; later ones are still estimates."""
    if value is None:
        return None
    if value <= as_of:
        return {
            "dateValue": value.isoformat(),
            "datePrecision": "ActualDate",
            "isConfidential": rng.random() < 0.03,
        }
    if rng.random() < 0.7:
        precision, value = "EstimatedMonth", month_start(value)
    else:
        precision, value = "EstimatedQuarter", quarter_start(value)
    return {
        "dateValue": value.isoformat(),
        "datePrecision": precision,
        "isConfidential": rng.random() < 0.1,
    }


def yes_no_unknown(rng, yes, no, unknown):
    return weighted(rng, {"Yes": yes, "No": no, "Unknown": unknown})


def pick_some(rng, values, low, high):
    count = min(len(values), rng.randint(low, high))
    return rng.sample(values, count)


def parse_source_date(value):
    estimated = re.fullmatch(r"Q[1-4] (\d{4}) / (\w+)", value)
    if estimated:
        return date(int(estimated.group(1)), MONTHS[estimated.group(2)], 1)
    return date.fromisoformat(value[:10])


# --- Indication parsing -----------------------------------------------------------------------

POPULATIONS = {
    "adult patients": "adults",
    "adults": "adults",
    "children": "children",
    "infants": "infants",
    "patients": "patients",
}

INDICATION_PATTERNS = [
    (
        r"\S+ in combination with (?P<combo>.+?) is indicated for the treatment of "
        r"(?P<disease>.+?) in (?P<population>.+?)\.",
        "combination",
        "In combination with {combo}, {population} with {disease}",
    ),
    (
        r"\S+ is indicated as monotherapy for the treatment of relapsed or refractory "
        r"(?P<disease>.+?) in (?P<population>.+?)\.",
        "relapsed",
        "Monotherapy, {population} with relapsed or refractory {disease}",
    ),
    (
        r"\S+ is indicated as first-line treatment of (?P<disease>.+?) in "
        r"(?P<population>.+?) not previously treated with systemic therapy\.",
        "first-line",
        "1st line, {population} with {disease}",
    ),
    (
        r"\S+ is indicated for the treatment of (?P<population>.+?) with (?P<disease>.+?) "
        r"whose disease has progressed on or after prior therapy\.",
        "later-line",
        "2nd line or later, {population} with {disease}",
    ),
    (
        r"\S+ is indicated for the treatment of (?P<disease>.+?) in (?P<population>.+?) "
        r"who have received at least one prior line of therapy\.",
        "later-line",
        "2nd line or later, {population} with {disease}",
    ),
    (
        r"\S+ is indicated for the treatment of (?P<disease>.+?) in (?P<population>.+?) "
        r"with confirmed (?P<marker>.+?)-positive status\.",
        "biomarker",
        "{population} with {marker}-positive {disease}",
    ),
]


def parse_indication(indication):
    for pattern, setting, title_template in INDICATION_PATTERNS:
        match = re.fullmatch(pattern, indication)
        if match:
            parts = match.groupdict()
            parts["disease"] = parts["disease"].strip('"')
            parts["population"] = POPULATIONS.get(parts["population"], parts["population"])
            title = title_template.format(**parts)
            return {
                "setting": setting,
                "disease": parts["disease"],
                "population": parts["population"],
                "combo": parts.get("combo"),
                "marker": parts.get("marker"),
                "title": title[0].upper() + title[1:],
            }
    raise ValueError(f"Unrecognised indication: {indication}")


def classify(disease, rng):
    """Returns the disease's category, and whether it was recognised rather than guessed."""
    for category in ref.CATEGORIES:
        if category.pattern.search(disease):
            return category, True
    return ref.CATEGORIES_BY_KEY[rng.choice(ref.FALLBACK_CATEGORY_KEYS)], False


def therapeutic_area(category, disease):
    for code, pattern in category.areas:
        if pattern is None or re.search(pattern, disease, re.IGNORECASE):
            return code
    raise AssertionError("Every category ends with a default area")


# --- Products ---------------------------------------------------------------------------------

BRAND_STARTS = ["Vel", "Zan", "Tri", "Lor", "Quin", "Ari", "Ster", "Nov", "Xel", "Cal", "Ory",
                "Bre", "Fen", "Dar", "Ily", "Mez", "Rov", "Sol", "Tav", "Vor", "Kei", "Pra"]
BRAND_MIDDLES = ["a", "e", "i", "o", "ra", "li", "ve", "no", "ta", "mi", ""]
BRAND_ENDS = ["vra", "zia", "tor", "lyn", "dex", "qor", "mira", "sta", "nex", "vio", "fyn", "ris"]


def modality(generic, is_atmp):
    if is_atmp:
        return "atmp"
    name = generic.lower()
    if name.endswith("mab"):
        return "ab"
    if name.endswith(("tide", "ase", "cept")):
        return "biologic"
    return "sm"


ATMP_MODES = {
    "Gene therapy medicinal product": ["AAV-based gene therapy", "Lentiviral ex vivo gene therapy"],
    "Somatic cell therapy medicinal product": [
        "Autologous CAR-T cell therapy",
        "Allogeneic mesenchymal stromal cell therapy",
    ],
    "Tissue engineered product": [
        "Tissue-engineered autologous chondrocyte implant",
        "Bioengineered skin substitute",
    ],
    "Combined ATMP": ["Cell-seeded biodegradable scaffold"],
}


def mode_of_action(rng, category, area, product_modality, atmp_classification):
    """Returns the mode of action and the kind of product it describes."""
    if product_modality == "atmp":
        return rng.choice(ATMP_MODES[atmp_classification]), "atmp"
    candidates = ref.AREA_MODES.get(area, category.modes)
    allowed = {"ab": {"ab"}, "sm": {"sm", "any"}, "biologic": {"bio", "ab", "any"}}
    modes = [(mode, kind) for mode, kind in candidates if kind in allowed[product_modality]]
    return rng.choice(modes or [(mode, kind) for mode, kind in candidates if kind != "atmp"])


def formulation(rng, category, kind, paediatric, atmp_classification, early_stage):
    """Picks a formulation that suits the kind of product the mode of action describes."""
    if early_stage and rng.random() < 0.08:
        return "Unknown at present"
    if rng.random() < 0.02:
        return "Other"
    if kind == "atmp":
        if atmp_classification in ("Tissue engineered product", "Combined ATMP"):
            return "Implant"
        return "Subretinal injection" if category.key == "ophthalmology" else "Intravenous infusion"
    if category.key == "ophthalmology":
        return "Intravitreal injection" if kind in ("ab", "bio") else weighted(
            rng, {"Eye drops": 6, "Tablet": 4})
    if kind == "bio" and category.key == "neurology":
        return weighted(rng, {"Intrathecal injection": 60, "Subcutaneous injection": 40})
    if kind == "ab":
        return weighted(rng, {
            "Intravenous infusion": 35,
            "Solution for infusion": 10,
            "Powder for solution for infusion": 15,
            "Subcutaneous injection": 35,
            "Intravenous injection": 5,
        })
    if kind == "bio":
        return weighted(rng, {
            "Subcutaneous injection": 50,
            "Intravenous infusion": 25,
            "Powder for solution for injection": 15,
            "Intramuscular injection": 10,
        })
    if paediatric == "ExclusivelyChildren":
        return weighted(rng, {
            "Oral solution": 30,
            "Oral suspension": 20,
            "Dispersible tablet": 20,
            "Granules": 10,
            "Powder for oral solution": 10,
            "Tablet": 10,
        })
    if category.key == "dermatology":
        return weighted(rng, {"Cream": 30, "Gel": 15, "Cutaneous solution": 5, "Tablet": 35,
                              "Capsule": 15})
    if category.key in ("respiratory", "allergy"):
        return weighted(rng, {"Dry powder inhaler": 35, "Metered dose inhaler": 20,
                              "Solution for nebuliser": 10, "Nasal spray": 5, "Tablet": 30})
    if category.key == "infection":
        return weighted(rng, {"Intravenous infusion": 30, "Powder for solution for infusion": 15,
                              "Tablet": 35, "Capsule": 20})
    return weighted(rng, {"Tablet": 50, "Capsule": 25, "Enteric coated tablet": 8,
                          "Oral solution": 7, "Subcutaneous infusion": 3, "Intrathecal injection": 2,
                          "Implant": 1})


def presentation_and_dose(rng, form):
    strength = rng.choice([5, 10, 20, 25, 40, 50, 75, 100, 150, 200, 250, 300, 400])
    concentration = rng.choice([10, 20, 25, 50, 100])
    if form in ("Tablet", "Enteric coated tablet", "Dispersible tablet"):
        kind = {"Tablet": "Film-coated tablets", "Enteric coated tablet": "Gastro-resistant tablets",
                "Dispersible tablet": "Dispersible tablets"}[form]
        return (f"{kind}, {strength} mg, blister packs of {rng.choice([28, 30, 56, 84])}",
                f"{strength} mg orally {rng.choice(['once', 'twice'])} daily")
    if form == "Capsule":
        return (f"Hard capsules, {strength} mg, bottles of {rng.choice([30, 60, 90])}",
                f"{strength} mg orally {rng.choice(['once', 'twice'])} daily with food")
    if form in ("Oral solution", "Oral suspension", "Powder for oral solution", "Granules"):
        return (f"{form}, {concentration} mg/mL when prepared, supplied with an oral dosing syringe",
                f"{rng.choice([2, 3, 5, 10])} mg/kg orally twice daily (maximum {strength} mg per dose)")
    if form in ("Intravenous infusion", "Solution for infusion"):
        return (f"Concentrate for solution for infusion, {concentration} mg/mL, single-use "
                f"{rng.choice([4, 10, 20])} mL vial",
                f"{rng.choice([3, 5, 10, 15])} mg/kg by intravenous infusion every "
                f"{rng.choice([2, 3, 4])} weeks")
    if form in ("Powder for solution for infusion", "Powder for solution for injection"):
        return (f"Lyophilised powder, {strength} mg single-dose vial for reconstitution",
                f"{strength} mg every {rng.choice([2, 3, 4, 6])} weeks")
    if form in ("Subcutaneous injection", "Subcutaneous infusion", "Intramuscular injection"):
        return (f"Solution for injection in pre-filled pen, {strength} mg/{rng.choice([1, 2])} mL",
                f"{strength * 2} mg loading dose, then {strength} mg every "
                f"{rng.choice([1, 2, 4, 8])} weeks")
    if form == "Intravenous injection":
        return (f"Solution for injection, {concentration} mg/mL, {rng.choice([2, 5])} mL ampoule",
                f"{strength} mg by slow intravenous injection once daily for "
                f"{rng.choice([5, 7, 14])} days")
    if form in ("Intravitreal injection", "Subretinal injection"):
        return (f"Solution for injection, {concentration} mg/mL, single-dose vial with filter needle",
                "Monthly injections for 3 doses, then every 8 to 16 weeks"
                if form == "Intravitreal injection" else "Single subretinal administration per eye")
    if form == "Eye drops":
        return (f"Eye drops, solution, {concentration / 10:g} mg/mL, 5 mL multi-dose bottle",
                "One drop in the affected eye(s) twice daily")
    if form in ("Cream", "Gel", "Cutaneous solution"):
        return (f"{form}, {rng.choice([0.05, 0.1, 1, 2])}% w/w, 30 g and 60 g tubes",
                "Apply a thin layer to affected skin twice daily")
    if form in ("Dry powder inhaler", "Metered dose inhaler", "Solution for nebuliser", "Nasal spray"):
        return (f"{form}, {rng.choice([50, 100, 200, 250])} micrograms per actuation, 30-day supply",
                f"{rng.choice([1, 2])} inhalation(s) twice daily")
    if form == "Intrathecal injection":
        return (f"Solution for intrathecal injection, {concentration} mg/5 mL vial",
                "Four loading doses over 2 months, then every 4 months")
    if form == "Implant":
        return ("Single-use implant supplied in a pre-loaded applicator",
                "Single surgical implantation")
    return (None, None)


TREATMENT_DURATIONS = {
    "oncology": ["Until disease progression or unacceptable toxicity", "Up to 2 years",
                 "6 cycles (21-day cycles)", "12 months"],
    "infection": ["7 to 14 days", "Single dose", "6 months", "Up to 28 days"],
    "viral": ["8 to 12 weeks", "Lifelong", "5 days"],
    "pain": ["Single dose", "Up to 12 weeks", "As required"],
}
DEFAULT_DURATIONS = ["Long-term (indefinite)", "12 months", "Indefinite, reviewed annually",
                     "Up to 24 months"]

PLACE_IN_THERAPY = {
    "first-line": "First-line option",
    "later-line": "Second-line option after failure of standard therapy",
    "relapsed": "Treatment option for relapsed or refractory disease",
    "combination": "Add-on to standard care",
    "biomarker": "Targeted option for biomarker-positive patients",
}

MONITORING_TESTS = {
    "oncology": ["CT imaging every 9 to 12 weeks to assess tumour response.",
                 "Circulating tumour DNA at progression to guide subsequent treatment."],
    "haematology": ["Full blood count and reticulocytes monthly for the first 6 months.",
                    "Haemoglobin and LDH every 4 weeks."],
    "viral": ["Viral load at weeks 4, 12 and 24.", "Sustained virological response 12 weeks "
              "after the end of treatment."],
    "infection": ["Clinical and microbiological cure assessed at the test-of-cure visit.",
                  "Therapeutic drug monitoring of trough levels."],
    "endocrine": ["HbA1c every 3 months.", "Plasma substrate levels every 6 months."],
    "neurology": ["MRI at 6 and 12 months to assess disease activity.",
                  "Functional rating scale every 3 months."],
    "rheumatology": ["Disease activity score (DAS28) at 12 and 24 weeks."],
    "dermatology": ["Psoriasis Area and Severity Index (PASI) at week 16."],
    "ophthalmology": ["Optical coherence tomography before each injection."],
    "respiratory": ["Spirometry (FEV1) at 12 and 52 weeks."],
    "cardiovascular": ["Echocardiogram at 12 weeks and every 6 months thereafter."],
}
DEFAULT_MONITORING_TESTS = ["Clinical review at 12 weeks to assess response."]
SAFETY_TESTS = [
    "ECG at baseline and week 2 to monitor the QTc interval.",
    "Full blood count before each treatment cycle.",
    "Liver function tests every 4 weeks for the first 3 months.",
    "Renal function before each dose.",
    "Screening for latent tuberculosis before starting treatment.",
]

# --- Free text --------------------------------------------------------------------------------

PARTNER_SUFFIXES = ["Therapeutics", "Biosciences", "Pharmaceuticals", "Oncology", "Pharma"]

HTA_DETAIL_TEMPLATES = {
    "Nice": [
        "NICE: Single technology appraisal anticipated; evidence submission planned around the "
        "time of UK licence.",
        "NICE: Scoping expected 6 months before UK licence; cost-comparison route under "
        "consideration.",
        "NICE: Highly specialised technologies evaluation may be appropriate given the small "
        "eligible population.",
    ],
    "Smc": [
        "SMC: Full submission planned within 3 months of UK marketing authorisation.",
        "SMC: Submission via the orphan or end-of-life process is being considered.",
    ],
    "Awmsg": [
        "AWMSG: Submission planned if the product is not appraised by NICE.",
        "AWMSG: Will follow the NICE recommendation for Wales.",
    ],
}

ON_HOLD_NOTES = {
    "FilingWithdrawn": [
        "Marketing authorisation application withdrawn to address quality (CMC) questions; "
        "resubmission planned.",
        "UK filing withdrawn while additional efficacy data from the ongoing phase III trial "
        "mature.",
    ],
    "TrialSuspended": [
        "Pivotal trial {trial} paused pending independent data monitoring committee review.",
        "Recruitment to {trial} suspended following a protocol amendment.",
    ],
    "AwaitingExternalClarification": [
        "Awaiting MHRA confirmation of the eligible regulatory route.",
        "Awaiting confirmation from the global partner on UK launch sequencing.",
    ],
    "Other": [
        "Global development timelines under review following portfolio prioritisation.",
        "UK plans paused while manufacturing capacity is reviewed.",
    ],
}

ARCHIVE_NOTES = {
    "DevelopmentDiscontinued": [
        "Development discontinued following a phase III futility analysis.",
        "Global development discontinued for commercial reasons.",
        "Development discontinued ahead of anticipated UK availability.",
    ],
    "Other": [
        "Duplicate of another record for the same product and indication.",
        "Marketing rights transferred to another company, which has created a new record.",
    ],
}

# How the source data marks records that were archived automatically, which is also the note
# used for most automatic archives.
AUTO_ARCHIVE_NOTE = (
    "Archived automatically: product has been available in the UK for more than 6 months."
)
NOT_UPDATED_ARCHIVE_NOTE = (
    "Archived automatically: record has not been updated within the required period."
)
NOT_UPDATED_ARCHIVE_COUNT = 10
# Placeholder until the period is agreed: records not reviewed for this long are archived.
NOT_UPDATED_ARCHIVE_DAYS = 365

# --- Organisations ----------------------------------------------------------------------------

UK_TELEPHONES = {
    "London": "020 7946 0{:03d}",
    "Manchester": "0161 496 0{:03d}",
    "Leeds": "0113 496 0{:03d}",
    "Birmingham": "0121 496 0{:03d}",
    "Edinburgh": "0131 496 0{:03d}",
    "Cardiff": "029 2018 0{:03d}",
    "Belfast": "028 9018 0{:03d}",
}


def organisation(rng, name, organisation_type, allowed_entity, created_at, last_active):
    town, postcode = rng.choice(ref.TOWNS)
    slug = re.sub(r"[^a-z0-9]+", "-", name.lower()).strip("-")
    telephone = UK_TELEPHONES.get(town, "020 7946 0{:03d}").format(rng.randint(0, 999))
    return {
        "organisationName": name,
        "organisationType": organisation_type,
        "allowedPharmaceuticalEntity": allowed_entity,
        "countryOrRegion": "United Kingdom",
        "headOfficeAddress": f"{rng.randint(1, 250)} {rng.choice(ref.STREETS)}, {town}, "
        f"{postcode} {rng.randint(1, 9)}{rng.choice('ABDEFGHJLNPQRSTUWXYZ')}"
        f"{rng.choice('ABDEFGHJLNPQRSTUWXYZ')}",
        "headOfficeTelephone": telephone,
        "headOfficeEmail": f"enquiries@{slug}.example.com",
        "status": "Active",
        "createdAt": created_at.isoformat(),
        "lastActive": last_active.isoformat() if last_active else None,
    }


def company_code_prefix(name):
    words = [word for word in re.findall(r"[A-Za-z]+", name) if word.lower() != "and"]
    return "".join(word[0] for word in words[:3]).upper()


# --- Record generation ------------------------------------------------------------------------


class RecordBuilder:
    def __init__(self, partner_names):
        self.partner_names = partner_names
        self.used_brands = set()
        self.used_codes = set()
        self.used_ta_ids = set()

    def brand_name(self, rng):
        while True:
            name = rng.choice(BRAND_STARTS) + rng.choice(BRAND_MIDDLES) + rng.choice(BRAND_ENDS)
            if name not in self.used_brands:
                self.used_brands.add(name)
                return name

    def company_code(self, rng, organisation_name):
        prefix = company_code_prefix(organisation_name)
        while True:
            code = f"{prefix}-{rng.randint(100, 9999)}"
            if code not in self.used_codes:
                self.used_codes.add(code)
                return code

    def nice_ta_id(self, rng, highly_specialised):
        while True:
            value = (f"GID-HST{rng.randint(10050, 10099)}" if highly_specialised
                     else f"GID-TA{rng.randint(11000, 11999)}")
            if value not in self.used_ta_ids:
                self.used_ta_ids.add(value)
                return value

    def build(self, row, organisation_name, status, unpublished):
        rng = random.Random(f"ukps-record-{row['metadata']['id']}")
        indication = parse_indication(row["indication"])
        category, recognised = classify(indication["disease"], rng)
        generic = row["generic_name"]
        atmp_classification = row["atmp_classification"]
        product_modality = modality(generic, atmp_classification is not None)

        status, status_reason, status_note = status
        record_dates = self.record_dates(rng, status, status_reason, status_note, unpublished)
        as_of = record_dates["lastUpdatedAt"]
        procedure = self.procedure(rng, category, row)
        timeline = self.timeline(
            rng, row, status, status_reason, status_note, procedure, record_dates
        )

        paediatric = self.paediatric(rng, indication["population"])
        # Unrecognised diseases are the generic genetic conditions, which are all rare.
        is_rare = (
            row["eu_orphan_date"] is not None
            or not recognised
            or bool(ref.RARE_DISEASE_PATTERN.search(indication["disease"]))
            or (category.population_bias == "rare" and rng.random() < 0.15)
            or rng.random() < 0.04
        )
        early_stage = timeline["ukSubmission"] > as_of + timedelta(days=540)

        lab = self.laboratory_testing(rng, category, indication)
        area = therapeutic_area(category, indication["disease"])
        mode, kind = mode_of_action(rng, category, area, product_modality, atmp_classification)
        trials, recruiting = self.clinical_trials(
            rng, row, indication, generic, require_trial=status_reason == "TrialSuspended"
        )
        designations = self.special_designations(
            rng, is_rare, timeline, as_of, atmp_classification, category
        )

        record = {
            "sourceId": row["metadata"]["id"],
            "organisationName": organisation_name,
            "recordType": "Medicine",
            "recordStatus": status,
            "createdAt": record_dates["createdAt"].isoformat(),
            "lastUpdatedAt": as_of.isoformat(),
            "reviewedAt": record_dates["reviewedAt"].isoformat()
            if record_dates["reviewedAt"]
            else None,
            "statusChange": (
                {
                    "reason": status_reason,
                    "note": self.status_note(rng, status, status_reason, status_note, trials),
                    "changedAt": record_dates["statusChangedAt"].isoformat(),
                }
                if status in ("OnHold", "Archived")
                else None
            ),
            "productRecordDetails": {
                "namesAndIdentifiers": self.names(
                    rng, organisation_name, generic, indication, early_stage
                ),
            },
            "indicationAndDevelopmentInformation": {
                "indicationDetails": self.indication_details(
                    rng, row, indication, category, mode, kind, paediatric, is_rare,
                    lab, atmp_classification, early_stage,
                ),
                "developmentBackground": self.development_background(rng, row, indication),
            },
            "clinicalTrialInformation": {
                "recruitingInUk": recruiting,
                "clinicalTrials": trials,
            },
            "regulatoryAccessAndLaunchInformation": {
                "mhraProcedureAndDates": self.mhra_procedure_and_dates(
                    rng, procedure, timeline, as_of, designations
                ),
                "healthTechnologyAssessmentAndLaunch": self.hta(rng, timeline, as_of, is_rare),
                "specialDesignations": designations,
            },
            "serviceReadinessInformation": {
                "laboratoryTestingDetails": lab,
                "patientAndClinicalRequirements": self.patient_and_clinical(
                    rng, category, indication, is_rare, kind, lab
                ),
                "pricingAndBudgetImpact": self.pricing(rng, category, is_rare, as_of),
            },
        }

        if unpublished:
            self.leave_draft_incomplete(rng, record)

        return record

    # Record lifecycle --------------------------------------------------------------------------

    def record_dates(self, rng, status, status_reason, status_note, unpublished):
        if unpublished:
            created = TODAY - days(rng, 5, 90)
            return {
                "createdAt": created,
                "lastUpdatedAt": created + days(rng, 0, (TODAY - created).days),
                "reviewedAt": None,
                "statusChangedAt": None,
            }

        if status_reason == "ArchivedAutomatically":
            return self.automatic_archive_dates(rng, status_note)

        due_days = UPDATE_DUE_DAYS.get(status, UPDATE_DUE_DAYS["Active"])
        overdue = status == "Active" and rng.random() < 0.2
        if overdue:
            # Records left unreviewed for longer would have been archived automatically.
            reviewed = TODAY - days(rng, due_days + 5, NOT_UPDATED_ARCHIVE_DAYS - 5)
        else:
            reviewed = TODAY - days(rng, 0, due_days - 5)
        updated = reviewed if rng.random() < 0.4 else reviewed - days(rng, 1, 120)
        created = max(EARLIEST_RECORD_DATE, updated - days(rng, 30, 600))
        updated = max(updated, created)
        reviewed = max(reviewed, updated)
        status_changed = None
        if status in ("OnHold", "Archived"):
            # Changing the status is the latest update, and confirms the record at the same time.
            reviewed = updated
            status_changed = updated
        return {"createdAt": created, "lastUpdatedAt": updated, "reviewedAt": reviewed,
                "statusChangedAt": status_changed}

    def automatic_archive_dates(self, rng, status_note):
        """The system archives a record some time after its last update, which is not a review."""
        archived = TODAY - days(rng, 0, 120)
        if status_note == NOT_UPDATED_ARCHIVE_NOTE:
            reviewed = archived - days(rng, NOT_UPDATED_ARCHIVE_DAYS + 1, NOT_UPDATED_ARCHIVE_DAYS + 90)
        else:
            reviewed = archived - days(rng, 1, 120)
        created = max(EARLIEST_RECORD_DATE, reviewed - days(rng, 30, 400))
        reviewed = max(reviewed, created)
        return {"createdAt": created, "lastUpdatedAt": reviewed, "reviewedAt": reviewed,
                "statusChangedAt": archived}

    def status_note(self, rng, status, reason, source_note, trials):
        if source_note is not None:
            return source_note
        notes = (ARCHIVE_NOTES if status == "Archived" else ON_HOLD_NOTES)[reason]
        if reason == "TrialSuspended":
            return rng.choice(notes).format(trial=trials[0]["clinicalTrialsGovNumber"])
        return rng.choice(notes) if rng.random() < 0.9 else None

    def leave_draft_incomplete(self, rng, record):
        """Unpublished drafts are often missing their optional later sections."""
        service = record["serviceReadinessInformation"]
        if rng.random() < 0.6:
            for subsection in service.values():
                for key, value in subsection.items():
                    subsection[key] = [] if isinstance(value, list) else None
        if rng.random() < 0.3:
            designations = record["regulatoryAccessAndLaunchInformation"]["specialDesignations"]
            for key in designations:
                designations[key] = None

    # Regulatory timeline -----------------------------------------------------------------------

    PROCEDURE_DURATIONS = {
        "MHRA national assessment procedure - standard": (330, 420),
        "MHRA national assessment procedure - accelerated": (150, 240),
        "International Recognition Procedure": (60, 200),
        "EC decision reliance procedure": (60, 90),
        "EU mutual recognition reliance procedure": (90, 150),
        "Access consortium": (240, 330),
        "Project Orbis": (180, 270),
        "Rolling review procedure": (200, 300),
        "Unknown / Other – please provide details": (240, 400),
    }

    def procedure(self, rng, category, row):
        weights = {
            "International Recognition Procedure": 30,
            "MHRA national assessment procedure - standard": 20,
            "MHRA national assessment procedure - accelerated": 10,
            "Access consortium": 8,
            "Rolling review procedure": 5,
            "EC decision reliance procedure": 4,
            "EU mutual recognition reliance procedure": 2,
            "Unknown / Other – please provide details": 11,
        }
        if category.is_cancer:
            weights["Project Orbis"] = 12
        return weighted(rng, weights)

    def timeline(self, rng, row, status, status_reason, status_note, procedure, record_dates):
        as_of = record_dates["lastUpdatedAt"]
        low, high = self.PROCEDURE_DURATIONS[procedure]
        submission = parse_source_date(row["uk_submission_date"])
        if row["uk_submission_date_type"] != "actual":
            submission += days(rng, 0, 27)

        if status_reason == "ArchivedAutomatically" and status_note == AUTO_ARCHIVE_NOTE:
            # Archived once the product had been available in the UK for more than 6 months.
            launch = record_dates["statusChangedAt"] - days(rng, 190, 540)
            licence = launch - days(rng, 30, 150)
            submission = licence - days(rng, low, high)
        else:
            licence = submission + days(rng, low, high)
            launch = licence + days(rng, 20, 180)
            if status == "Archived" or status_reason == "FilingWithdrawn":
                # Discontinued or withdrawn products never reached a UK licence.
                shift = max(timedelta(0), as_of - licence + days(rng, 60, 400))
            elif launch < as_of - timedelta(days=150):
                # Products available for more than 6 months would have been archived.
                shift = as_of - launch + days(rng, -150, 400)
            else:
                shift = timedelta(0)
            submission, licence, launch = submission + shift, licence + shift, launch + shift
            if status_reason == "FilingWithdrawn":
                submission = min(submission, as_of - days(rng, 30, 300))
            if status_reason == "TrialSuspended":
                submission = max(submission, as_of + days(rng, 120, 500))
                licence = max(licence, submission + days(rng, low, high))
                launch = max(launch, licence + days(rng, 20, 180))

        return {"ukSubmission": submission, "ukLicence": licence, "ukLaunch": launch}

    def mhra_procedure_and_dates(self, rng, procedure, timeline, as_of, designations):
        submission = timeline["ukSubmission"]
        is_irp = procedure == "International Recognition Procedure"
        result = {
            "mhraProcedureType": procedure,
            "procedureDetails": None,
            "ukSubmissionDate": regulatory_date(submission, as_of, rng),
            "globalFirstSubmissionRegion": None,
            "globalSubmissionActualDate": None,
            "ukLicenceDate": regulatory_date(timeline["ukLicence"], as_of, rng),
            "ukConditionalApprovalAnticipated": None,
            "irpReferenceRegulator": None,
            "irpRoute": None,
            "intlSubmissionDate": None,
            "intlLicenceDate": None,
            "intlConditionalApprovalAnticipated": None,
        }

        if procedure.startswith("Unknown"):
            result["procedureDetails"] = rng.choice([
                "Route to be confirmed following a scientific advice meeting with the MHRA.",
                "Considering the Innovative Licensing and Access Pathway (ILAP); route not yet "
                "confirmed.",
                "Dependent on the timing of the US and EU approvals.",
            ])
        elif rng.random() < 0.25:
            result["procedureDetails"] = rng.choice([
                "Pre-submission meeting with the MHRA held; no major objections raised.",
                "Clock-stop expected for responses to the day 120 questions.",
                "Submission will include data from the ongoing long-term extension study.",
                "Paediatric investigation plan compliance check to be completed before "
                "submission.",
            ])

        expedited = procedure in (
            "MHRA national assessment procedure - accelerated",
            "Project Orbis",
        ) or designations["pimDesignationStatus"] == "Granted"
        result["ukConditionalApprovalAnticipated"] = (
            yes_no_unknown(rng, 45, 30, 25) if expedited else yes_no_unknown(rng, 10, 65, 25)
        )

        if is_irp:
            regulator = weighted(rng, {"European Union": 55, "United States": 30, "Canada": 5,
                                       "Japan": 3, "Switzerland": 3, "Australia": 2,
                                       "Singapore": 2})
            route = weighted(rng, {"Recognition A": 60, "Recognition B": 40})
            gap = (20, 300) if route == "Recognition A" else (300, 1500)
            intl_licence = submission - days(rng, *gap)
            intl_submission = intl_licence - days(rng, 300, 420)
            result.update({
                "irpReferenceRegulator": regulator,
                "irpRoute": route,
                "intlSubmissionDate": regulatory_date(intl_submission, as_of, rng),
                "intlLicenceDate": regulatory_date(intl_licence, as_of, rng),
                "intlConditionalApprovalAnticipated": yes_no_unknown(rng, 15, 60, 25),
                "globalFirstSubmissionRegion": regulator,
            })
            global_date = intl_submission
        elif rng.random() < 0.75:
            region = weighted(rng, {"United States": 45, "European Union": 30, "Japan": 7,
                                    "China": 6, "United Kingdom": 5, "Switzerland": 4,
                                    "Australia": 3})
            result["globalFirstSubmissionRegion"] = region
            global_date = submission if region == "United Kingdom" else submission - days(
                rng, 0, 400)
        else:
            global_date = None

        # Only the actual global first submission date is collected.
        if global_date is not None and global_date <= as_of:
            result["globalSubmissionActualDate"] = regulatory_date(global_date, as_of, rng)

        return result

    def hta(self, rng, timeline, as_of, is_rare):
        intended = weighted(rng, {"Yes": 82, "No": 6, "Unknown": 12})
        bodies = None
        details = None
        aligned = None
        ta_id = None
        if intended == "Yes":
            bodies = [body for body, chance in (("Nice", 0.9), ("Smc", 0.75), ("Awmsg", 0.35))
                      if rng.random() < chance] or ["Nice"]
            if rng.random() < 0.55:
                details = "\n".join(rng.choice(HTA_DETAIL_TEMPLATES[body]) for body in bodies)
            if "Nice" in bodies:
                aligned = yes_no_unknown(rng, 30, 45, 25)
                if rng.random() < 0.75:
                    ta_id = self.nice_ta_id(rng, is_rare and rng.random() < 0.3)
        return {
            "medicineHtaSubmissionIntended": intended,
            "medicineHtaBodies": bodies,
            "htaAdditionalDetails": details,
            "htaNiceAlignedPathway": aligned,
            "niceTaDevelopmentId": ta_id,
            "ukLaunchDate": regulatory_date(timeline["ukLaunch"], as_of, rng),
        }

    def special_designations(self, rng, is_rare, timeline, as_of, atmp_classification,
                             category):
        submission = timeline["ukSubmission"]

        # EU orphan designation.
        if is_rare:
            orphan_status = weighted(rng, {"Granted": 45, "ApplicationSubmittedDecisionPending": 15,
                                           "DecisionToSubmitOngoing": 10,
                                           "NoSubmissionIntended": 15, "NotGranted": 5, None: 10})
        else:
            orphan_status = weighted(rng, {"NoSubmissionIntended": 60, None: 40})
        orphan_date = orphan_number = None
        if orphan_status == "Granted":
            granted = min(as_of - days(rng, 1, 60), submission - days(rng, 200, 2000))
            orphan_date = regulatory_date(granted, as_of, rng)
            orphan_number = f"EU/3/{granted.year % 100:02d}/{rng.randint(1000, 2999)}"

        # EU ATMP classification.
        atmp_class = atmp_date = None
        if atmp_classification is not None:
            atmp_status = weighted(rng, {"Granted": 75, "ApplicationSubmittedDecisionPending": 15,
                                         "NotGranted": 10})
            if atmp_status in ("Granted", "NotGranted"):
                recommended = min(as_of - days(rng, 1, 60), submission - days(rng, 300, 1500))
                atmp_date = regulatory_date(recommended, as_of, rng)
            if atmp_status == "Granted":
                atmp_class = atmp_classification
        else:
            atmp_status = weighted(rng, {"NoSubmissionIntended": 80, None: 20})

        # PIM designation and EAMS. An EAMS scientific opinion requires a PIM designation.
        pim_weights = {"NoSubmissionIntended": 45, None: 20, "DecisionToSubmitOngoing": 10,
                       "ApplicationSubmittedDecisionPending": 8, "Granted": 10, "NotGranted": 7}
        if category.is_cancer or is_rare:
            pim_weights["Granted"] = 22
        pim_status = weighted(rng, pim_weights)
        if pim_status == "Granted":
            will_submit = yes_no_unknown(rng, 50, 30, 20)
        else:
            will_submit = weighted(rng, {"No": 60, "Unknown": 25, None: 15})

        eams_submission = eams_opinion = decision = None
        if will_submit == "Yes":
            submitted = timeline["ukLicence"] - days(rng, 240, 420)
            opinion = submitted + days(rng, 60, 120)
            eams_submission = regulatory_date(submitted, as_of, rng)
            eams_opinion = regulatory_date(opinion, as_of, rng)
            if opinion <= as_of:
                decision = weighted(rng, {"Positive": 85, "Negative": 15})

        return {
            "euOrphanStatus": orphan_status,
            "euOrphanGrantedDate": orphan_date,
            "euOrphanStatusNumber": orphan_number,
            "euAtmpClassificationStatus": atmp_status,
            "atmpRecommendationDate": atmp_date,
            "atmpClassification": atmp_class,
            "pimDesignationStatus": pim_status,
            "willSubmitToEams": will_submit,
            "eamsSubmissionDate": eams_submission,
            "eamsOpinionDate": eams_opinion,
            "eamsOpinionDecision": decision,
        }

    # Product and indication --------------------------------------------------------------------

    def names(self, rng, organisation_name, generic, indication, early_stage):
        code = self.company_code(rng, organisation_name)
        has_brand = rng.random() < (0.45 if early_stage else 0.85)
        other_identifiers = []
        if rng.random() < 0.3:
            other_identifiers.append(f"{code}{rng.choice(['A', 'B', '-01', '-SC', '-IV'])}")
        if rng.random() < 0.15:
            partner = rng.choice(self.partner_names)
            other_identifiers.append(f"{company_code_prefix(partner)}-{rng.randint(100, 9999)}")
        return {
            "companyCode": code,
            "brandedName": self.brand_name(rng) if has_brand else None,
            "genericNames": [generic],
            "otherIdentifiers": other_identifiers,
            "recordTitle": indication["title"],
        }

    def paediatric(self, rng, population):
        if population in ("children", "infants"):
            return "ExclusivelyChildren"
        if population == "adults":
            return weighted(rng, {"ExclusivelyAdults": 85, "Unknown": 15})
        return weighted(rng, {"BothChildrenAndAdults": 60, "ExclusivelyAdults": 20, "Unknown": 20})

    def indication_details(self, rng, row, indication, category, mode, kind, paediatric,
                           is_rare, lab, atmp_classification, early_stage):
        area = therapeutic_area(category, indication["disease"])
        areas = [area]
        if rng.random() < 0.15:
            secondary = {"oncology": "3.4", "immunology": "9.1", "neurology": "3.5",
                         "rheumatology": "3.4", "dermatology": "9.1"}.get(category.key)
            if secondary:
                areas.append(secondary)

        form = formulation(rng, category, kind, paediatric, atmp_classification, early_stage)
        presentation, dose = presentation_and_dose(rng, form)

        technology = row["technology_status"] or ["NewChemicalOrBiologicalEntity"]
        technology = [
            {"New chemical / biological entity": "NewChemicalOrBiologicalEntity",
             "New indication": "NewIndication", "New formulation": "NewFormulation",
             "New dosing regimen": "NewDosingRegimen", "New presentation": "NewPresentation",
             "Biosimilar": "Biosimilar"}.get(value, value)
            for value in technology
        ]

        personalised = (
            "Yes"
            if lab["diagnosticTestRequired"] == "Yes" and lab["biomarkerType"] != "Unknown"
            else yes_no_unknown(rng, 0, 80, 20)
        )

        return {
            "indication": row["indication"],
            "bnfChapter": self.bnf_chapter(rng, category, area, indication),
            "therapeuticAreas": [self.reference_entry(ref.THERAPEUTIC_AREAS, code)
                                 for code in areas],
            "indicationIsPaediatric": paediatric,
            "indicationIsCancer": "Yes" if category.is_cancer else yes_no_unknown(rng, 0, 97, 3),
            "indicationIsRareDisease": "Yes" if is_rare else yes_no_unknown(rng, 0, 85, 15),
            "formulationType": form,
            "presentation": presentation if presentation and rng.random() < 0.75 else None,
            "modeOfAction": mode if rng.random() < 0.92 else None,
            "proposedDoseRegimen": dose if dose and rng.random() < 0.8 else None,
            "isPersonalisedMedicine": personalised,
            "medicineTechnologyStatus": technology,
        }

    def bnf_chapter(self, rng, category, area, indication):
        if category.is_cancer:
            mode_hint = indication["disease"].lower()
            code = "8.3" if re.search(r"prostat|breast", mode_hint) else rng.choice(["8.1", "8.2"])
        else:
            code = rng.choice(ref.AREA_BNF.get(area, category.bnf))
        return self.reference_entry(ref.BNF_CHAPTERS, code)

    @staticmethod
    def reference_entry(entries, code):
        label = dict(entries)[code]
        return {"code": code, "label": label}

    def development_background(self, rng, row, indication):
        technology = row["technology_status"] or []
        if "New chemical / biological entity" in technology or "Biosimilar" in technology:
            repurposed = weighted(rng, {"No": 90, "Unknown": 10})
        elif "New indication" in technology:
            repurposed = yes_no_unknown(rng, 45, 45, 10)
        else:
            repurposed = yes_no_unknown(rng, 5, 75, 20)

        originator = yes_no_unknown(rng, 78, 17, 5)
        co_marketed = yes_no_unknown(rng, 15, 65, 20)
        partners = rng.sample(self.partner_names, 2)

        return {
            "isRepurposedMedicine": repurposed,
            "repurposedMedicineDetails": (
                f"Currently licensed in the UK for another indication; this application extends "
                f"use to {indication['disease']}."
                if repurposed == "Yes" and rng.random() < 0.85
                else None
            ),
            "isOriginatorCompany": originator,
            "originatorCompanyName": partners[0] if originator == "No" else None,
            "isCoMarketed": co_marketed,
            "coMarketingCompanyName": partners[1] if co_marketed == "Yes" else None,
        }

    # Clinical trials ---------------------------------------------------------------------------

    def clinical_trials(self, rng, row, indication, generic, require_trial):
        source_trials = row["clinical_trials"]
        trials = [
            (trial["study_name"], trial["national_clinical_trial_number"],
             trial["clinical_trials_recruiting_in_the_uk"])
            for trial in source_trials
        ]
        if not trials and (require_trial or rng.random() < 0.75):
            for _ in range(rng.randint(1, 2)):
                trials.append((
                    f"A study of {generic} in {indication['population']} with "
                    f"{indication['disease']}",
                    f"NCT{rng.randint(10000000, 99999999)}",
                    weighted(rng, {"Yes, trials are open and recruiting in the UK": 35, "No": 40,
                                   "Unknown": 25}),
                ))

        # Trial phase and description are only collected for vaccines.
        result = [
            {
                "studyName": study_name,
                "clinicalTrialsGovNumber": number,
                "otherClinicalTrialNumbers": self.other_trial_numbers(rng),
                "trialPhase": None,
                "briefDescription": None,
            }
            for study_name, number, _ in trials
        ]

        if not trials:
            recruiting = None
        else:
            answers = {answer for _, _, answer in trials}
            if any(answer.startswith("Yes") for answer in answers):
                recruiting = "Yes"
            elif answers == {"No"}:
                recruiting = "No"
            else:
                recruiting = "Unknown"
        return result, recruiting

    def other_trial_numbers(self, rng):
        if rng.random() >= 0.35:
            return []
        year = rng.randint(2018, 2025)
        return [rng.choice([
            f"{year}-00{rng.randint(1000, 9999)}-{rng.randint(10, 99)}",
            f"ISRCTN{rng.randint(10000000, 99999999)}",
            f"{year}-5{rng.randint(10000, 99999)}-{rng.randint(10, 99)}-00",
        ])]

    # Service readiness -------------------------------------------------------------------------

    def laboratory_testing(self, rng, category, indication):
        empty = {key: None for key in (
            "diagnosticTestRequired", "biomarkerType", "nonGenomicBiomarkerDescription",
            "genomicTarget", "genomicTestNgtdRelationship", "genomicSampleType",
            "genomicTurnaroundTimeDetails", "patientPathwayPoint", "genomicTestPathwayPointOther",
            "genomicAlterations", "additionalGenomicFactors", "genomicTestUsedInTrials",
            "genomicTestSpecificitySensitivity", "genomicTestMandatoryStatus", "genomicTestNotes",
            "monitoringTestsDetails", "safetyTestsDetails",
        )}
        result = dict(empty)

        marker = indication["marker"]
        if marker:
            required = "Yes"
        else:
            chance = 0.45 if category.is_cancer else (
                0.2 if category.genomic_markers or category.non_genomic_markers else 0.05)
            required = "Yes" if rng.random() < chance else yes_no_unknown(rng, 0, 85, 15)
        result["diagnosticTestRequired"] = required

        if required == "Yes":
            genes = {gene: alteration for gene, alteration in category.genomic_markers}
            if marker and not re.search(r"CDK4/6", marker):
                genomic = True
                gene, alteration = marker, f"{marker} alteration"
            elif genes and (not category.non_genomic_markers or rng.random() < 0.7):
                genomic = True
                gene = rng.choice(list(genes))
                alteration = genes[gene]
            else:
                genomic = False

            if rng.random() < 0.05:
                result["biomarkerType"] = "Unknown"
            elif genomic:
                later_line = indication["setting"] in ("later-line", "relapsed")
                pathway = weighted(rng, {
                    "At diagnosis": 10 if later_line else 40,
                    "Before first-line treatment": 0 if later_line else 30,
                    "At disease progression or relapse": 40 if later_line else 10,
                    "Before second or subsequent line of treatment": 40 if later_line else 0,
                    "Other": 8,
                })
                result.update({
                    "biomarkerType": "GenomicBiomarker",
                    "genomicTarget": gene,
                    "genomicTestNgtdRelationship": weighted(rng, {
                        "NewTest": 35, "ExistingTestNewIndication": 30,
                        "ExistingTestSameIndication": 25, "Unknown": 10}),
                    "genomicSampleType": rng.choice(
                        ["Tumour tissue (FFPE block)", "Plasma (circulating tumour DNA)"]
                        if category.is_cancer
                        else ["Peripheral blood (EDTA)", "Saliva", "Dried blood spot"]),
                    "genomicTurnaroundTimeDetails": (
                        f"Results needed within {rng.choice([7, 10, 14])} working days so that "
                        f"the start of treatment is not delayed." if rng.random() < 0.6 else None),
                    "patientPathwayPoint": pathway,
                    "genomicTestPathwayPointOther": (
                        "At the multidisciplinary team meeting following imaging"
                        if pathway == "Other" else None),
                    "genomicAlterations": alteration,
                    "additionalGenomicFactors": rng.choice([
                        "Co-occurring STK11 or KEAP1 mutations may reduce response.",
                        "Tumour mutational burden may inform treatment sequencing.",
                        "Germline testing is recommended where a somatic variant is identified.",
                    ]) if rng.random() < 0.4 else None,
                    "genomicTestUsedInTrials": rng.choice([
                        "FoundationOne CDx", "Oncomine Dx Target Test", "Guardant360 CDx",
                        "Validated real-time PCR assay", "Whole genome sequencing",
                        "Laboratory-developed NGS panel",
                    ]) if rng.random() < 0.7 else None,
                    "genomicTestSpecificitySensitivity": (
                        f"Sensitivity of {rng.choice([90, 93, 95, 98])}% and specificity of "
                        f"{rng.choice([97, 98, 99])}% against reference sequencing"
                        if rng.random() < 0.5 else None),
                    "genomicTestMandatoryStatus": weighted(rng, {
                        "MandatoryNoAlternatives": 30, "MandatoryAlternativesMayExist": 35,
                        "RecommendedNotRequired": 25, "Unknown": 10}),
                    "genomicTestNotes": rng.choice([
                        "Assumes testing is delivered through the existing Genomic Laboratory Hub "
                        "network.",
                        "Uncertainty over whether liquid biopsy will be accepted where tissue is "
                        "insufficient.",
                        "Additional pathology capacity may be needed in the first year.",
                    ]) if rng.random() < 0.4 else None,
                })
            else:
                markers = category.non_genomic_markers or (
                    "Disease-specific serum biomarker above the upper limit of normal",)
                result.update({
                    "biomarkerType": "NonGenomicBiomarker",
                    "nonGenomicBiomarkerDescription": rng.choice(markers),
                    "patientPathwayPoint": weighted(rng, {"At diagnosis": 50,
                                                          "Before first-line treatment": 30,
                                                          "During treatment (response monitoring)": 20}),
                })

        if rng.random() < 0.35:
            result["monitoringTestsDetails"] = rng.choice(
                MONITORING_TESTS.get(category.key, DEFAULT_MONITORING_TESTS))
        if rng.random() < 0.35:
            result["safetyTestsDetails"] = rng.choice(SAFETY_TESTS)
        return result

    def patient_and_clinical(self, rng, category, indication, is_rare, kind, lab):
        screening = yes_no_unknown(rng, 12, 70, 18)
        urgent = yes_no_unknown(rng, 30 if category.key in ("oncology", "infection") else 8, 65,
                                20)
        changes = weighted(rng, {"NoChanges": 30, "SomeChange": 35, "CompleteTransformation": 8,
                                 "Unknown": 27})
        if kind == "atmp":
            changes = weighted(rng, {"SomeChange": 40, "CompleteTransformation": 60})
        needs_handling = kind in ("ab", "bio", "atmp")
        handling = (yes_no_unknown(rng, 70, 20, 10) if needs_handling
                    else yes_no_unknown(rng, 5, 80, 15))

        size = "rare" if is_rare else (
            "common" if category.key in ("cardiovascular", "respiratory", "psychiatry", "pain",
                                         "endocrine", "dermatology", "musculoskeletal",
                                         "gastroenterology") else "medium")
        population_range = self.population_range(rng, size)
        eligible = {"rare": (40, 900), "medium": (1000, 15000), "common": (20000, 400000)}[size]

        comparators = ", ".join(pick_some(rng, category.comparators, 1, 3))
        place = PLACE_IN_THERAPY[indication["setting"]]
        if is_rare and rng.random() < 0.3:
            place = "No other treatment available apart from best supportive care"

        return {
            "screeningRequired": screening,
            "screeningDetails": rng.choice([
                "Targeted case-finding in primary care using existing clinical records.",
                "Newborn screening would be needed to identify eligible infants early.",
                "Annual surveillance imaging in people at high genetic risk.",
            ]) if screening == "Yes" else None,
            "urgentIdentificationRequired": urgent,
            "urgentIdentificationDetails": rng.choice([
                "Treatment must start within 72 hours of diagnosis to be effective.",
                "Rapid referral pathway from emergency departments to specialist centres needed.",
                "Biomarker results needed before the first treatment cycle.",
            ]) if urgent == "Yes" else None,
            "proposedPlaceInTherapy": f"Place in therapy: {place} | Likely comparators: "
            f"{comparators}",
            "estimatedDurationOfTreatment": rng.choice(
                TREATMENT_DURATIONS.get(category.key, DEFAULT_DURATIONS))
            if rng.random() < 0.85 else None,
            "nhsServiceChangesRequired": changes,
            "nhsServiceChangesDetails": self.service_change_details(rng, kind, lab)
            if changes in ("SomeChange", "CompleteTransformation") else None,
            "handlingStorageRequirements": handling,
            "handlingStorageDetails": (
                "Shipped and stored in the vapour phase of liquid nitrogen (-150°C or below)."
                if kind == "atmp"
                else "Store refrigerated (2°C to 8°C). Do not freeze. Protect from light."
            ) if handling == "Yes" else None,
            "ukPatientPopulationRange": population_range,
            "ukPatientPopulationNotes": (
                f"Based on published {rng.choice(['UK', 'European', 'England'])} prevalence "
                f"estimates ({rng.randint(2018, 2025)}), adjusted for the licensed population."
                if population_range != "Unknown" and rng.random() < 0.6 else None),
            "estimatedEligiblePatientPopulation": (
                f"Approximately {round(rng.randint(*eligible), -1):,} patients in England each year"
                if population_range != "Unknown" and rng.random() < 0.7 else None),
        }

    def population_range(self, rng, size):
        if rng.random() < 0.08:
            return "Unknown"
        ranges = ref.UK_PATIENT_POPULATION_RANGES
        indexes = {"rare": range(0, 3), "medium": range(2, 7), "common": range(6, 12)}[size]
        return ranges[rng.choice(list(indexes))]

    def service_change_details(self, rng, kind, lab):
        options = [
            "Additional day-unit chair time needed for intravenous infusions.",
            "Homecare delivery service required for self-administered injections.",
            "Additional specialist nurse capacity to support treatment initiation.",
        ]
        if lab["biomarkerType"] == "GenomicBiomarker":
            options.append("New genomic test to be commissioned through the NHS Genomic Medicine "
                           "Service.")
        if kind == "atmp":
            options = ["Treatment limited to a small number of accredited specialist centres "
                       "with cell therapy facilities."]
        return rng.choice(options)

    def pricing(self, rng, category, is_rare, as_of):
        compassionate = yes_no_unknown(rng, 18, 55, 27)
        pas = yes_no_unknown(rng, 40, 15, 45)
        isp = yes_no_unknown(rng, 8, 62, 30)
        if is_rare:
            band = weighted(rng, {"LessThan5M": 75, "Between5MAnd40M": 15, "Unknown": 10})
        elif category.key in ("cardiovascular", "endocrine", "respiratory"):
            band = weighted(rng, {"Between5MAnd40M": 40, "FortyMOrMore": 25, "LessThan5M": 15,
                                  "Unknown": 20})
        else:
            band = weighted(rng, {"LessThan5M": 40, "Between5MAnd40M": 35, "FortyMOrMore": 5,
                                  "Unknown": 20})
        first = rng.randint(5, 20)
        second = first + rng.randint(5, 15)
        third = second + rng.randint(5, 15)
        opened = as_of - days(rng, 30, 400)
        return {
            "estimatedUptake": (f"Year 1: {first}%, year 2: {second}%, year 3: {third}% of the "
                                f"eligible population" if rng.random() < 0.65 else None),
            "compassionateAccessAvailable": compassionate,
            "compassionateAccessDetails": (
                f"Free-of-charge early access programme open to UK patients since "
                f"{calendar.month_name[opened.month]} {opened.year}."
                if compassionate == "Yes" else None),
            "patientAccessSchemePlanned": pas,
            "patientAccessSchemeRegions": (
                sorted(pick_some(rng, ["England", "Wales", "Scotland", "NorthernIreland"], 1, 4),
                       key=["England", "Wales", "Scotland", "NorthernIreland"].index)
                if pas == "Yes" else None),
            "indicationSpecificPricingPlanned": isp,
            "indicationSpecificPricingDetails": (
                "Different net prices proposed for first-line and later-line use."
                if isp == "Yes" else None),
            "netUkBudgetImpactBand": band,
        }


# --- Assembly ---------------------------------------------------------------------------------


def record_status(row):
    metadata = row["metadata"]
    if metadata["archived"]:
        if metadata["archived_details"] == AUTO_ARCHIVE_NOTE:
            return "Archived", "ArchivedAutomatically", AUTO_ARCHIVE_NOTE
        reason = {"Development discontinued": "DevelopmentDiscontinued"}.get(
            metadata["archived_reason"], "Other")
        if metadata["archived_details"] == "Development discontinued ahead of anticipated UK " \
                                           "availability.":
            reason = "DevelopmentDiscontinued"
        return "Archived", reason, None
    if metadata["on_hold"]:
        reason = {"Filing has been withdrawn": "FilingWithdrawn",
                  "Trial has been suspended": "TrialSuspended"}.get(metadata["on_hold_reason"])
        if reason is None:
            rng = random.Random(f"ukps-hold-{metadata['id']}")
            reason = weighted(rng, {"AwaitingExternalClarification": 60, "Other": 40})
        return "OnHold", reason, None
    return "Active", None, None


def read_json(path):
    opener = gzip.open if path.suffix == ".gz" else open
    with opener(path, "rt", encoding="utf-8") as file:
        return json.load(file)


def write_json(path, data):
    text = json.dumps(data, indent=2, ensure_ascii=False) + "\n"
    if path.suffix != ".gz":
        path.write_text(text, encoding="utf-8")
        return
    # A fixed timestamp keeps the compressed file identical when the data has not changed.
    with open(path, "wb") as raw, gzip.GzipFile(filename="", mode="wb", fileobj=raw, mtime=0) as file:
        file.write(text.encode("utf-8"))


def main():
    source = Path(sys.argv[1]) if len(sys.argv) > 1 else DEFAULT_INPUT
    target = Path(sys.argv[2]) if len(sys.argv) > 2 else DEFAULT_OUTPUT

    rows = read_json(source)["rows"]

    rng = random.Random("ukps-organisations")

    # Keep the most frequent source companies as the pharmaceutical organisations; the rest
    # become partner companies (originators, co-marketers) and their records are reassigned.
    counts = {}
    for row in rows:
        counts[row["company_name"]] = counts.get(row["company_name"], 0) + 1
    ranked = sorted(counts, key=lambda name: (-counts[name], name))
    pharma_names = ranked[:PHARMA_ORGANISATION_COUNT]
    partner_names = ranked[PHARMA_ORGANISATION_COUNT:]
    reassigned = {name: rng.choice(pharma_names) for name in partner_names}

    statuses = {row["metadata"]["id"]: record_status(row) for row in rows}
    active_ids = [row_id for row_id, status in statuses.items() if status[0] == "Active"]
    unpublished_ids = set(rng.sample(active_ids, UNPUBLISHED_RECORD_COUNT))

    # A minority of automatic archives are for records that stopped being updated.
    automatic_ids = [row_id for row_id, status in statuses.items()
                     if status[1] == "ArchivedAutomatically"]
    for row_id in rng.sample(automatic_ids, NOT_UPDATED_ARCHIVE_COUNT):
        statuses[row_id] = ("Archived", "ArchivedAutomatically", NOT_UPDATED_ARCHIVE_NOTE)

    builder = RecordBuilder(partner_names)
    records = []
    for row in rows:
        row_id = row["metadata"]["id"]
        organisation_name = row["company_name"] if row["company_name"] in pharma_names \
            else reassigned[row["company_name"]]
        unpublished = row_id in unpublished_ids
        status = ("Unpublished", None, None) if unpublished else statuses[row_id]
        records.append(builder.build(row, organisation_name, status, unpublished))

    organisations = []
    for name in pharma_names:
        owned = [record for record in records if record["organisationName"] == name]
        first = min(date.fromisoformat(record["createdAt"]) for record in owned)
        last = max(date.fromisoformat(record["lastUpdatedAt"]) for record in owned)
        organisations.append(organisation(
            rng, name, "PharmaCompany", weighted(rng, {"Medicines": 60, "Both": 40}),
            clamp(first - days(rng, 5, 60), date(2023, 11, 1), first),
            min(TODAY, last + days(rng, 0, 10)),
        ))
    for names, organisation_type in (
        (ref.HORIZON_SCANNING_ORGANISATIONS, "HorizonScanning"),
        (ref.STRATEGIC_ORGANISATIONS, "Strategic"),
        (ref.INTERNAL_ORGANISATIONS, "Internal"),
    ):
        for name in names:
            created = date(2023, 11, 1) + days(rng, 0, 600)
            organisations.append(organisation(
                rng, name, organisation_type, "Both", created, TODAY - days(rng, 0, 30)))

    output = {
        "asOf": TODAY.isoformat(),
        "referenceData": ref.REFERENCE_DATA,
        "organisations": organisations,
        "records": records,
    }
    write_json(target, output)

    print(f"Wrote {len(organisations)} organisations and {len(records)} records to {target}")


if __name__ == "__main__":
    main()
