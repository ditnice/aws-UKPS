using Microsoft.EntityFrameworkCore;
using UKPS.Api.Persistence.Entities.Email;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Entities.Reporting;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Entities.UserFeatures;
using UKPS.Api.Persistence.Entities.VaccinesRevisionContent;

namespace UKPS.Api.Persistence;

internal sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // ── Identity & Access Management ────────────────────────────────────────
    public DbSet<Organisation> Organisations => Set<Organisation>();
    public DbSet<TermsAcceptance> TermsAcceptances => Set<TermsAcceptance>();
    public DbSet<OrganisationAudit> OrganisationAudits => Set<OrganisationAudit>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserOrgMembership> UserOrgMemberships => Set<UserOrgMembership>();
    public DbSet<UserAudit> UserAudits => Set<UserAudit>();

    // ── Record Core Workflow ─────────────────────────────────────────────────
    public DbSet<Record> Records => Set<Record>();
    public DbSet<RecordRevision> RecordRevisions => Set<RecordRevision>();
    public DbSet<QaReview> QaReviews => Set<QaReview>();
    public DbSet<QaReviewItem> QaReviewItems => Set<QaReviewItem>();
    public DbSet<RecordStatusHistory> RecordStatusHistories => Set<RecordStatusHistory>();
    public DbSet<RecordEvent> RecordEvents => Set<RecordEvent>();
    public DbSet<RecordEventFieldChange> RecordEventFieldChanges => Set<RecordEventFieldChange>();

    // ── Shared Revision Content ──────────────────────────────────────────────
    public DbSet<RegulatoryDate> RegulatoryDates => Set<RegulatoryDate>();
    public DbSet<RecordMhraProcedure> RecordMhraProcedures => Set<RecordMhraProcedure>();
    public DbSet<RecordMhraDate> RecordMhraDates => Set<RecordMhraDate>();
    public DbSet<RecordIntlRecognition> RecordIntlRecognitions => Set<RecordIntlRecognition>();
    public DbSet<RecordGlobalSubmission> RecordGlobalSubmissions => Set<RecordGlobalSubmission>();
    public DbSet<RecordHta> RecordHtas => Set<RecordHta>();
    public DbSet<RecordHtaBody> RecordHtaBodies => Set<RecordHtaBody>();
    public DbSet<RecordClinicalTrial> RecordClinicalTrials => Set<RecordClinicalTrial>();
    public DbSet<OtherClinicalTrialNumber> OtherClinicalTrialNumbers =>
        Set<OtherClinicalTrialNumber>();

    // ── Medicines Revision Content ───────────────────────────────────────────
    public DbSet<MedicinesProductDetail> MedicinesProductDetails => Set<MedicinesProductDetail>();
    public DbSet<MedicinesActiveSubstance> MedicinesActiveSubstances =>
        Set<MedicinesActiveSubstance>();
    public DbSet<MedicinesRecordStatus> MedicinesRecordStatuses => Set<MedicinesRecordStatus>();
    public DbSet<MedicinesCompanyInfo> MedicinesCompanyInfos => Set<MedicinesCompanyInfo>();
    public DbSet<MedicinesDetail> MedicinesDetails => Set<MedicinesDetail>();
    public DbSet<MedicinesEamsPim> MedicinesEamsPims => Set<MedicinesEamsPim>();
    public DbSet<MedicinesEuStatus> MedicinesEuStatuses => Set<MedicinesEuStatus>();
    public DbSet<MedicinesPatientIdentification> MedicinesPatientIdentifications =>
        Set<MedicinesPatientIdentification>();
    public DbSet<MedicinesLaboratoryTesting> MedicinesLaboratoryTestings =>
        Set<MedicinesLaboratoryTesting>();
    public DbSet<MedicinesTreatmentDetail> MedicinesTreatmentDetails =>
        Set<MedicinesTreatmentDetail>();
    public DbSet<MedicinesServiceImpact> MedicinesServiceImpacts => Set<MedicinesServiceImpact>();
    public DbSet<MedicinesBudgetImpact> MedicinesBudgetImpacts => Set<MedicinesBudgetImpact>();
    public DbSet<MedicinesPasRegion> MedicinesPasRegions => Set<MedicinesPasRegion>();

    // ── Vaccines Revision Content ────────────────────────────────────────────
    public DbSet<VaccinesProductDetail> VaccinesProductDetails => Set<VaccinesProductDetail>();
    public DbSet<VaccinesCompanyCode> VaccinesCompanyCodes => Set<VaccinesCompanyCode>();
    public DbSet<VaccinesCompanyInfo> VaccinesCompanyInfos => Set<VaccinesCompanyInfo>();
    public DbSet<VaccinesDiseaseDetail> VaccinesDiseaseDetails => Set<VaccinesDiseaseDetail>();
    public DbSet<VaccinesPathogen> VaccinesPathogens => Set<VaccinesPathogen>();
    public DbSet<VaccinesTechnology> VaccinesTechnologies => Set<VaccinesTechnology>();
    public DbSet<VaccinesAntigen> VaccinesAntigens => Set<VaccinesAntigen>();
    public DbSet<VaccinesAdjuvant> VaccinesAdjuvants => Set<VaccinesAdjuvant>();
    public DbSet<VaccinesServiceReadiness> VaccinesServiceReadinesses =>
        Set<VaccinesServiceReadiness>();
    public DbSet<VaccinesPopulation> VaccinesPopulations => Set<VaccinesPopulation>();

    // ── Reference Data ───────────────────────────────────────────────────────
    public DbSet<FormulationType> FormulationTypes => Set<FormulationType>();
    public DbSet<MedicineTechnologyStatus> MedicineTechnologyStatuses =>
        Set<MedicineTechnologyStatus>();
    public DbSet<MhraProcedureType> MhraProcedureTypes => Set<MhraProcedureType>();
    public DbSet<IrpReferenceRegulator> IrpReferenceRegulators => Set<IrpReferenceRegulator>();
    public DbSet<IrpRoute> IrpRoutes => Set<IrpRoute>();
    public DbSet<AtmpClassification> AtmpClassifications => Set<AtmpClassification>();
    public DbSet<GenomicSampleType> GenomicSampleTypes => Set<GenomicSampleType>();
    public DbSet<PatientPathwayPoint> PatientPathwayPoints => Set<PatientPathwayPoint>();
    public DbSet<UkPatientPopulationRange> UkPatientPopulationRanges =>
        Set<UkPatientPopulationRange>();
    public DbSet<PasRegion> PasRegions => Set<PasRegion>();
    public DbSet<VaccineAdministrationRoute> VaccineAdministrationRoutes =>
        Set<VaccineAdministrationRoute>();
    public DbSet<VaccineDiseaseArea> VaccineDiseaseAreas => Set<VaccineDiseaseArea>();
    public DbSet<VaccineStorageRequirement> VaccineStorageRequirements =>
        Set<VaccineStorageRequirement>();
    public DbSet<VaccinePlatform> VaccinePlatforms => Set<VaccinePlatform>();
    public DbSet<BnfChapter> BnfChapters => Set<BnfChapter>();
    public DbSet<TherapeuticArea> TherapeuticAreas => Set<TherapeuticArea>();

    // ── Reporting & Email ────────────────────────────────────────────────────
    public DbSet<ReportPreset> ReportPresets => Set<ReportPreset>();
    public DbSet<ReportAudit> ReportAudits => Set<ReportAudit>();

    // ── Email ────────────────────────────────────────────────────────────────
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<EmailAudit> EmailAudits => Set<EmailAudit>();

    // ── User Features ────────────────────────────────────────────────────────
    public DbSet<RecordWatchlist> RecordWatchlists => Set<RecordWatchlist>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("ukps");
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
