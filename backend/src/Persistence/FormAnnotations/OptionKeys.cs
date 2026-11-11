namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// Names of the option providers registered in <c>Application/RecordForms/Options</c>. Plain
/// strings so entities can refer to providers without depending on the Application layer.
/// </summary>
/// <remarks>
/// A question finds its provider by type, so it needs a key only to choose one of several
/// providers of the same type: pass it to <see cref="FormOptionsAttribute"/>. Each key must be
/// registered exactly once, and startup rejects a question whose key is unknown or whose provider
/// does not suit it. A key is
/// part of the form fingerprint, so renaming one, or changing its provider's definition, needs a
/// form version bump. By convention, keys are lowercase slugs.
/// </remarks>
internal static class OptionKeys
{
    /// <summary>
    /// The <c>YesNoUnknown</c> enum: Yes, No and Unknown at this stage. For a
    /// <see cref="FormControl.Radio"/> or <see cref="FormControl.Select"/> on a
    /// <c>YesNoUnknown?</c> property.
    /// </summary>
    public const string YesNoUnknown = "yes-no-unknown";

    /// <summary>
    /// The <c>IndicationPaediatricStatus</c> enum: exclusively for children, for both children
    /// and adults, exclusively for adults, and unknown at this stage. For a
    /// <see cref="FormControl.Radio"/> or <see cref="FormControl.Select"/> on an
    /// <c>IndicationPaediatricStatus?</c> property.
    /// </summary>
    public const string PaediatricStatus = "paediatric-status";

    /// <summary>
    /// The <c>BnfChapter</c> reference data as a two-level tree of chapters and their sections,
    /// labelled <c>"{Code}: {Label}"</c> and ordered by display order. A tree provider, so only
    /// for a <see cref="FormControl.HierarchicalSelect"/> on a <c>BnfChapterId</c> property.
    /// Archived options are hidden unless already saved.
    /// </summary>
    public const string BnfChapters = "bnf-chapters";

    /// <summary>
    /// The <c>TherapeuticArea</c> reference data as a two-level tree of areas and their diseases
    /// or conditions, labelled <c>"{Code}: {Label}"</c> and ordered by display order. A tree
    /// provider, so only for a <see cref="FormControl.HierarchicalSelect"/> on a therapeutic area
    /// ID or junction. Archived options are hidden unless already saved.
    /// </summary>
    public const string TherapeuticAreas = "therapeutic-areas";

    /// <summary>
    /// The <c>BiomarkerType</c> enum: genomic biomarker, non-genomic biomarker and unknown at this
    /// stage. For a <see cref="FormControl.Radio"/> or <see cref="FormControl.Select"/> on a
    /// <c>BiomarkerType?</c> property.
    /// </summary>
    public const string BiomarkerType = "biomarker-type";

    /// <summary>
    /// The <c>GenomicTestNgtdRelationship</c> enum: how a test relates to the National Genomic Test
    /// Directory (new test, existing test with a new or the same indication, or unknown at this
    /// stage). For a <see cref="FormControl.Radio"/> or <see cref="FormControl.Select"/> on a
    /// <c>GenomicTestNgtdRelationship?</c> property.
    /// </summary>
    public const string GenomicTestNgtdRelationship = "genomic-test-ngtd-relationship";

    /// <summary>
    /// The <c>GenomicTestMandatoryStatus</c> enum: whether testing is mandatory before treatment
    /// can start (mandatory with or without alternatives, recommended only, or unknown at this
    /// stage). For a <see cref="FormControl.Radio"/> or <see cref="FormControl.Select"/> on a
    /// <c>GenomicTestMandatoryStatus?</c> property.
    /// </summary>
    public const string GenomicTestMandatoryStatus = "genomic-test-mandatory-status";

    /// <summary>
    /// The <c>PatientPathwayPoint</c> reference data as a flat list, ordered by ID. For a
    /// <see cref="FormControl.Radio"/> or <see cref="FormControl.Select"/> on a
    /// <c>PatientPathwayPointId</c> property. Archived options are hidden unless already saved.
    /// </summary>
    public const string PatientPathwayPoints = "patient-pathway-points";

    /// <summary>
    /// The <c>FormulationType</c> reference data as a flat list, ordered by ID. For a
    /// <see cref="FormControl.Radio"/> or <see cref="FormControl.Select"/> on a
    /// <c>FormulationTypeId</c> property. Archived options are hidden unless already saved.
    /// </summary>
    public const string FormulationTypes = "formulation-types";

    /// <summary>
    /// The <c>MedicineTechnologyStatus</c> flags enum: biosimilar, new chemical or biological
    /// entity, new dosing regimen, new formulation, new indication, new presentation, and SPC
    /// amendment without indication change. For a <see cref="FormControl.Checkbox"/> on a
    /// <c>MedicineTechnologyStatus?</c> property.
    /// </summary>
    public const string MedicineTechnologyStatus = "medicine-technology-status";

    /// <summary>
    /// The <c>MedicineHtaAssessor</c> flags enum: NICE, SMC and AWMSG. For a
    /// <see cref="FormControl.Checkbox"/> on a <c>MedicineHtaAssessor?</c> property.
    /// </summary>
    public const string MedicineHtaAssessor = "medicine-hta-assessor";

    /// <summary>
    /// The <c>DesignationStatus</c> enum worded for MHRA PIM designation: granted, not granted,
    /// decision to submit is ongoing, application submitted and decision pending, and no
    /// submission intended. For a <see cref="FormControl.Radio"/> on a
    /// <c>DesignationStatus?</c> property. Other designations that share the enum need their own
    /// key, so name this one with <c>[FormOptions]</c>.
    /// </summary>
    public const string PimDesignationStatus = "pim-designation-status";

    /// <summary>
    /// The <c>EamsOpinionDecision</c> enum: positive and negative. For a
    /// <see cref="FormControl.Radio"/> on an <c>EamsOpinionDecision?</c> property.
    /// </summary>
    public const string EamsOpinionDecision = "eams-opinion-decision";

    /// <summary>
    /// The <c>NhsServiceChangesRequired</c> enum: complete transformation, some change, no
    /// changes and unknown at this stage, with a hint on all but the last. For a
    /// <see cref="FormControl.Radio"/> or <see cref="FormControl.Select"/> on a
    /// <c>NhsServiceChangesRequired?</c> property.
    /// </summary>
    public const string NhsServiceChangesRequired = "nhs-service-changes-required";

    /// <summary>
    /// The <c>UkPatientPopulationRange</c> reference data as a flat list, ordered by sort order.
    /// For a <see cref="FormControl.Radio"/> or <see cref="FormControl.Select"/> on a
    /// <c>UkPatientPopulationRangeId</c> property. Archived options are hidden unless already saved.
    /// </summary>
    public const string UkPatientPopulationRanges = "uk-patient-population-ranges";
}
