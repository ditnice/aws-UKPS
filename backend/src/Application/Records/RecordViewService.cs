using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.Records.Dtos.PublishedRecord;
using UKPS.Api.Application.Records.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records;

// Type aliases for Result
using GetPublishedRecordResult = Result<PublishedRecordDto, GetPublishedRecordError>;

internal sealed class RecordViewService(
    AppDbContext dbContext,
    IOrganisationAuthoriser organisationAuthoriser
) : IRecordViewService
{
    public async Task<GetPublishedRecordResult> GetPublishedRecord(
        int recordId,
        RecordType recordType,
        CancellationToken cancellationToken
    )
    {
        RecordHeader? record = await GetRecordHeader(recordId, cancellationToken);

        if (record is null)
        {
            return GetPublishedRecordResult.Err(new GetPublishedRecordError.NotFound(recordId));
        }

        if (
            !organisationAuthoriser.CanPerformOperationOnOrganisation(
                Operation.Read,
                record.OrganisationId
            )
        )
        {
            return GetPublishedRecordResult.Err(new GetPublishedRecordError.NotAllowed(recordId));
        }

        if (
            record.RecordStatus is not (RecordStatus.Active or RecordStatus.OnHold)
            || record.PublishedRevisionId is null
        )
        {
            return GetPublishedRecordResult.Err(new GetPublishedRecordError.NotFound(recordId));
        }

        if (record.RecordType != recordType)
        {
            return GetPublishedRecordResult.Err(
                new GetPublishedRecordError.RecordTypeMismatch(
                    recordId,
                    recordType,
                    record.RecordType
                )
            );
        }

        int revisionId = record.PublishedRevisionId.Value;

        return GetPublishedRecordResult.Ok(
            recordType switch
            {
                RecordType.Medicine => await GetPublishedMedicineRecord(
                    recordId,
                    record,
                    revisionId,
                    cancellationToken
                ),
                RecordType.Vaccine => GetPublishedVaccineRecord(recordId, record, revisionId),
                _ => throw new UnreachableException($"Unhandled record type {recordType}."),
            }
        );
    }

    private Task<RecordHeader?> GetRecordHeader(
        int recordId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .Records.AsNoTracking()
            .Where(r => r.Id == recordId)
            .Select(r => new RecordHeader(
                r.OrganisationId,
                r.RecordType,
                r.RecordStatus,
                r.ReviewedAt,
                r.Revisions.Where(rev => rev.WorkflowStatus == WorkflowStatus.Published)
                    .OrderByDescending(rev => rev.CreatedAt)
                    .Select(rev => (int?)rev.Id)
                    .FirstOrDefault()
            ))
            .SingleOrDefaultAsync(cancellationToken);

    private async Task<PublishedMedicineRecordDto> GetPublishedMedicineRecord(
        int recordId,
        RecordHeader record,
        int revisionId,
        CancellationToken cancellationToken
    )
    {
        return new PublishedMedicineRecordDto
        {
            RecordId = recordId,
            OrganisationId = record.OrganisationId,
            RecordStatus = record.RecordStatus,
            ReviewedAt = record.ReviewedAt,
            RevisionId = revisionId,
            MedicinesProductDetail = await GetMedicinesProductDetail(revisionId, cancellationToken),
            MedicinesDetail = await GetMedicinesDetail(revisionId, cancellationToken),
            MedicinesCompanyInfo = await GetMedicinesCompanyInfo(revisionId, cancellationToken),
            MedicinesTreatmentDetail = await GetMedicinesTreatmentDetail(
                revisionId,
                cancellationToken
            ),
            MedicinesPatientIdentification = await GetMedicinesPatientIdentification(
                revisionId,
                cancellationToken
            ),
            MedicinesLaboratoryTesting = await GetMedicinesLaboratoryTesting(
                revisionId,
                cancellationToken
            ),
            MedicinesServiceImpact = await GetMedicinesServiceImpact(revisionId, cancellationToken),
            MedicinesBudgetImpact = await GetMedicinesBudgetImpact(revisionId, cancellationToken),
            MedicinesEamsPim = await GetMedicinesEamsPim(revisionId, cancellationToken),
            MedicinesEuStatus = await GetMedicinesEuStatus(revisionId, cancellationToken),
            MedicinesGlobalSubmission = await GetMedicinesGlobalSubmission(
                revisionId,
                cancellationToken
            ),
            MedicinesIntlRecognition = await GetMedicinesIntlRecognition(
                revisionId,
                cancellationToken
            ),
            RecordClinicalTrials = await GetRecordClinicalTrials(revisionId, cancellationToken),
            RecordHta = await GetRecordHta(revisionId, cancellationToken),
            RecordMhraDate = await GetRecordMhraDate(revisionId, cancellationToken),
            RecordMhraProcedure = await GetRecordMhraProcedure(revisionId, cancellationToken),
        };
    }

    // TODO: Populate the vaccine record sections.
    private static PublishedVaccineRecordDto GetPublishedVaccineRecord(
        int recordId,
        RecordHeader record,
        int revisionId
    )
    {
        return new PublishedVaccineRecordDto
        {
            RecordId = recordId,
            OrganisationId = record.OrganisationId,
            RecordStatus = record.RecordStatus,
            ReviewedAt = record.ReviewedAt,
            RevisionId = revisionId,
        };
    }

    private Task<MedicinesProductDetailDto?> GetMedicinesProductDetail(
        int revisionId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .MedicinesProductDetails.AsNoTracking()
            .Where(x => x.RevisionId == revisionId)
            .Select(x => new MedicinesProductDetailDto
            {
                RecordTitle = x.RecordTitle,
                BrandedName = x.BrandedName,
                Indication = x.Indication,
                IndicationIsPaediatric = x.IndicationIsPaediatric,
                IndicationIsCancer = x.IndicationIsCancer,
                IndicationIsRareDisease = x.IndicationIsRareDisease,
                NiceTaDevelopmentId = x.NiceTaDevelopmentId,
                BnfChapter =
                    x.BnfChapter == null
                        ? null
                        : new ReferenceDataDto { Id = x.BnfChapter.Id, Label = x.BnfChapter.Label },
                FormulationType = ToDto(x.FormulationType),
                Presentation = x.Presentation,
                MedicineTechnologyStatus = ToFlagList(x.MedicineTechnologyStatus),
                TherapeuticAreas = x
                    .TherapeuticAreas.OrderBy(t => t.TherapeuticArea!.DisplayOrder)
                    .ThenBy(t => t.TherapeuticArea!.Label)
                    .Select(t => new ReferenceDataDto
                    {
                        Id = t.TherapeuticAreaId,
                        Label = t.TherapeuticArea!.Label,
                    })
                    .ToList(),
                ActiveSubstances = x
                    .ActiveSubstances.OrderBy(s => s.DisplayOrder)
                    .ThenBy(s => s.Id)
                    .Select(s => new MedicinesActiveSubstanceDto
                    {
                        Name = s.Name,
                        NameType = s.NameType,
                    })
                    .ToList(),
            })
            .SingleOrDefaultAsync(cancellationToken);

    private Task<MedicinesDetailDto?> GetMedicinesDetail(
        int revisionId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .MedicinesDetails.AsNoTracking()
            .Where(x => x.RevisionId == revisionId)
            .Select(x => new MedicinesDetailDto
            {
                ModeOfAction = x.ModeOfAction,
                ProposedDoseRegimen = x.ProposedDoseRegimen,
                IsPersonalisedMedicine = x.IsPersonalisedMedicine,
                IsRepurposedMedicine = x.IsRepurposedMedicine,
                RepurposedMedicineDetails = x.RepurposedMedicineDetails,
            })
            .SingleOrDefaultAsync(cancellationToken);

    private Task<MedicinesCompanyInfoDto?> GetMedicinesCompanyInfo(
        int revisionId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .MedicinesCompanyInfos.AsNoTracking()
            .Where(x => x.RevisionId == revisionId)
            .Select(x => new MedicinesCompanyInfoDto
            {
                IsOriginatorCompany = x.IsOriginatorCompany,
                OriginatorCompanyName = x.OriginatorCompanyName,
                IsCoMarketed = x.IsCoMarketed,
                CoMarketingCompanyName = x.CoMarketingCompanyName,
            })
            .SingleOrDefaultAsync(cancellationToken);

    private Task<MedicinesTreatmentDetailDto?> GetMedicinesTreatmentDetail(
        int revisionId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .MedicinesTreatmentDetails.AsNoTracking()
            .Where(x => x.RevisionId == revisionId)
            .Select(x => new MedicinesTreatmentDetailDto
            {
                ProposedPlaceInTherapy = x.ProposedPlaceInTherapy,
                EstimatedDurationOfTreatment = x.EstimatedDurationOfTreatment,
            })
            .SingleOrDefaultAsync(cancellationToken);

    private Task<MedicinesPatientIdentificationDto?> GetMedicinesPatientIdentification(
        int revisionId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .MedicinesPatientIdentifications.AsNoTracking()
            .Where(x => x.RevisionId == revisionId)
            .Select(x => new MedicinesPatientIdentificationDto
            {
                ScreeningRequired = x.ScreeningRequired,
                ScreeningDetails = x.ScreeningDetails,
                UrgentIdentificationRequired = x.UrgentIdentificationRequired,
                UrgentIdentificationDetails = x.UrgentIdentificationDetails,
            })
            .SingleOrDefaultAsync(cancellationToken);

    private Task<MedicinesLaboratoryTestingDto?> GetMedicinesLaboratoryTesting(
        int revisionId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .MedicinesLaboratoryTestings.AsNoTracking()
            .Where(x => x.RevisionId == revisionId)
            .Select(x => new MedicinesLaboratoryTestingDto
            {
                DiagnosticTestRequired = x.DiagnosticTestRequired,
                BiomarkerType = x.BiomarkerType,
                NonGenomicBiomarkerDescription = x.NonGenomicBiomarkerDescription,
                GenomicTarget = x.GenomicTarget,
                GenomicTestNgtdRelationship = x.GenomicTestNgtdRelationship,
                GenomicSampleType = x.GenomicSampleType,
                GenomicTurnaroundTimeDetails = x.GenomicTurnaroundTimeDetails,
                PatientPathwayPoint = ToDto(x.PatientPathwayPoint),
                GenomicTestPathwayPointOther = x.GenomicTestPathwayPointOther,
                GenomicAlterations = x.GenomicAlterations,
                GenomicTestUsedInTrials = x.GenomicTestUsedInTrials,
                GenomicTestSpecificitySensitivity = x.GenomicTestSpecificitySensitivity,
                GenomicTestNotes = x.GenomicTestNotes,
                GenomicTestMandatoryStatus = x.GenomicTestMandatoryStatus,
                AdditionalGenomicFactors = x.AdditionalGenomicFactors,
                MonitoringTestsDetails = x.MonitoringTestsDetails,
                SafetyTestsDetails = x.SafetyTestsDetails,
            })
            .SingleOrDefaultAsync(cancellationToken);

    private Task<MedicinesServiceImpactDto?> GetMedicinesServiceImpact(
        int revisionId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .MedicinesServiceImpacts.AsNoTracking()
            .Where(x => x.RevisionId == revisionId)
            .Select(x => new MedicinesServiceImpactDto
            {
                NhsServiceChangesRequired = x.NhsServiceChangesRequired,
                NhsServiceChangesDetails = x.NhsServiceChangesDetails,
                HandlingStorageRequirements = x.HandlingStorageRequirements,
                HandlingStorageDetails = x.HandlingStorageDetails,
                EstimatedUptake = x.EstimatedUptake,
                UkPatientPopulationRange = ToDto(x.UkPatientPopulationRange),
                UkPatientPopulationNotes = x.UkPatientPopulationNotes,
                EstimatedEligiblePatientPopulation = x.EstimatedEligiblePatientPopulation,
                CompassionateAccessAvailable = x.CompassionateAccessAvailable,
                CompassionateAccessDetails = x.CompassionateAccessDetails,
            })
            .SingleOrDefaultAsync(cancellationToken);

    private Task<MedicinesBudgetImpactDto?> GetMedicinesBudgetImpact(
        int revisionId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .MedicinesBudgetImpacts.AsNoTracking()
            .Where(x => x.RevisionId == revisionId)
            .Select(x => new MedicinesBudgetImpactDto
            {
                PatientAccessSchemePlanned = x.PatientAccessSchemePlanned,
                IndicationSpecificPricingPlanned = x.IndicationSpecificPricingPlanned,
                IndicationSpecificPricingDetails = x.IndicationSpecificPricingDetails,
                NetUkBudgetImpactBand = x.NetUkBudgetImpactBand,
                PatientAccessSchemeRegions = ToFlagList(x.PatientAccessSchemeRegions),
            })
            .SingleOrDefaultAsync(cancellationToken);

    private Task<MedicinesEamsPimDto?> GetMedicinesEamsPim(
        int revisionId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .MedicinesEamsPims.AsNoTracking()
            .Where(x => x.RevisionId == revisionId)
            .Select(x => new MedicinesEamsPimDto
            {
                PimDesignationStatus = x.PimDesignationStatus,
                WillSubmitToEams = x.WillSubmitToEams,
                EamsOpinionDecision = x.EamsOpinionDecision,
                EamsSubmissionDate = ToDto(x.EamsSubmissionDate),
                EamsOpinionDate = ToDto(x.EamsOpinionDate),
            })
            .SingleOrDefaultAsync(cancellationToken);

    private Task<MedicinesEuStatusDto?> GetMedicinesEuStatus(
        int revisionId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .MedicinesEuStatuses.AsNoTracking()
            .Where(x => x.RevisionId == revisionId)
            .Select(x => new MedicinesEuStatusDto
            {
                EuOrphanStatus = x.EuOrphanStatus,
                EuOrphanStatusNumber = x.EuOrphanStatusNumber,
                EuOrphanGrantedDate = ToDto(x.EuOrphanGrantedDate),
                EuAtmpClassificationStatus = x.EuAtmpClassificationStatus,
                AtmpRecommendationDate = ToDto(x.AtmpRecommendationDate),
                AtmpClassification = ToDto(x.AtmpClassification),
            })
            .SingleOrDefaultAsync(cancellationToken);

    private Task<MedicinesGlobalSubmissionDto?> GetMedicinesGlobalSubmission(
        int revisionId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .MedicinesGlobalSubmissions.AsNoTracking()
            .Where(x => x.RevisionId == revisionId)
            .Select(x => new MedicinesGlobalSubmissionDto
            {
                GlobalFirstSubmissionRegion = x.GlobalFirstSubmissionRegion,
                GlobalSubmissionActualDate = ToDto(x.GlobalSubmissionActualDate),
            })
            .SingleOrDefaultAsync(cancellationToken);

    private Task<MedicinesIntlRecognitionDto?> GetMedicinesIntlRecognition(
        int revisionId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .MedicinesIntlRecognitions.AsNoTracking()
            .Where(x => x.RevisionId == revisionId)
            .Select(x => new MedicinesIntlRecognitionDto
            {
                IrpRoute = ToDto(x.IrpRoute),
                IntlConditionalApprovalAnticipated = x.IntlConditionalApprovalAnticipated,
                IntlSubmissionDate = ToDto(x.IntlSubmissionDate),
                IntlLicenceDate = ToDto(x.IntlLicenceDate),
            })
            .SingleOrDefaultAsync(cancellationToken);

    private async Task<IReadOnlyCollection<RecordClinicalTrialDto>> GetRecordClinicalTrials(
        int revisionId,
        CancellationToken cancellationToken
    ) =>
        await dbContext
            .RecordClinicalTrials.AsNoTracking()
            .Where(x => x.RevisionId == revisionId)
            .OrderBy(x => x.Id)
            .Select(x => new RecordClinicalTrialDto
            {
                StudyName = x.StudyName,
                ClinicalTrialsGovNumber = x.ClinicalTrialsGovNumber,
                BriefDescription = x.BriefDescription,
                RecruitingInUk = x.RecruitingInUk,
                TrialPhase = x.TrialPhase,
                OtherClinicalTrialNumbers = x
                    .OtherClinicalTrialNumbers.OrderBy(n => n.DisplayOrder)
                    .ThenBy(n => n.Id)
                    .Select(n => n.OtherRegistryNumber)
                    .ToList(),
            })
            .ToListAsync(cancellationToken);

    private Task<RecordHtaDto?> GetRecordHta(int revisionId, CancellationToken cancellationToken) =>
        dbContext
            .RecordHtas.AsNoTracking()
            .Where(x => x.RevisionId == revisionId)
            .Select(x => new RecordHtaDto
            {
                MedicineHtaSubmissionIntended = x.MedicineHtaSubmissionIntended,
                MedicineHtaBodies = ToFlagList(x.MedicineHtaBodies),
                HtaNiceAlignedPathway = x.HtaNiceAlignedPathway,
                HtaAdditionalDetails = x.HtaAdditionalDetails,
            })
            .SingleOrDefaultAsync(cancellationToken);

    private Task<RecordMhraDateDto?> GetRecordMhraDate(
        int revisionId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .RecordMhraDates.AsNoTracking()
            .Where(x => x.RevisionId == revisionId)
            .Select(x => new RecordMhraDateDto
            {
                UkSubmissionDate = ToDto(x.UkSubmissionDate),
                UkLicenceDate = ToDto(x.UkLicenceDate),
                UkLaunchDate = ToDto(x.UkLaunchDate),
            })
            .SingleOrDefaultAsync(cancellationToken);

    private Task<RecordMhraProcedureDto?> GetRecordMhraProcedure(
        int revisionId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .RecordMhraProcedures.AsNoTracking()
            .Where(x => x.RevisionId == revisionId)
            .Select(x => new RecordMhraProcedureDto
            {
                MhraProcedureType = ToDto(x.MhraProcedureType),
                IrpReferenceRegulator = ToDto(x.IrpReferenceRegulator),
                ProcedureDetails = x.ProcedureDetails,
            })
            .SingleOrDefaultAsync(cancellationToken);

    private static ReferenceDataDto? ToDto(ReferenceDataBase? referenceData) =>
        referenceData is null
            ? null
            : new ReferenceDataDto { Id = referenceData.Id, Label = referenceData.Label };

    private static RegulatoryDateDto? ToDto(RegulatoryDate? regulatoryDate) =>
        regulatoryDate is null
            ? null
            : new RegulatoryDateDto
            {
                DateValue = regulatoryDate.DateValue,
                DatePrecision = regulatoryDate.DatePrecision,
                IsConfidential = regulatoryDate.IsConfidential,
                ConditionalApprovalAnticipated = regulatoryDate.ConditionalApprovalAnticipated,
            };

    /// <summary>
    /// Splits a combined [Flags] value into the individual values it contains.
    /// </summary>
    private static List<TEnum>? ToFlagList<TEnum>(TEnum? combinedFlags)
        where TEnum : struct, Enum
    {
        return (combinedFlags is null)
            ? null
            : Enum.GetValues<TEnum>().Where(flag => combinedFlags.Value.HasFlag(flag)).ToList();
    }

    private sealed record RecordHeader(
        int OrganisationId,
        RecordType RecordType,
        RecordStatus RecordStatus,
        DateTime? ReviewedAt,
        int? PublishedRevisionId
    );
}
