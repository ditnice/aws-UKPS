using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using UKPS.Api.Application.Common;
using UKPS.Api.Application.InternalServices.Authorisation;
using UKPS.Api.Application.Records.Dtos;
using UKPS.Api.Application.Records.Dtos.RecordDetails;
using UKPS.Api.Application.Records.Errors;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Application.Records;

// Type aliases for Result
using GetRecordResult = Result<RecordDto, GetRecordError>;

internal sealed class RecordViewService(
    AppDbContext dbContext,
    IOrganisationAuthoriser organisationAuthoriser
) : IRecordViewService
{
    public async Task<GetRecordResult> GetRecord(
        int recordId,
        RecordType recordType,
        CancellationToken cancellationToken
    )
    {
        RecordHeader? record = await GetRecordHeader(recordId, cancellationToken);

        if (record is null)
        {
            return GetRecordResult.Err(new GetRecordError.NotFound(recordId));
        }

        if (
            !organisationAuthoriser.CanPerformOperationOnOrganisation(
                Operation.Read,
                record.OrganisationId
            )
        )
        {
            return GetRecordResult.Err(new GetRecordError.NotAllowed(recordId));
        }

        // Every record is created with a revision, so this only guards against bad data.
        if (record.LatestRevisionId is null)
        {
            return GetRecordResult.Err(new GetRecordError.NotFound(recordId));
        }

        if (record.RecordType != recordType)
        {
            return GetRecordResult.Err(
                new GetRecordError.RecordTypeMismatch(recordId, recordType, record.RecordType)
            );
        }

        int revisionId = record.LatestRevisionId.Value;

        return GetRecordResult.Ok(
            recordType switch
            {
                RecordType.Medicine => await GetMedicineRecord(
                    recordId,
                    record,
                    revisionId,
                    cancellationToken
                ),
                RecordType.Vaccine => GetVaccineRecord(recordId, record, revisionId),
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
            .SelectLatestRevision()
            .Select(x => new RecordHeader(
                x.Record.OrganisationId,
                x.Record.RecordType,
                x.Record.RecordStatus,
                x.Record.ReviewedAt,
                x.LatestRevisionId,
                x.DisplayStatus
            ))
            .SingleOrDefaultAsync(cancellationToken);

    private async Task<MedicineRecordDto> GetMedicineRecord(
        int recordId,
        RecordHeader record,
        int revisionId,
        CancellationToken cancellationToken
    )
    {
        return new MedicineRecordDto
        {
            RecordId = recordId,
            OrganisationId = record.OrganisationId,
            RecordStatus = record.RecordStatus,
            DisplayStatus = record.DisplayStatus,
            ReviewedAt = record.ReviewedAt,
            RevisionId = revisionId,
            RecordProductDetail = await GetRecordProductDetail(revisionId, cancellationToken),
            MedicinesIndicationDetail = await GetMedicinesIndicationDetail(
                revisionId,
                cancellationToken
            ),
            MedicinesDevelopmentBackground = await GetMedicinesDevelopmentBackground(
                revisionId,
                cancellationToken
            ),
            RecordClinicalTrialInformation = await GetRecordClinicalTrialInformation(
                revisionId,
                cancellationToken
            ),
            RecordClinicalTrials = await GetRecordClinicalTrials(revisionId, cancellationToken),
            RecordMhraProcedure = await GetRecordMhraProcedure(revisionId, cancellationToken),
            RecordMhraDate = await GetRecordMhraDate(revisionId, cancellationToken),
            MedicinesGlobalSubmission = await GetMedicinesGlobalSubmission(
                revisionId,
                cancellationToken
            ),
            MedicinesIntlRecognition = await GetMedicinesIntlRecognition(
                revisionId,
                cancellationToken
            ),
            RecordHta = await GetRecordHta(revisionId, cancellationToken),
            MedicinesEuStatus = await GetMedicinesEuStatus(revisionId, cancellationToken),
            MedicinesEamsPim = await GetMedicinesEamsPim(revisionId, cancellationToken),
            MedicinesLaboratoryTesting = await GetMedicinesLaboratoryTesting(
                revisionId,
                cancellationToken
            ),
            MedicinesPatientIdentification = await GetMedicinesPatientIdentification(
                revisionId,
                cancellationToken
            ),
            MedicinesTreatmentDetail = await GetMedicinesTreatmentDetail(
                revisionId,
                cancellationToken
            ),
            MedicinesServiceImpact = await GetMedicinesServiceImpact(revisionId, cancellationToken),
            MedicinesBudgetImpact = await GetMedicinesBudgetImpact(revisionId, cancellationToken),
        };
    }

    // TODO: Populate the vaccine record sections.
    private static VaccineRecordDto GetVaccineRecord(
        int recordId,
        RecordHeader record,
        int revisionId
    )
    {
        return new VaccineRecordDto
        {
            RecordId = recordId,
            OrganisationId = record.OrganisationId,
            RecordStatus = record.RecordStatus,
            DisplayStatus = record.DisplayStatus,
            ReviewedAt = record.ReviewedAt,
            RevisionId = revisionId,
        };
    }

    private Task<RecordProductDetailDto?> GetRecordProductDetail(
        int revisionId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .RecordProductDetails.AsNoTracking()
            .Where(x => x.RevisionId == revisionId)
            .Select(x => new RecordProductDetailDto
            {
                CompanyCode = x.CompanyCode,
                NamesAndIdentifiers = x
                    .NamesAndIdentifiers.OrderBy(n => n.DisplayOrder)
                    .ThenBy(n => n.Id)
                    .Select(n => new RecordNameAndIdentifierDto
                    {
                        Name = n.Name,
                        NameType = n.NameType,
                    })
                    .ToList(),
                BrandedName = x.BrandedName,
                RecordTitle = x.RecordTitle,
            })
            .SingleOrDefaultAsync(cancellationToken);

    private Task<MedicinesIndicationDetailDto?> GetMedicinesIndicationDetail(
        int revisionId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .MedicinesIndicationDetails.AsNoTracking()
            .Where(x => x.RevisionId == revisionId)
            .Select(x => new MedicinesIndicationDetailDto
            {
                Indication = x.Indication,
                BnfChapter =
                    x.BnfChapter == null
                        ? null
                        : new ReferenceDataDto { Id = x.BnfChapter.Id, Label = x.BnfChapter.Label },
                TherapeuticAreas = x
                    .TherapeuticAreas.OrderBy(t => t.TherapeuticArea!.DisplayOrder)
                    .ThenBy(t => t.TherapeuticArea!.Label)
                    .Select(t => new ReferenceDataDto
                    {
                        Id = t.TherapeuticAreaId,
                        Label = t.TherapeuticArea!.Label,
                    })
                    .ToList(),
                IndicationIsPaediatric = x.IndicationIsPaediatric,
                IndicationIsCancer = x.IndicationIsCancer,
                IndicationIsRareDisease = x.IndicationIsRareDisease,
                FormulationType = ToDto(x.FormulationType),
                Presentation = x.Presentation,
                ModeOfAction = x.ModeOfAction,
                ProposedDoseRegimen = x.ProposedDoseRegimen,
                IsPersonalisedMedicine = x.IsPersonalisedMedicine,
                MedicineTechnologyStatus = ToFlagList(x.MedicineTechnologyStatus),
            })
            .SingleOrDefaultAsync(cancellationToken);

    private Task<MedicinesDevelopmentBackgroundDto?> GetMedicinesDevelopmentBackground(
        int revisionId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .MedicinesDevelopmentBackgrounds.AsNoTracking()
            .Where(x => x.RevisionId == revisionId)
            .Select(x => new MedicinesDevelopmentBackgroundDto
            {
                IsRepurposedMedicine = x.IsRepurposedMedicine,
                RepurposedMedicineDetails = x.RepurposedMedicineDetails,
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
                AdditionalGenomicFactors = x.AdditionalGenomicFactors,
                GenomicTestUsedInTrials = x.GenomicTestUsedInTrials,
                GenomicTestSpecificitySensitivity = x.GenomicTestSpecificitySensitivity,
                GenomicTestMandatoryStatus = x.GenomicTestMandatoryStatus,
                GenomicTestNotes = x.GenomicTestNotes,
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
                UkPatientPopulationRange = ToDto(x.UkPatientPopulationRange),
                UkPatientPopulationNotes = x.UkPatientPopulationNotes,
                EstimatedEligiblePatientPopulation = x.EstimatedEligiblePatientPopulation,
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
                EstimatedUptake = x.EstimatedUptake,
                CompassionateAccessAvailable = x.CompassionateAccessAvailable,
                CompassionateAccessDetails = x.CompassionateAccessDetails,
                PatientAccessSchemePlanned = x.PatientAccessSchemePlanned,
                PatientAccessSchemeRegions = ToFlagList(x.PatientAccessSchemeRegions),
                IndicationSpecificPricingPlanned = x.IndicationSpecificPricingPlanned,
                IndicationSpecificPricingDetails = x.IndicationSpecificPricingDetails,
                NetUkBudgetImpactBand = x.NetUkBudgetImpactBand,
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
                EamsSubmissionDate = ToDto(x.EamsSubmissionDate),
                EamsOpinionDate = ToDto(x.EamsOpinionDate),
                EamsOpinionDecision = x.EamsOpinionDecision,
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
                IntlSubmissionDate = ToDto(x.IntlSubmissionDate),
                IntlLicenceDate = ToDto(x.IntlLicenceDate),
                IntlConditionalApprovalAnticipated = x.IntlConditionalApprovalAnticipated,
            })
            .SingleOrDefaultAsync(cancellationToken);

    private Task<RecordClinicalTrialInformationDto?> GetRecordClinicalTrialInformation(
        int revisionId,
        CancellationToken cancellationToken
    ) =>
        dbContext
            .RecordClinicalTrialInformation.AsNoTracking()
            .Where(x => x.RevisionId == revisionId)
            .Select(x => new RecordClinicalTrialInformationDto
            {
                RecruitingInUk = x.RecruitingInUk,
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
                OtherClinicalTrialNumbers = x
                    .OtherClinicalTrialNumbers.OrderBy(n => n.DisplayOrder)
                    .ThenBy(n => n.Id)
                    .Select(n => n.OtherRegistryNumber)
                    .ToList(),
                TrialPhase = x.TrialPhase,
                BriefDescription = x.BriefDescription,
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
                HtaAdditionalDetails = x.HtaAdditionalDetails,
                HtaNiceAlignedPathway = x.HtaNiceAlignedPathway,
                NiceTaDevelopmentId = x.NiceTaDevelopmentId,
                UkLaunchDate = ToDto(x.UkLaunchDate),
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
                UkConditionalApprovalAnticipated = x.UkConditionalApprovalAnticipated,
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
        int? LatestRevisionId,
        RecordDisplayStatus DisplayStatus
    );
}
