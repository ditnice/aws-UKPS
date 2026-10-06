"""Checks synthetic-medicine-records.json against the model's enums, the reference data and the
conditional and chronological rules the generator is meant to follow.

Usage: python3 validate_synthetic_data.py [file]

By default this checks the seeder's embedded data file.
"""

import calendar
import collections
import glob
import gzip
import json
import os
import re
import sys
from datetime import date, timedelta
from pathlib import Path

sys.dont_write_bytecode = True

import synthetic_reference_data as ref  # noqa: E402
from transform_synthetic_data import (  # noqa: E402
    AUTO_ARCHIVE_NOTE,
    NOT_UPDATED_ARCHIVE_DAYS,
    NOT_UPDATED_ARCHIVE_NOTE,
)

REPOSITORY_ROOT = Path(__file__).resolve().parents[3]
ENUM_DIR = REPOSITORY_ROOT / "backend/src/Persistence/Enums"
DEFAULT_FILE = (
    REPOSITORY_ROOT
    / "backend/src/Persistence/Data/Seeding/SyntheticData/synthetic-medicine-records.json.gz"
)

ENUM_FIELDS = {
    "recordType": "RecordType",
    "recordStatus": "RecordStatus",
    "reason": "RecordStatusChangeReason",
    "indicationIsPaediatric": "IndicationPaediatricStatus",
    "medicineTechnologyStatus": "MedicineTechnologyStatus",
    "trialPhase": "TrialPhase",
    "euOrphanStatus": "DesignationStatus",
    "euAtmpClassificationStatus": "DesignationStatus",
    "pimDesignationStatus": "DesignationStatus",
    "eamsOpinionDecision": "EamsOpinionDecision",
    "medicineHtaBodies": "MedicineHtaAssessor",
    "datePrecision": "DatePrecision",
    "biomarkerType": "BiomarkerType",
    "genomicTestNgtdRelationship": "GenomicTestNgtdRelationship",
    "genomicTestMandatoryStatus": "GenomicTestMandatoryStatus",
    "nhsServiceChangesRequired": "NhsServiceChangesRequired",
    "patientAccessSchemeRegions": "PatientAccessSchemeRegion",
    "netUkBudgetImpactBand": "NetUkBudgetImpactBand",
    "organisationType": "OrganisationType",
    "allowedPharmaceuticalEntity": "PharmaceuticalEntity",
    "status": "UserOrgStatus",
}
YES_NO_UNKNOWN_FIELDS = {
    "indicationIsCancer", "indicationIsRareDisease", "isPersonalisedMedicine",
    "isRepurposedMedicine", "isOriginatorCompany", "isCoMarketed", "recruitingInUk",
    "ukConditionalApprovalAnticipated", "intlConditionalApprovalAnticipated",
    "medicineHtaSubmissionIntended", "htaNiceAlignedPathway", "willSubmitToEams",
    "diagnosticTestRequired", "screeningRequired", "urgentIdentificationRequired",
    "handlingStorageRequirements", "compassionateAccessAvailable", "patientAccessSchemePlanned",
    "indicationSpecificPricingPlanned",
}

REFERENCE_FIELDS = {
    "formulationType": "formulationTypes",
    "mhraProcedureType": "mhraProcedureTypes",
    "irpReferenceRegulator": "irpReferenceRegulators",
    "irpRoute": "irpRoutes",
    "atmpClassification": "atmpClassifications",
    "patientPathwayPoint": "patientPathwayPoints",
    "ukPatientPopulationRange": "ukPatientPopulationRanges",
}


def load_enums():
    enums = {}
    for path in glob.glob(f"{ENUM_DIR}/*.cs"):
        text = open(path, encoding="utf-8").read()
        enums[os.path.basename(path)[:-3]] = set(re.findall(r"^\s+([A-Z]\w*)\s*(?:=|,)", text, re.M))
    return enums


def period(value):
    """The first and last day a regulatory date could refer to."""
    start = date.fromisoformat(value["dateValue"])
    if value["datePrecision"] == "ActualDate":
        return start, start
    months = 3 if value["datePrecision"] == "EstimatedQuarter" else 1
    end_month = start.month + months - 1
    end = date(start.year, end_month, calendar.monthrange(start.year, end_month)[1])
    return start, end


def not_after(earlier, later):
    """Whether one regulatory date can fall on or before another, allowing for estimates."""
    if earlier is None or later is None:
        return True
    return period(earlier)[0] <= period(later)[1]


def main():
    path = Path(sys.argv[1]) if len(sys.argv) > 1 else DEFAULT_FILE
    opener = gzip.open if path.suffix == ".gz" else open
    with opener(path, "rt", encoding="utf-8") as file:
        data = json.load(file)
    enums = load_enums()
    for field in YES_NO_UNKNOWN_FIELDS:
        ENUM_FIELDS[field] = "YesNoUnknown"

    reference = data["referenceData"]
    reference_labels = {key: {row["label"] for row in rows} for key, rows in reference.items()}
    reference_codes = {
        key: {row["code"]: row["label"] for row in reference[key]}
        for key in ("bnfChapters", "therapeuticAreas")
    }
    errors = collections.defaultdict(list)

    def error(rule, record):
        errors[rule].append(record.get("sourceId", record.get("organisationName")))

    def check_values(node, record):
        if isinstance(node, dict):
            for key, value in node.items():
                if key in ENUM_FIELDS and value is not None:
                    for item in value if isinstance(value, list) else [value]:
                        if item not in enums[ENUM_FIELDS[key]]:
                            error(f"invalid {ENUM_FIELDS[key]} value {item!r} for {key}", record)
                if key in REFERENCE_FIELDS and value is not None:
                    if value not in reference_labels[REFERENCE_FIELDS[key]]:
                        error(f"unknown reference data {value!r} for {key}", record)
                check_values(value, record)
        elif isinstance(node, list):
            for item in node:
                check_values(item, record)

    def regulatory_dates(node):
        if isinstance(node, dict):
            if "datePrecision" in node:
                yield node
            for value in node.values():
                yield from regulatory_dates(value)

    organisations = {row["organisationName"]: row for row in data["organisations"]}
    for row in data["organisations"]:
        check_values(row, row)
    company_codes = collections.Counter()

    for record in data["records"]:
        check_values(record, record)
        names = record["productRecordDetails"]["namesAndIdentifiers"]
        indication = record["indicationAndDevelopmentInformation"]["indicationDetails"]
        background = record["indicationAndDevelopmentInformation"]["developmentBackground"]
        trials = record["clinicalTrialInformation"]
        regulatory = record["regulatoryAccessAndLaunchInformation"]
        mhra = regulatory["mhraProcedureAndDates"]
        hta = regulatory["healthTechnologyAssessmentAndLaunch"]
        designations = regulatory["specialDesignations"]
        service = record["serviceReadinessInformation"]
        lab = service["laboratoryTestingDetails"]
        patient = service["patientAndClinicalRequirements"]
        pricing = service["pricingAndBudgetImpact"]
        status = record["recordStatus"]

        # Record lifecycle.
        created = date.fromisoformat(record["createdAt"])
        updated = date.fromisoformat(record["lastUpdatedAt"])
        today = date.fromisoformat(data["asOf"])
        if not created <= updated <= today:
            error("createdAt <= lastUpdatedAt <= today", record)
        if status == "Unpublished":
            if record["reviewedAt"] is not None:
                error("unpublished records are never reviewed", record)
        elif not updated <= date.fromisoformat(record["reviewedAt"]) <= today:
            error("lastUpdatedAt <= reviewedAt <= today", record)
        change = record["statusChange"]
        if (change is not None) != (status in ("OnHold", "Archived")):
            error("status change only for on hold and archived records", record)
        if change is not None:
            changed = date.fromisoformat(change["changedAt"])
            reviewed = date.fromisoformat(record["reviewedAt"])
            if change["reason"] == "ArchivedAutomatically":
                if not reviewed < changed <= today:
                    error("automatic archives happen after the last review", record)
            elif not changed == updated == reviewed:
                error("manual status changes are the latest update and review", record)
        if status == "Active" and date.fromisoformat(record["reviewedAt"]) <= today - timedelta(
                days=NOT_UPDATED_ARCHIVE_DAYS):
            error("active records were reviewed within the archiving period", record)
        if record["organisationName"] not in organisations:
            error("organisation exists", record)
        elif date.fromisoformat(organisations[record["organisationName"]]["createdAt"]) > created:
            error("organisation created before its records", record)

        # Names.
        company_codes[names["companyCode"]] += 1
        if not names["companyCode"] or not names["recordTitle"]:
            error("company code and record title are required", record)

        # Indication.
        bnf = indication["bnfChapter"]
        if bnf is None or reference_codes["bnfChapters"].get(bnf["code"]) != bnf["label"]:
            error("BNF chapter matches reference data", record)
        for area in indication["therapeuticAreas"]:
            if reference_codes["therapeuticAreas"].get(area["code"]) != area["label"]:
                error("therapeutic area matches reference data", record)
        is_cancer = indication["indicationIsCancer"] == "Yes"
        in_chapter_8 = bnf is not None and bnf["code"].startswith("8")
        # Chapter 8.2 also covers immunosuppressants for non-cancer indications.
        if is_cancer != in_chapter_8 and not (in_chapter_8 and bnf["code"] == "8.2"):
            error("cancer indications use BNF chapter 8", record)
        if is_cancer != indication["therapeuticAreas"][0]["code"].startswith("14."):
            error("cancer indications use the oncology therapeutic area", record)
        if not 1 <= len(indication["therapeuticAreas"]) <= 3:
            error("1 to 3 therapeutic areas", record)
        if indication["isPersonalisedMedicine"] == "Yes" and lab["diagnosticTestRequired"] != "Yes" \
                and status != "Unpublished":
            error("personalised medicines need a diagnostic test", record)

        mode_kinds = {mode: kind for category in ref.CATEGORIES for mode, kind in category.modes}
        mode_kinds.update({mode: kind for modes in ref.AREA_MODES.values() for mode, kind in modes})
        oral = {"Tablet", "Capsule", "Enteric coated tablet", "Dispersible tablet", "Oral solution",
                "Oral suspension", "Powder for oral solution", "Granules"}
        if mode_kinds.get(indication["modeOfAction"]) in ("ab", "bio") and \
                indication["formulationType"] in oral:
            error("antibodies and injectable biologics are not oral", record)

        # Development background.
        if background["repurposedMedicineDetails"] and background["isRepurposedMedicine"] != "Yes":
            error("repurposed details only when repurposed", record)
        if (background["originatorCompanyName"] is not None) != (
                background["isOriginatorCompany"] == "No"):
            error("originator company name only when not the originator", record)
        if (background["coMarketingCompanyName"] is not None) != (
                background["isCoMarketed"] == "Yes"):
            error("co-marketing company only when co-marketed", record)

        # Clinical trials.
        if (trials["recruitingInUk"] is None) != (not trials["clinicalTrials"]):
            error("recruiting in the UK answered only when there are trials", record)
        if any(trial["trialPhase"] or trial["briefDescription"]
               for trial in trials["clinicalTrials"]):
            error("trial phase and description are vaccine-only", record)
        if status == "OnHold" and record["statusChange"]["reason"] == "TrialSuspended" \
                and not trials["clinicalTrials"]:
            error("suspended trial exists", record)

        # MHRA procedure and dates.
        is_irp = mhra["mhraProcedureType"] == "International Recognition Procedure"
        irp_fields = ("irpReferenceRegulator", "irpRoute", "intlLicenceDate")
        if is_irp != all(mhra[field] is not None for field in irp_fields):
            error("IRP fields only and always for the IRP", record)
        if not is_irp and any(mhra[field] is not None for field in (
                "irpReferenceRegulator", "irpRoute", "intlSubmissionDate", "intlLicenceDate",
                "intlConditionalApprovalAnticipated")):
            error("no IRP fields outside the IRP", record)
        if mhra["mhraProcedureType"].startswith("Unknown") and not mhra["procedureDetails"]:
            error("unknown procedure needs details", record)
        if mhra["globalSubmissionActualDate"] and (
                mhra["globalSubmissionActualDate"]["datePrecision"] != "ActualDate"
                or not mhra["globalFirstSubmissionRegion"]):
            error("global first submission date is actual and has a region", record)
        for earlier, later in (
            (mhra["ukSubmissionDate"], mhra["ukLicenceDate"]),
            (mhra["ukLicenceDate"], hta["ukLaunchDate"]),
            (mhra["intlSubmissionDate"], mhra["intlLicenceDate"]),
            (mhra["intlLicenceDate"], mhra["ukSubmissionDate"]),
            (mhra["globalSubmissionActualDate"], mhra["ukSubmissionDate"]),
            (designations["eamsSubmissionDate"], designations["eamsOpinionDate"]),
            (designations["eamsOpinionDate"], mhra["ukLicenceDate"]),
            (designations["euOrphanGrantedDate"], mhra["ukSubmissionDate"]),
        ):
            if not not_after(earlier, later):
                error("regulatory dates in chronological order", record)

        for value in regulatory_dates(regulatory):
            start, end = period(value)
            if value["datePrecision"] == "ActualDate" and start > updated:
                error("actual dates are on or before the last update", record)
            if value["datePrecision"] != "ActualDate" and end < updated:
                error("estimated dates are after the last update", record)

        launch = hta["ukLaunchDate"]
        launched_long_ago = launch is not None and launch["datePrecision"] == "ActualDate" and \
            date.fromisoformat(launch["dateValue"]) <= updated - timedelta(days=183)
        auto_archived = change is not None and change["reason"] == "ArchivedAutomatically"
        available_in_uk = auto_archived and change["note"] == AUTO_ARCHIVE_NOTE
        not_updated = auto_archived and change["note"] == NOT_UPDATED_ARCHIVE_NOTE
        if auto_archived and status != "Archived":
            error("only archived records are archived automatically", record)
        if auto_archived and not (available_in_uk or not_updated):
            error("automatic archives explain why in their note", record)
        if available_in_uk and not (
                launch is not None and launch["datePrecision"] == "ActualDate"
                and date.fromisoformat(launch["dateValue"])
                <= date.fromisoformat(change["changedAt"]) - timedelta(days=183)):
            error("products archived for UK availability launched 6 months before", record)
        if not_updated and (date.fromisoformat(change["changedAt"])
                            - date.fromisoformat(record["reviewedAt"])).days \
                < NOT_UPDATED_ARCHIVE_DAYS:
            error("records archived for not being updated were left for the full period", record)
        if status != "Archived" and launched_long_ago:
            error("products launched over 6 months ago are archived", record)
        if status == "Archived" and not available_in_uk and mhra["ukLicenceDate"] and \
                mhra["ukLicenceDate"]["datePrecision"] == "ActualDate":
            error("only products archived for UK availability were licensed", record)

        # HTA.
        bodies = hta["medicineHtaBodies"] or []
        if bool(bodies) != (hta["medicineHtaSubmissionIntended"] == "Yes"):
            error("HTA bodies only and always when a submission is intended", record)
        if (hta["niceTaDevelopmentId"] or hta["htaNiceAlignedPathway"]) and "Nice" not in bodies:
            error("NICE fields only when submitting to NICE", record)

        # Special designations (all empty for some drafts).
        if any(value is not None for value in designations.values()):
            orphan_granted = designations["euOrphanStatus"] == "Granted"
            if orphan_granted != (designations["euOrphanGrantedDate"] is not None) or \
                    orphan_granted != (designations["euOrphanStatusNumber"] is not None):
                error("orphan date and number only and always when granted", record)
            atmp = designations["euAtmpClassificationStatus"]
            if (designations["atmpClassification"] is not None) != (atmp == "Granted"):
                error("ATMP classification only and always when granted", record)
            if (designations["atmpRecommendationDate"] is not None) != (
                    atmp in ("Granted", "NotGranted")):
                error("ATMP recommendation date only after a recommendation", record)
            eams = designations["willSubmitToEams"] == "Yes"
            if eams and designations["pimDesignationStatus"] != "Granted":
                error("EAMS requires a PIM designation", record)
            if eams != (designations["eamsSubmissionDate"] is not None):
                error("EAMS dates only and always when submitting to EAMS", record)
            decision = designations["eamsOpinionDecision"]
            opinion = designations["eamsOpinionDate"]
            if decision and (opinion is None or opinion["datePrecision"] != "ActualDate"):
                error("EAMS decision only after the opinion", record)

        # Laboratory testing.
        biomarker = lab["biomarkerType"]
        if biomarker and lab["diagnosticTestRequired"] != "Yes":
            error("biomarker type only when a test is required", record)
        genomic_fields = ("genomicTarget", "genomicTestNgtdRelationship", "genomicSampleType",
                          "genomicAlterations", "genomicTestMandatoryStatus")
        if (biomarker == "GenomicBiomarker") != all(lab[field] for field in genomic_fields):
            error("genomic fields only and always for genomic biomarkers", record)
        if (lab["nonGenomicBiomarkerDescription"] is not None) != (
                biomarker == "NonGenomicBiomarker"):
            error("non-genomic description only for non-genomic biomarkers", record)
        if (lab["genomicTestPathwayPointOther"] is not None) != (
                lab["patientPathwayPoint"] == "Other"):
            error("pathway point details only for Other", record)

        # Patient and clinical requirements, and pricing.
        draft_empty = all(value in (None, []) for value in patient.values())
        if not draft_empty and not patient["proposedPlaceInTherapy"]:
            error("proposed place in therapy is required", record)
        for answer, details, trigger in (
            (patient["screeningRequired"], patient["screeningDetails"], {"Yes"}),
            (patient["urgentIdentificationRequired"], patient["urgentIdentificationDetails"],
             {"Yes"}),
            (patient["nhsServiceChangesRequired"], patient["nhsServiceChangesDetails"],
             {"SomeChange", "CompleteTransformation"}),
            (patient["handlingStorageRequirements"], patient["handlingStorageDetails"], {"Yes"}),
            (pricing["compassionateAccessAvailable"], pricing["compassionateAccessDetails"],
             {"Yes"}),
            (pricing["patientAccessSchemePlanned"], pricing["patientAccessSchemeRegions"],
             {"Yes"}),
            (pricing["indicationSpecificPricingPlanned"],
             pricing["indicationSpecificPricingDetails"], {"Yes"}),
        ):
            if (details not in (None, [])) != (answer in trigger):
                error("follow-up details only and always for the triggering answer", record)

    duplicates = [code for code, count in company_codes.items() if count > 1]
    if duplicates:
        errors["company codes are unique"] = duplicates

    if errors:
        for rule, ids in sorted(errors.items()):
            print(f"FAIL {rule}: {len(ids)} (e.g. {ids[:5]})")
        sys.exit(1)
    print(f"All checks passed for {len(data['records'])} records and "
          f"{len(data['organisations'])} organisations.")


if __name__ == "__main__":
    main()
