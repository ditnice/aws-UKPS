using UKPS.Api.Persistence;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Tests.Utilities.Data;

/// <summary>
/// Writes answers to every required medicine question on a revision, taking the shortest route
/// through the form: every gate that reveals follow-up questions is answered No.
/// </summary>
internal static class CompleteMedicineRevision
{
    /// <summary>Adds the rows and the reference data they point at.</summary>
    /// <param name="context">The context to add them with.</param>
    /// <param name="revisionId">The revision to complete.</param>
    /// <param name="ukLaunchQuarter">The first day of the estimated launch quarter.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    public static async Task AddAsync(
        AppDbContext context,
        int revisionId,
        DateOnly ukLaunchQuarter,
        CancellationToken cancellationToken
    )
    {
        BnfChapter bnfChapter = new()
        {
            Code = "5",
            Label = "Infections",
            DisplayOrder = 1,
        };
        FormulationType formulation = new() { Label = "Tablet" };
        UkPatientPopulationRange populationRange = new()
        {
            Label = "1,000 to 5,000",
            SortOrder = 1,
        };
        context.AddRange(bnfChapter, formulation, populationRange);
        await context.SaveChangesAsync(cancellationToken);

        context.AddRange(
            new MedicinesIndicationDetail
            {
                RevisionId = revisionId,
                Indication = "Chronic hepatitis C virus infection in adults",
                BnfChapterId = bnfChapter.Id,
                IndicationIsPaediatric = IndicationPaediatricStatus.ExclusivelyAdults,
                IndicationIsCancer = YesNoUnknown.No,
                IndicationIsRareDisease = YesNoUnknown.No,
                FormulationTypeId = formulation.Id,
                IsPersonalisedMedicine = YesNoUnknown.No,
                MedicineTechnologyStatus = MedicineTechnologyStatus.NewIndication,
            },
            new MedicinesLaboratoryTesting
            {
                RevisionId = revisionId,
                DiagnosticTestRequired = YesNoUnknown.No,
                MonitoringTestsDetails = "Liver function tests",
                SafetyTestsDetails = "None",
            },
            new MedicinesPatientIdentification
            {
                RevisionId = revisionId,
                ScreeningRequired = YesNoUnknown.No,
                UrgentIdentificationRequired = YesNoUnknown.No,
            },
            new MedicinesDevelopmentBackground
            {
                RevisionId = revisionId,
                IsRepurposedMedicine = YesNoUnknown.No,
                IsOriginatorCompany = YesNoUnknown.Yes,
                IsCoMarketed = YesNoUnknown.No,
            },
            new MedicinesServiceImpact
            {
                RevisionId = revisionId,
                NhsServiceChangesRequired = NhsServiceChangesRequired.NoChanges,
                HandlingStorageRequirements = YesNoUnknown.No,
                UkPatientPopulationRangeId = populationRange.Id,
                UkPatientPopulationNotes = "Adults diagnosed in secondary care",
                EstimatedEligiblePatientPopulation = "Around 3,000",
            },
            new MedicinesTreatmentDetail
            {
                RevisionId = revisionId,
                ProposedPlaceInTherapy = "First line",
                EstimatedDurationOfTreatment = "12 weeks",
            },
            new RecordHta
            {
                RevisionId = revisionId,
                MedicineHtaSubmissionIntended = YesNoUnknown.No,
                UkLaunchDate = new RegulatoryDate
                {
                    RevisionId = revisionId,
                    DateEvent = DateEventType.UkLaunch,
                    DatePrecision = DatePrecision.EstimatedQuarter,
                    DateValue = ukLaunchQuarter,
                    IsConfidential = true,
                },
            }
        );
        await context.SaveChangesAsync(cancellationToken);
        context.ChangeTracker.Clear();
    }
}
