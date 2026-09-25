using Bogus;
using Shouldly;
using UKPS.Api.Application.Records;
using UKPS.Api.Application.Records.Dtos.PublishedRecord;
using UKPS.Api.Application.Records.Errors;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.AssertionHelpers;
using UKPS.Api.Tests.Utilities.Fixtures;
using UKPS.Api.Tests.Utilities.Harnesses;
using GetPublishedRecordResult = UKPS.Api.Application.Common.Result<
    UKPS.Api.Application.Records.Dtos.PublishedRecord.PublishedRecordDto,
    UKPS.Api.Application.Records.Errors.GetPublishedRecordError
>;
using Record = UKPS.Api.Persistence.Entities.RecordWorkflow.Record;

namespace UKPS.Api.Tests.Application.Records;

[Collection(DatabaseCollection.Name)]
public class RecordViewServiceTests : DatabaseTestBase
{
    private static readonly DateTime _reviewedAt = new(2026, 6, 19, 12, 0, 0, DateTimeKind.Utc);

    private Organisation _organisation = null!;
    private User _user = null!;
    private IServiceTestHarness<IRecordViewService> _harness = null!;
    private IRecordViewService Service => _harness.Service;

    public RecordViewServiceTests(PostgresFixture fixture)
        : base(fixture)
    {
        Randomizer.Seed = new Random(547);
    }

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        _organisation = await AddEntity(
            new OrganisationFaker().Generate(),
            TestContext.Current.CancellationToken
        );
        _user = await AddEntity(new UserFaker().Generate(), TestContext.Current.CancellationToken);

        _harness = new ServiceTestHarness<IRecordViewService>(Context).UpdateCurrentUser(x =>
            x with
            {
                OrganisationId = _organisation.Id,
                UserRole = UserRole.Standard,
            }
        );
    }

    [Theory]
    [InlineData(RecordStatus.Active)]
    [InlineData(RecordStatus.OnHold)]
    public async Task GetPublishedRecord_ActiveOrOnHold_ReturnsRecordHeader(RecordStatus status)
    {
        Record record = await AddRecord(status);
        RecordRevision revision = await AddRevision(record, 1, WorkflowStatus.Published);

        GetPublishedRecordResult result = await Service.GetPublishedRecord(
            record.Id,
            RecordType.Medicine,
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess().ShouldBeOfType<PublishedMedicineRecordDto>();
        dto.RecordId.ShouldBe(record.Id);
        dto.OrganisationId.ShouldBe(_organisation.Id);
        dto.RecordStatus.ShouldBe(status);
        dto.ReviewedAt.ShouldBe(_reviewedAt);
        dto.RevisionId.ShouldBe(revision.Id);
    }

    [Fact]
    public async Task GetPublishedRecord_MultipleRevisions_ReturnsLatestPublishedRevision()
    {
        Record record = await AddRecord(RecordStatus.Active);
        RecordRevision first = await AddRevision(record, 1, WorkflowStatus.Published);
        RecordRevision second = await AddRevision(record, 2, WorkflowStatus.Published);
        RecordRevision rejected = await AddRevision(record, 3, WorkflowStatus.Rejected);
        RecordRevision inReview = await AddRevision(record, 4, WorkflowStatus.InReview);
        RecordRevision draft = await AddRevision(record, 5, WorkflowStatus.Draft);
        await AddEntities(
            new[] { first, second, rejected, inReview, draft }.Select(r =>
                new MedicinesProductDetailFaker()
                    .RuleFor(x => x.RevisionId, r.Id)
                    .RuleFor(x => x.RecordTitle, $"Title {r.Id}")
                    .Generate()
            ),
            TestContext.Current.CancellationToken
        );

        GetPublishedRecordResult result = await Service.GetPublishedRecord(
            record.Id,
            RecordType.Medicine,
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess().ShouldBeOfType<PublishedMedicineRecordDto>();
        dto.RevisionId.ShouldBe(second.Id);
        dto.MedicinesProductDetail.ShouldNotBeNull().RecordTitle.ShouldBe($"Title {second.Id}");
    }

    [Theory]
    [InlineData(RecordStatus.Unpublished, WorkflowStatus.Draft)]
    [InlineData(RecordStatus.Archived, WorkflowStatus.Published)]
    public async Task GetPublishedRecord_NotActiveOrOnHold_ReturnsNotFound(
        RecordStatus recordStatus,
        WorkflowStatus workflowStatus
    )
    {
        Record record = await AddRecord(recordStatus);
        await AddRevision(record, 1, workflowStatus);

        GetPublishedRecordResult result = await Service.GetPublishedRecord(
            record.Id,
            RecordType.Medicine,
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError().ShouldBeOfType<GetPublishedRecordError.NotFound>();
    }

    [Fact]
    public async Task GetPublishedRecord_NoPublishedRevision_ReturnsNotFound()
    {
        Record record = await AddRecord(RecordStatus.OnHold);
        await AddRevision(record, 1, WorkflowStatus.Draft);

        GetPublishedRecordResult result = await Service.GetPublishedRecord(
            record.Id,
            RecordType.Medicine,
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError().ShouldBeOfType<GetPublishedRecordError.NotFound>();
    }

    [Fact]
    public async Task GetPublishedRecord_RecordDoesNotExist_ReturnsNotFound()
    {
        GetPublishedRecordResult result = await Service.GetPublishedRecord(
            999_999,
            RecordType.Medicine,
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError().ShouldBeOfType<GetPublishedRecordError.NotFound>();
    }

    [Fact]
    public async Task GetPublishedRecord_UserInAnotherOrganisation_ReturnsNotAllowed()
    {
        Record record = await AddRecord(RecordStatus.Active);
        await AddRevision(record, 1, WorkflowStatus.Published);
        var service = _harness
            .UpdateCurrentUser(x => x with { OrganisationId = _organisation.Id + 1 })
            .Service;

        GetPublishedRecordResult result = await service.GetPublishedRecord(
            record.Id,
            RecordType.Medicine,
            TestContext.Current.CancellationToken
        );

        result.ShouldBeError().ShouldBeOfType<GetPublishedRecordError.NotAllowed>();
    }

    [Fact]
    public async Task GetPublishedRecord_VaccineRecord_ReturnsVaccineRecord()
    {
        Record record = await AddRecord(RecordStatus.Active, RecordType.Vaccine);
        RecordRevision revision = await AddRevision(record, 1, WorkflowStatus.Published);

        GetPublishedRecordResult result = await Service.GetPublishedRecord(
            record.Id,
            RecordType.Vaccine,
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess().ShouldBeOfType<PublishedVaccineRecordDto>();
        dto.RecordId.ShouldBe(record.Id);
        dto.RevisionId.ShouldBe(revision.Id);
    }

    [Theory]
    [InlineData(RecordType.Medicine, RecordType.Vaccine)]
    [InlineData(RecordType.Vaccine, RecordType.Medicine)]
    public async Task GetPublishedRecord_RequestedTypeDiffers_ReturnsRecordTypeMismatch(
        RecordType actualRecordType,
        RecordType requestedRecordType
    )
    {
        Record record = await AddRecord(RecordStatus.Active, actualRecordType);
        await AddRevision(record, 1, WorkflowStatus.Published);

        GetPublishedRecordResult result = await Service.GetPublishedRecord(
            record.Id,
            requestedRecordType,
            TestContext.Current.CancellationToken
        );

        var error = result
            .ShouldBeError()
            .ShouldBeOfType<GetPublishedRecordError.RecordTypeMismatch>();
        error.RequestedRecordType.ShouldBe(requestedRecordType);
        error.ActualRecordType.ShouldBe(actualRecordType);
    }

    [Fact]
    public async Task GetPublishedRecord_NoSectionData_ReturnsEmptySections()
    {
        Record record = await AddRecord(RecordStatus.Active);
        await AddRevision(record, 1, WorkflowStatus.Published);

        GetPublishedRecordResult result = await Service.GetPublishedRecord(
            record.Id,
            RecordType.Medicine,
            TestContext.Current.CancellationToken
        );

        var dto = result.ShouldBeSuccess().ShouldBeOfType<PublishedMedicineRecordDto>();
        dto.MedicinesProductDetail.ShouldBeNull();
        dto.MedicinesDetail.ShouldBeNull();
        dto.MedicinesCompanyInfo.ShouldBeNull();
        dto.MedicinesTreatmentDetail.ShouldBeNull();
        dto.MedicinesPatientIdentification.ShouldBeNull();
        dto.MedicinesLaboratoryTesting.ShouldBeNull();
        dto.MedicinesServiceImpact.ShouldBeNull();
        dto.MedicinesBudgetImpact.ShouldBeNull();
        dto.MedicinesEamsPim.ShouldBeNull();
        dto.MedicinesEuStatus.ShouldBeNull();
        dto.MedicinesGlobalSubmission.ShouldBeNull();
        dto.MedicinesIntlRecognition.ShouldBeNull();
        dto.RecordClinicalTrials.ShouldBeEmpty();
        dto.RecordHta.ShouldBeNull();
        dto.RecordMhraDate.ShouldBeNull();
        dto.RecordMhraProcedure.ShouldBeNull();
    }

    [Fact]
    public async Task GetPublishedRecord_ProductDetail_MapsAllFields()
    {
        var ct = TestContext.Current.CancellationToken;
        Record record = await AddRecord(RecordStatus.Active);
        RecordRevision revision = await AddRevision(record, 1, WorkflowStatus.Published);
        var bnfChapter = await AddEntity(new BnfChapter { Code = "1.1", Label = "Dyspepsia" }, ct);
        var formulationType = await AddEntity(new FormulationType { Label = "Tablet" }, ct);
        var areaB = await AddEntity(
            new TherapeuticArea { Label = "Oncology", DisplayOrder = 2 },
            ct
        );
        var areaA = await AddEntity(
            new TherapeuticArea { Label = "Cardiology", DisplayOrder = 1 },
            ct
        );
        await AddEntity(
            new MedicinesProductDetail
            {
                RevisionId = revision.Id,
                RecordTitle = "Chronic hepatitis C in adults",
                BrandedName = "Brand",
                Indication = "Hepatitis C",
                IndicationIsPaediatric = IndicationPaediatricStatus.ExclusivelyAdults,
                IndicationIsCancer = YesNoUnknown.No,
                IndicationIsRareDisease = YesNoUnknown.Yes,
                NiceTaDevelopmentId = "GID-TA1234",
                BnfChapterId = bnfChapter.Id,
                FormulationTypeId = formulationType.Id,
                Presentation = "10mg",
                MedicineTechnologyStatus =
                    MedicineTechnologyStatus.Biosimilar | MedicineTechnologyStatus.NewIndication,
                TherapeuticAreas =
                [
                    new MedicinesProductDetailTherapeuticArea { TherapeuticAreaId = areaB.Id },
                    new MedicinesProductDetailTherapeuticArea { TherapeuticAreaId = areaA.Id },
                ],
                ActiveSubstances =
                [
                    new MedicinesActiveSubstance
                    {
                        Name = "ABC-123",
                        NameType = SubstanceNameType.DevelopmentName,
                        DisplayOrder = 2,
                    },
                    new MedicinesActiveSubstance
                    {
                        Name = "abcumab",
                        NameType = SubstanceNameType.GenericName,
                        DisplayOrder = 1,
                    },
                ],
            },
            ct
        );

        GetPublishedRecordResult result = await Service.GetPublishedRecord(
            record.Id,
            RecordType.Medicine,
            ct
        );

        var dto = result
            .ShouldBeSuccess()
            .ShouldBeOfType<PublishedMedicineRecordDto>()
            .MedicinesProductDetail.ShouldNotBeNull();
        dto.RecordTitle.ShouldBe("Chronic hepatitis C in adults");
        dto.BrandedName.ShouldBe("Brand");
        dto.Indication.ShouldBe("Hepatitis C");
        dto.IndicationIsPaediatric.ShouldBe(IndicationPaediatricStatus.ExclusivelyAdults);
        dto.IndicationIsCancer.ShouldBe(YesNoUnknown.No);
        dto.IndicationIsRareDisease.ShouldBe(YesNoUnknown.Yes);
        dto.NiceTaDevelopmentId.ShouldBe("GID-TA1234");
        dto.BnfChapter.ShouldBe(new ReferenceDataDto { Id = bnfChapter.Id, Label = "Dyspepsia" });
        dto.FormulationType.ShouldBe(
            new ReferenceDataDto { Id = formulationType.Id, Label = "Tablet" }
        );
        dto.Presentation.ShouldBe("10mg");
        dto.MedicineTechnologyStatus.ShouldBe([
            MedicineTechnologyStatus.Biosimilar,
            MedicineTechnologyStatus.NewIndication,
        ]);
        dto.TherapeuticAreas.ShouldBe([
            new ReferenceDataDto { Id = areaA.Id, Label = "Cardiology" },
            new ReferenceDataDto { Id = areaB.Id, Label = "Oncology" },
        ]);
        dto.ActiveSubstances.ShouldBe([
            new MedicinesActiveSubstanceDto
            {
                Name = "abcumab",
                NameType = SubstanceNameType.GenericName,
            },
            new MedicinesActiveSubstanceDto
            {
                Name = "ABC-123",
                NameType = SubstanceNameType.DevelopmentName,
            },
        ]);
    }

    [Fact]
    public async Task GetPublishedRecord_MedicineSections_MapsAllFields()
    {
        var ct = TestContext.Current.CancellationToken;
        Record record = await AddRecord(RecordStatus.Active);
        RecordRevision revision = await AddRevision(record, 1, WorkflowStatus.Published);
        var pathwayPoint = await AddEntity(new PatientPathwayPoint { Label = "Diagnosis" }, ct);
        var populationRange = await AddEntity(
            new UkPatientPopulationRange { Label = "1,000 to 10,000" },
            ct
        );
        var atmpClassification = await AddEntity(
            new AtmpClassification { Label = "Gene therapy" },
            ct
        );
        var irpRoute = await AddEntity(new IrpRoute { Label = "Route A" }, ct);
        var eamsSubmission = await AddDate(revision, DateEventType.EamsSubmission, 1);
        var eamsOpinion = await AddDate(revision, DateEventType.EamsOpinion, 2);
        var orphanGranted = await AddDate(revision, DateEventType.EuOrphanGranted, 3);
        var atmpRecommendation = await AddDate(
            revision,
            DateEventType.AtmpClassificationRecommendation,
            4
        );
        var globalSubmission = await AddDate(revision, DateEventType.GlobalFirstSubmission, 5);
        var intlSubmission = await AddDate(revision, DateEventType.IntlSubmission, 6);
        var intlLicence = await AddDate(revision, DateEventType.IntlLicence, 7);

        Context.AddRange(
            new MedicinesDetail
            {
                RevisionId = revision.Id,
                ModeOfAction = "Mode",
                ProposedDoseRegimen = "Dose",
                IsPersonalisedMedicine = YesNoUnknown.Yes,
                IsRepurposedMedicine = YesNoUnknown.No,
                RepurposedMedicineDetails = "Repurposed",
            },
            new MedicinesCompanyInfo
            {
                RevisionId = revision.Id,
                IsOriginatorCompany = YesNoUnknown.No,
                OriginatorCompanyName = "Originator",
                IsCoMarketed = YesNoUnknown.Yes,
                CoMarketingCompanyName = "Co-marketer",
            },
            new MedicinesTreatmentDetail
            {
                RevisionId = revision.Id,
                ProposedPlaceInTherapy = "First line",
                EstimatedDurationOfTreatment = "6 months",
            },
            new MedicinesPatientIdentification
            {
                RevisionId = revision.Id,
                ScreeningRequired = YesNoUnknown.Yes,
                ScreeningDetails = "Screening",
                UrgentIdentificationRequired = YesNoUnknown.No,
                UrgentIdentificationDetails = "Urgent",
            },
            new MedicinesLaboratoryTesting
            {
                RevisionId = revision.Id,
                DiagnosticTestRequired = YesNoUnknown.Yes,
                BiomarkerType = BiomarkerType.GenomicBiomarker,
                NonGenomicBiomarkerDescription = "Non-genomic",
                GenomicTarget = "Target",
                GenomicTestNgtdRelationship = GenomicTestNgtdRelationship.NewTest,
                GenomicSampleType = "Blood",
                GenomicTurnaroundTimeDetails = "2 weeks",
                PatientPathwayPointId = pathwayPoint.Id,
                GenomicTestPathwayPointOther = "Other point",
                GenomicAlterations = "Alterations",
                GenomicTestUsedInTrials = "Trial test",
                GenomicTestSpecificitySensitivity = "High",
                GenomicTestNotes = "Notes",
                GenomicTestMandatoryStatus = GenomicTestMandatoryStatus.MandatoryNoAlternatives,
                AdditionalGenomicFactors = "Factors",
                MonitoringTestsDetails = "Monitoring",
                SafetyTestsDetails = "Safety",
            },
            new MedicinesServiceImpact
            {
                RevisionId = revision.Id,
                NhsServiceChangesRequired = NhsServiceChangesRequired.SomeChange,
                NhsServiceChangesDetails = "Changes",
                HandlingStorageRequirements = YesNoUnknown.Yes,
                HandlingStorageDetails = "Fridge",
                EstimatedUptake = "High",
                UkPatientPopulationRangeId = populationRange.Id,
                UkPatientPopulationNotes = "Population notes",
                EstimatedEligiblePatientPopulation = "5,000",
                CompassionateAccessAvailable = YesNoUnknown.No,
                CompassionateAccessDetails = "Compassionate",
            },
            new MedicinesBudgetImpact
            {
                RevisionId = revision.Id,
                PatientAccessSchemePlanned = YesNoUnknown.Yes,
                IndicationSpecificPricingPlanned = YesNoUnknown.No,
                IndicationSpecificPricingDetails = "Pricing",
                NetUkBudgetImpactBand = NetUkBudgetImpactBand.Between5MAnd40M,
                PatientAccessSchemeRegions =
                    PatientAccessSchemeRegion.England | PatientAccessSchemeRegion.Scotland,
            },
            new MedicinesEamsPim
            {
                RevisionId = revision.Id,
                PimDesignationStatus = DesignationStatus.Granted,
                WillSubmitToEams = YesNoUnknown.Yes,
                EamsOpinionDecision = EamsOpinionDecision.Positive,
                EamsSubmissionDateId = eamsSubmission.Id,
                EamsOpinionDateId = eamsOpinion.Id,
            },
            new MedicinesEuStatus
            {
                RevisionId = revision.Id,
                EuOrphanStatus = DesignationStatus.NotGranted,
                EuOrphanStatusNumber = "EU/3/26/1234",
                EuOrphanGrantedDateId = orphanGranted.Id,
                EuAtmpClassificationStatus = DesignationStatus.Granted,
                AtmpRecommendationDateId = atmpRecommendation.Id,
                AtmpClassificationId = atmpClassification.Id,
            },
            new MedicinesGlobalSubmission
            {
                RevisionId = revision.Id,
                GlobalFirstSubmissionRegion = "USA",
                GlobalSubmissionActualDateId = globalSubmission.Id,
            },
            new MedicinesIntlRecognition
            {
                RevisionId = revision.Id,
                IrpRouteId = irpRoute.Id,
                IntlConditionalApprovalAnticipated = YesNoUnknown.Unknown,
                IntlSubmissionDateId = intlSubmission.Id,
                IntlLicenceDateId = intlLicence.Id,
            }
        );
        await Context.SaveChangesAsync(ct);

        GetPublishedRecordResult result = await Service.GetPublishedRecord(
            record.Id,
            RecordType.Medicine,
            ct
        );

        var dto = result.ShouldBeSuccess().ShouldBeOfType<PublishedMedicineRecordDto>();
        dto.MedicinesDetail.ShouldBe(
            new MedicinesDetailDto
            {
                ModeOfAction = "Mode",
                ProposedDoseRegimen = "Dose",
                IsPersonalisedMedicine = YesNoUnknown.Yes,
                IsRepurposedMedicine = YesNoUnknown.No,
                RepurposedMedicineDetails = "Repurposed",
            }
        );
        dto.MedicinesCompanyInfo.ShouldBe(
            new MedicinesCompanyInfoDto
            {
                IsOriginatorCompany = YesNoUnknown.No,
                OriginatorCompanyName = "Originator",
                IsCoMarketed = YesNoUnknown.Yes,
                CoMarketingCompanyName = "Co-marketer",
            }
        );
        dto.MedicinesTreatmentDetail.ShouldBe(
            new MedicinesTreatmentDetailDto
            {
                ProposedPlaceInTherapy = "First line",
                EstimatedDurationOfTreatment = "6 months",
            }
        );
        dto.MedicinesPatientIdentification.ShouldBe(
            new MedicinesPatientIdentificationDto
            {
                ScreeningRequired = YesNoUnknown.Yes,
                ScreeningDetails = "Screening",
                UrgentIdentificationRequired = YesNoUnknown.No,
                UrgentIdentificationDetails = "Urgent",
            }
        );
        dto.MedicinesLaboratoryTesting.ShouldBe(
            new MedicinesLaboratoryTestingDto
            {
                DiagnosticTestRequired = YesNoUnknown.Yes,
                BiomarkerType = BiomarkerType.GenomicBiomarker,
                NonGenomicBiomarkerDescription = "Non-genomic",
                GenomicTarget = "Target",
                GenomicTestNgtdRelationship = GenomicTestNgtdRelationship.NewTest,
                GenomicSampleType = "Blood",
                GenomicTurnaroundTimeDetails = "2 weeks",
                PatientPathwayPoint = new ReferenceDataDto
                {
                    Id = pathwayPoint.Id,
                    Label = "Diagnosis",
                },
                GenomicTestPathwayPointOther = "Other point",
                GenomicAlterations = "Alterations",
                GenomicTestUsedInTrials = "Trial test",
                GenomicTestSpecificitySensitivity = "High",
                GenomicTestNotes = "Notes",
                GenomicTestMandatoryStatus = GenomicTestMandatoryStatus.MandatoryNoAlternatives,
                AdditionalGenomicFactors = "Factors",
                MonitoringTestsDetails = "Monitoring",
                SafetyTestsDetails = "Safety",
            }
        );
        dto.MedicinesServiceImpact.ShouldBe(
            new MedicinesServiceImpactDto
            {
                NhsServiceChangesRequired = NhsServiceChangesRequired.SomeChange,
                NhsServiceChangesDetails = "Changes",
                HandlingStorageRequirements = YesNoUnknown.Yes,
                HandlingStorageDetails = "Fridge",
                EstimatedUptake = "High",
                UkPatientPopulationRange = new ReferenceDataDto
                {
                    Id = populationRange.Id,
                    Label = "1,000 to 10,000",
                },
                UkPatientPopulationNotes = "Population notes",
                EstimatedEligiblePatientPopulation = "5,000",
                CompassionateAccessAvailable = YesNoUnknown.No,
                CompassionateAccessDetails = "Compassionate",
            }
        );

        var budgetImpact = dto.MedicinesBudgetImpact.ShouldNotBeNull();
        budgetImpact.PatientAccessSchemePlanned.ShouldBe(YesNoUnknown.Yes);
        budgetImpact.IndicationSpecificPricingPlanned.ShouldBe(YesNoUnknown.No);
        budgetImpact.IndicationSpecificPricingDetails.ShouldBe("Pricing");
        budgetImpact.NetUkBudgetImpactBand.ShouldBe(NetUkBudgetImpactBand.Between5MAnd40M);
        budgetImpact.PatientAccessSchemeRegions.ShouldBe([
            PatientAccessSchemeRegion.England,
            PatientAccessSchemeRegion.Scotland,
        ]);

        dto.MedicinesEamsPim.ShouldBe(
            new MedicinesEamsPimDto
            {
                PimDesignationStatus = DesignationStatus.Granted,
                WillSubmitToEams = YesNoUnknown.Yes,
                EamsOpinionDecision = EamsOpinionDecision.Positive,
                EamsSubmissionDate = ToDto(eamsSubmission),
                EamsOpinionDate = ToDto(eamsOpinion),
            }
        );
        dto.MedicinesEuStatus.ShouldBe(
            new MedicinesEuStatusDto
            {
                EuOrphanStatus = DesignationStatus.NotGranted,
                EuOrphanStatusNumber = "EU/3/26/1234",
                EuOrphanGrantedDate = ToDto(orphanGranted),
                EuAtmpClassificationStatus = DesignationStatus.Granted,
                AtmpRecommendationDate = ToDto(atmpRecommendation),
                AtmpClassification = new ReferenceDataDto
                {
                    Id = atmpClassification.Id,
                    Label = "Gene therapy",
                },
            }
        );
        dto.MedicinesGlobalSubmission.ShouldBe(
            new MedicinesGlobalSubmissionDto
            {
                GlobalFirstSubmissionRegion = "USA",
                GlobalSubmissionActualDate = ToDto(globalSubmission),
            }
        );
        dto.MedicinesIntlRecognition.ShouldBe(
            new MedicinesIntlRecognitionDto
            {
                IrpRoute = new ReferenceDataDto { Id = irpRoute.Id, Label = "Route A" },
                IntlConditionalApprovalAnticipated = YesNoUnknown.Unknown,
                IntlSubmissionDate = ToDto(intlSubmission),
                IntlLicenceDate = ToDto(intlLicence),
            }
        );
    }

    [Fact]
    public async Task GetPublishedRecord_SharedSections_MapsAllFields()
    {
        var ct = TestContext.Current.CancellationToken;
        Record record = await AddRecord(RecordStatus.Active);
        RecordRevision revision = await AddRevision(record, 1, WorkflowStatus.Published);
        var procedureType = await AddEntity(new MhraProcedureType { Label = "IRP" }, ct);
        var regulator = await AddEntity(new IrpReferenceRegulator { Label = "FDA" }, ct);
        var ukSubmission = await AddDate(revision, DateEventType.UkSubmission, 1);
        var ukLicence = await AddDate(revision, DateEventType.UkLicence, 2);
        var ukLaunch = await AddDate(revision, DateEventType.UkLaunch, 3);

        Context.AddRange(
            new RecordClinicalTrial
            {
                RevisionId = revision.Id,
                StudyName = "Study A",
                ClinicalTrialsGovNumber = "NCT00000001",
                BriefDescription = "Description A",
                RecruitingInUk = YesNoUnknown.Yes,
                TrialPhase = TrialPhase.PhaseIII,
                OtherClinicalTrialNumbers =
                [
                    new OtherClinicalTrialNumber
                    {
                        OtherRegistryNumber = "EudraCT 2",
                        DisplayOrder = 2,
                    },
                    new OtherClinicalTrialNumber
                    {
                        OtherRegistryNumber = "ISRCTN 1",
                        DisplayOrder = 1,
                    },
                ],
            },
            new RecordClinicalTrial
            {
                RevisionId = revision.Id,
                StudyName = "Study B",
                ClinicalTrialsGovNumber = "NCT00000002",
            },
            new RecordHta
            {
                RevisionId = revision.Id,
                MedicineHtaSubmissionIntended = YesNoUnknown.Yes,
                MedicineHtaBodies = MedicineHtaAssessor.Nice | MedicineHtaAssessor.Awmsg,
                HtaNiceAlignedPathway = YesNoUnknown.No,
                HtaAdditionalDetails = "HTA details",
            },
            new RecordMhraDate
            {
                RevisionId = revision.Id,
                UkSubmissionDateId = ukSubmission.Id,
                UkLicenceDateId = ukLicence.Id,
                UkLaunchDateId = ukLaunch.Id,
            },
            new RecordMhraProcedure
            {
                RevisionId = revision.Id,
                MhraProcedureTypeId = procedureType.Id,
                IrpReferenceRegulatorId = regulator.Id,
                ProcedureDetails = "Procedure details",
            }
        );
        await Context.SaveChangesAsync(ct);

        GetPublishedRecordResult result = await Service.GetPublishedRecord(
            record.Id,
            RecordType.Medicine,
            ct
        );

        var dto = result.ShouldBeSuccess().ShouldBeOfType<PublishedMedicineRecordDto>();
        dto.RecordClinicalTrials.Count.ShouldBe(2);
        var trialA = dto.RecordClinicalTrials.First();
        trialA.StudyName.ShouldBe("Study A");
        trialA.ClinicalTrialsGovNumber.ShouldBe("NCT00000001");
        trialA.BriefDescription.ShouldBe("Description A");
        trialA.RecruitingInUk.ShouldBe(YesNoUnknown.Yes);
        trialA.TrialPhase.ShouldBe(TrialPhase.PhaseIII);
        trialA.OtherClinicalTrialNumbers.ShouldBe(["ISRCTN 1", "EudraCT 2"]);
        var trialB = dto.RecordClinicalTrials.Last();
        trialB.StudyName.ShouldBe("Study B");
        trialB.OtherClinicalTrialNumbers.ShouldBeEmpty();

        var hta = dto.RecordHta.ShouldNotBeNull();
        hta.MedicineHtaSubmissionIntended.ShouldBe(YesNoUnknown.Yes);
        hta.MedicineHtaBodies.ShouldBe([MedicineHtaAssessor.Nice, MedicineHtaAssessor.Awmsg]);
        hta.HtaNiceAlignedPathway.ShouldBe(YesNoUnknown.No);
        hta.HtaAdditionalDetails.ShouldBe("HTA details");

        dto.RecordMhraDate.ShouldBe(
            new RecordMhraDateDto
            {
                UkSubmissionDate = ToDto(ukSubmission),
                UkLicenceDate = ToDto(ukLicence),
                UkLaunchDate = ToDto(ukLaunch),
            }
        );
        dto.RecordMhraProcedure.ShouldBe(
            new RecordMhraProcedureDto
            {
                MhraProcedureType = new ReferenceDataDto { Id = procedureType.Id, Label = "IRP" },
                IrpReferenceRegulator = new ReferenceDataDto { Id = regulator.Id, Label = "FDA" },
                ProcedureDetails = "Procedure details",
            }
        );
    }

    private async Task<Record> AddRecord(
        RecordStatus status,
        RecordType recordType = RecordType.Medicine
    ) =>
        await AddEntity(
            new RecordFaker()
                .RuleFor(x => x.OrganisationId, _organisation.Id)
                .RuleFor(x => x.RecordType, recordType)
                .RuleFor(x => x.RecordStatus, status)
                .RuleFor(x => x.ReviewedAt, _reviewedAt)
                .Generate(),
            TestContext.Current.CancellationToken
        );

    private async Task<RecordRevision> AddRevision(
        Record record,
        int sequence,
        WorkflowStatus workflowStatus
    ) =>
        await AddEntity(
            new RecordRevisionFaker()
                .RuleFor(x => x.RecordId, record.Id)
                .RuleFor(x => x.CreatedBy, _user.Id)
                .RuleFor(x => x.WorkflowStatus, workflowStatus)
                .RuleFor(x => x.CreatedAt, _reviewedAt.AddDays(sequence))
                .Generate(),
            TestContext.Current.CancellationToken
        );

    private async Task<RegulatoryDate> AddDate(
        RecordRevision revision,
        DateEventType dateEvent,
        int month
    ) =>
        await AddEntity(
            new RegulatoryDate
            {
                RevisionId = revision.Id,
                DateEvent = dateEvent,
                DatePrecision = DatePrecision.EstimatedMonth,
                DateValue = new DateOnly(2027, month, 1),
                IsConfidential = true,
                ConditionalApprovalAnticipated = YesNoUnknown.Yes,
            },
            TestContext.Current.CancellationToken
        );

    private static RegulatoryDateDto ToDto(RegulatoryDate date) =>
        new()
        {
            DateValue = date.DateValue,
            DatePrecision = date.DatePrecision,
            IsConfidential = date.IsConfidential,
            ConditionalApprovalAnticipated = date.ConditionalApprovalAnticipated,
        };
}
