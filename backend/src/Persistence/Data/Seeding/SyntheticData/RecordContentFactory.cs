using System.Collections;
using System.Globalization;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;
using static UKPS.Api.Persistence.Data.Seeding.SyntheticData.SyntheticRecord;

namespace UKPS.Api.Persistence.Data.Seeding.SyntheticData;

/// <summary>Creates the content entities that make up one revision of a synthetic medicine record.</summary>
internal sealed class RecordContentFactory
{
    private readonly SeedReferenceData _referenceData;

    public RecordContentFactory(SeedReferenceData referenceData)
    {
        _referenceData = referenceData;
    }

    public IEnumerable<object> Create(SyntheticRecord record, RecordRevision revision) =>
        [
            ProductDetail(record.ProductRecordDetails.NamesAndIdentifiers, revision),
            .. IndicationAndDevelopment(record.IndicationAndDevelopmentInformation, revision),
            .. ClinicalTrials(record.ClinicalTrialInformation, revision),
            .. RegulatoryAccessAndLaunch(record.RegulatoryAccessAndLaunchInformation, revision),
            .. ServiceReadiness(record.ServiceReadinessInformation, revision),
        ];

    private static RecordProductDetail ProductDetail(
        SyntheticNamesAndIdentifiers names,
        RecordRevision revision
    ) =>
        new()
        {
            Revision = revision,
            CompanyCode = names.CompanyCode,
            BrandedName = names.BrandedName,
            RecordTitle = names.RecordTitle,
            NamesAndIdentifiers =
            [
                .. names.GenericNames.Select(
                    (name, index) =>
                        NameAndIdentifier(name, NameAndIdentifierType.GenericName, index)
                ),
                .. names.OtherIdentifiers.Select(
                    (name, index) =>
                        NameAndIdentifier(name, NameAndIdentifierType.OtherIdentifier, index)
                ),
            ],
        };

    private IEnumerable<object> IndicationAndDevelopment(
        SyntheticIndicationAndDevelopment section,
        RecordRevision revision
    )
    {
        yield return IndicationDetail(section.IndicationDetails, revision);
        if (HasAnswers(section.DevelopmentBackground))
        {
            yield return DevelopmentBackground(section.DevelopmentBackground, revision);
        }
    }

    private static IEnumerable<object> ClinicalTrials(
        SyntheticClinicalTrialInformation section,
        RecordRevision revision
    )
    {
        if (section.RecruitingInUk is not null)
        {
            yield return new RecordClinicalTrialInformation
            {
                Revision = revision,
                RecruitingInUk = section.RecruitingInUk,
            };
        }
        foreach (SyntheticClinicalTrial trial in section.ClinicalTrials)
        {
            yield return ClinicalTrial(trial, revision);
        }
    }

    private IEnumerable<object> RegulatoryAccessAndLaunch(
        SyntheticRegulatoryAccessAndLaunch section,
        RecordRevision revision
    )
    {
        foreach (object entity in MhraProcedureAndDates(section.MhraProcedureAndDates, revision))
        {
            yield return entity;
        }
        yield return Hta(section.HealthTechnologyAssessmentAndLaunch, revision);
        if (HasAnswers(section.SpecialDesignations))
        {
            yield return EuStatus(section.SpecialDesignations, revision);
            yield return EamsPim(section.SpecialDesignations, revision);
        }
    }

    private IEnumerable<object> ServiceReadiness(
        SyntheticServiceReadiness section,
        RecordRevision revision
    )
    {
        if (HasAnswers(section.LaboratoryTestingDetails))
        {
            yield return LaboratoryTesting(section.LaboratoryTestingDetails, revision);
        }
        SyntheticPatientAndClinicalRequirements patient = section.PatientAndClinicalRequirements;
        if (HasAnswers(patient))
        {
            yield return new MedicinesPatientIdentification
            {
                Revision = revision,
                ScreeningRequired = patient.ScreeningRequired,
                ScreeningDetails = patient.ScreeningDetails,
                UrgentIdentificationRequired = patient.UrgentIdentificationRequired,
                UrgentIdentificationDetails = patient.UrgentIdentificationDetails,
            };
            yield return ServiceImpact(patient, revision);
        }
        if (patient.ProposedPlaceInTherapy is not null)
        {
            yield return new MedicinesTreatmentDetail
            {
                Revision = revision,
                ProposedPlaceInTherapy = patient.ProposedPlaceInTherapy,
                EstimatedDurationOfTreatment = patient.EstimatedDurationOfTreatment,
            };
        }
        if (HasAnswers(section.PricingAndBudgetImpact))
        {
            yield return BudgetImpact(section.PricingAndBudgetImpact, revision);
        }
    }

    private static RecordNameAndIdentifier NameAndIdentifier(
        string name,
        NameAndIdentifierType nameType,
        int index
    ) =>
        new()
        {
            Name = name,
            NameType = nameType,
            DisplayOrder = index + 1,
        };

    private MedicinesIndicationDetail IndicationDetail(
        SyntheticIndicationDetails details,
        RecordRevision revision
    )
    {
        MedicinesIndicationDetail indicationDetail = new()
        {
            Revision = revision,
            Indication = details.Indication,
            BnfChapter = SeedReferenceData.Find(
                _referenceData.BnfChapters,
                details.BnfChapter?.Code
            ),
            IndicationIsPaediatric = details.IndicationIsPaediatric,
            IndicationIsCancer = details.IndicationIsCancer,
            IndicationIsRareDisease = details.IndicationIsRareDisease,
            FormulationType = SeedReferenceData.Find(
                _referenceData.FormulationTypes,
                details.FormulationType
            ),
            Presentation = details.Presentation,
            ModeOfAction = details.ModeOfAction,
            ProposedDoseRegimen = details.ProposedDoseRegimen,
            IsPersonalisedMedicine = details.IsPersonalisedMedicine,
            MedicineTechnologyStatus = CombineFlags(details.MedicineTechnologyStatus),
        };
        indicationDetail.TherapeuticAreas =
        [
            .. details.TherapeuticAreas.Select(area => new MedicinesIndicationDetailTherapeuticArea
            {
                MedicinesIndicationDetail = indicationDetail,
                TherapeuticArea = SeedReferenceData.Find(
                    _referenceData.TherapeuticAreas,
                    area.Code
                ),
            }),
        ];
        return indicationDetail;
    }

    private static MedicinesDevelopmentBackground DevelopmentBackground(
        SyntheticDevelopmentBackground background,
        RecordRevision revision
    ) =>
        new()
        {
            Revision = revision,
            IsRepurposedMedicine = background.IsRepurposedMedicine,
            RepurposedMedicineDetails = background.RepurposedMedicineDetails,
            IsOriginatorCompany = background.IsOriginatorCompany,
            OriginatorCompanyName = background.OriginatorCompanyName,
            IsCoMarketed = background.IsCoMarketed,
            CoMarketingCompanyName = background.CoMarketingCompanyName,
        };

    private static RecordClinicalTrial ClinicalTrial(
        SyntheticClinicalTrial trial,
        RecordRevision revision
    ) =>
        new()
        {
            Revision = revision,
            StudyName = trial.StudyName,
            ClinicalTrialsGovNumber = trial.ClinicalTrialsGovNumber,
            TrialPhase = trial.TrialPhase,
            BriefDescription = trial.BriefDescription,
            OtherClinicalTrialNumbers =
            [
                .. trial.OtherClinicalTrialNumbers.Select(
                    (number, index) =>
                        new OtherClinicalTrialNumber
                        {
                            OtherRegistryNumber = number,
                            DisplayOrder = index + 1,
                        }
                ),
            ],
        };

    private IEnumerable<object> MhraProcedureAndDates(
        SyntheticMhraProcedureAndDates mhra,
        RecordRevision revision
    )
    {
        yield return new RecordMhraProcedure
        {
            Revision = revision,
            MhraProcedureType = SeedReferenceData.Find(
                _referenceData.MhraProcedureTypes,
                mhra.MhraProcedureType
            ),
            IrpReferenceRegulator = SeedReferenceData.Find(
                _referenceData.IrpReferenceRegulators,
                mhra.IrpReferenceRegulator
            ),
            ProcedureDetails = mhra.ProcedureDetails,
        };
        yield return new RecordMhraDate
        {
            Revision = revision,
            UkSubmissionDate = RegulatoryDate(
                mhra.UkSubmissionDate,
                DateEventType.UkSubmission,
                revision
            ),
            UkLicenceDate = RegulatoryDate(mhra.UkLicenceDate, DateEventType.UkLicence, revision),
            UkConditionalApprovalAnticipated = mhra.UkConditionalApprovalAnticipated,
        };
        if (mhra.GlobalFirstSubmissionRegion is not null)
        {
            yield return new MedicinesGlobalSubmission
            {
                Revision = revision,
                GlobalFirstSubmissionRegion = mhra.GlobalFirstSubmissionRegion,
                GlobalSubmissionActualDate = RegulatoryDate(
                    mhra.GlobalSubmissionActualDate,
                    DateEventType.GlobalFirstSubmission,
                    revision
                ),
            };
        }
        if (mhra.IrpRoute is not null)
        {
            yield return new MedicinesIntlRecognition
            {
                Revision = revision,
                IrpRoute = SeedReferenceData.Find(_referenceData.IrpRoutes, mhra.IrpRoute),
                IntlSubmissionDate = RegulatoryDate(
                    mhra.IntlSubmissionDate,
                    DateEventType.IntlSubmission,
                    revision
                ),
                IntlLicenceDate = RegulatoryDate(
                    mhra.IntlLicenceDate,
                    DateEventType.IntlLicence,
                    revision
                ),
                IntlConditionalApprovalAnticipated = mhra.IntlConditionalApprovalAnticipated,
            };
        }
    }

    private static RecordHta Hta(SyntheticHtaAndLaunch hta, RecordRevision revision) =>
        new()
        {
            Revision = revision,
            MedicineHtaSubmissionIntended = hta.MedicineHtaSubmissionIntended,
            MedicineHtaBodies = CombineFlags(hta.MedicineHtaBodies),
            HtaAdditionalDetails = hta.HtaAdditionalDetails,
            HtaNiceAlignedPathway = hta.HtaNiceAlignedPathway,
            NiceTaDevelopmentId = hta.NiceTaDevelopmentId,
            UkLaunchDate = RegulatoryDate(hta.UkLaunchDate, DateEventType.UkLaunch, revision),
        };

    private MedicinesEuStatus EuStatus(
        SyntheticSpecialDesignations designations,
        RecordRevision revision
    ) =>
        new()
        {
            Revision = revision,
            EuOrphanStatus = designations.EuOrphanStatus,
            EuOrphanStatusNumber = designations.EuOrphanStatusNumber,
            EuOrphanGrantedDate = RegulatoryDate(
                designations.EuOrphanGrantedDate,
                DateEventType.EuOrphanGranted,
                revision
            ),
            EuAtmpClassificationStatus = designations.EuAtmpClassificationStatus,
            AtmpRecommendationDate = RegulatoryDate(
                designations.AtmpRecommendationDate,
                DateEventType.AtmpClassificationRecommendation,
                revision
            ),
            AtmpClassification = SeedReferenceData.Find(
                _referenceData.AtmpClassifications,
                designations.AtmpClassification
            ),
        };

    private static MedicinesEamsPim EamsPim(
        SyntheticSpecialDesignations designations,
        RecordRevision revision
    ) =>
        new()
        {
            Revision = revision,
            PimDesignationStatus = designations.PimDesignationStatus,
            WillSubmitToEams = designations.WillSubmitToEams,
            EamsSubmissionDate = RegulatoryDate(
                designations.EamsSubmissionDate,
                DateEventType.EamsSubmission,
                revision
            ),
            EamsOpinionDate = RegulatoryDate(
                designations.EamsOpinionDate,
                DateEventType.EamsOpinion,
                revision
            ),
            EamsOpinionDecision = designations.EamsOpinionDecision,
        };

    private MedicinesLaboratoryTesting LaboratoryTesting(
        SyntheticLaboratoryTesting lab,
        RecordRevision revision
    ) =>
        new()
        {
            Revision = revision,
            DiagnosticTestRequired = lab.DiagnosticTestRequired,
            BiomarkerType = lab.BiomarkerType,
            NonGenomicBiomarkerDescription = lab.NonGenomicBiomarkerDescription,
            GenomicTarget = lab.GenomicTarget,
            GenomicTestNgtdRelationship = lab.GenomicTestNgtdRelationship,
            GenomicSampleType = lab.GenomicSampleType,
            GenomicTurnaroundTimeDetails = lab.GenomicTurnaroundTimeDetails,
            PatientPathwayPoint = SeedReferenceData.Find(
                _referenceData.PatientPathwayPoints,
                lab.PatientPathwayPoint
            ),
            GenomicTestPathwayPointOther = lab.GenomicTestPathwayPointOther,
            GenomicAlterations = lab.GenomicAlterations,
            AdditionalGenomicFactors = lab.AdditionalGenomicFactors,
            GenomicTestUsedInTrials = lab.GenomicTestUsedInTrials,
            GenomicTestSpecificitySensitivity = lab.GenomicTestSpecificitySensitivity,
            GenomicTestMandatoryStatus = lab.GenomicTestMandatoryStatus,
            GenomicTestNotes = lab.GenomicTestNotes,
            MonitoringTestsDetails = lab.MonitoringTestsDetails,
            SafetyTestsDetails = lab.SafetyTestsDetails,
        };

    private MedicinesServiceImpact ServiceImpact(
        SyntheticPatientAndClinicalRequirements patient,
        RecordRevision revision
    ) =>
        new()
        {
            Revision = revision,
            NhsServiceChangesRequired = patient.NhsServiceChangesRequired,
            NhsServiceChangesDetails = patient.NhsServiceChangesDetails,
            HandlingStorageRequirements = patient.HandlingStorageRequirements,
            HandlingStorageDetails = patient.HandlingStorageDetails,
            UkPatientPopulationRange = SeedReferenceData.Find(
                _referenceData.UkPatientPopulationRanges,
                patient.UkPatientPopulationRange
            ),
            UkPatientPopulationNotes = patient.UkPatientPopulationNotes,
            EstimatedEligiblePatientPopulation = patient.EstimatedEligiblePatientPopulation,
        };

    private static MedicinesBudgetImpact BudgetImpact(
        SyntheticPricingAndBudgetImpact pricing,
        RecordRevision revision
    ) =>
        new()
        {
            Revision = revision,
            EstimatedUptake = pricing.EstimatedUptake,
            CompassionateAccessAvailable = pricing.CompassionateAccessAvailable,
            CompassionateAccessDetails = pricing.CompassionateAccessDetails,
            PatientAccessSchemePlanned = pricing.PatientAccessSchemePlanned,
            PatientAccessSchemeRegions = CombineFlags(pricing.PatientAccessSchemeRegions),
            IndicationSpecificPricingPlanned = pricing.IndicationSpecificPricingPlanned,
            IndicationSpecificPricingDetails = pricing.IndicationSpecificPricingDetails,
            NetUkBudgetImpactBand = pricing.NetUkBudgetImpactBand,
        };

    private static RegulatoryDate? RegulatoryDate(
        SyntheticRegulatoryDate? date,
        DateEventType dateEvent,
        RecordRevision revision
    ) =>
        date is null
            ? null
            : new RegulatoryDate
            {
                Revision = revision,
                DateEvent = dateEvent,
                DatePrecision = date.DatePrecision,
                DateValue = date.DateValue,
                IsConfidential = date.IsConfidential,
            };

    private static T? CombineFlags<T>(IReadOnlyCollection<T>? values)
        where T : struct, Enum
    {
        if (values is null || values.Count == 0)
        {
            return null;
        }

        int combined = values.Aggregate(
            0,
            (flags, value) => flags | Convert.ToInt32(value, CultureInfo.InvariantCulture)
        );
        return (T)Enum.ToObject(typeof(T), combined);
    }

    /// <summary>
    /// Whether any question in a section has been answered. Unpublished drafts leave some
    /// optional sections empty, and those are not saved.
    /// </summary>
    private static bool HasAnswers(object section) =>
        section
            .GetType()
            .GetProperties()
            .Select(property => property.GetValue(section))
            .Any(value =>
                value is not null && (value is not ICollection collection || collection.Count > 0)
            );
}
