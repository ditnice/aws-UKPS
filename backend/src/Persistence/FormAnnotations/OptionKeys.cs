namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// Names of the option providers registered in <c>Application/RecordForms/Options</c>. Plain
/// strings so entities can refer to providers without depending on the Application layer.
/// </summary>
/// <remarks>
/// Pass one to <see cref="FormOptionsAttribute"/>. Each key must be registered exactly once, and
/// startup rejects a question whose key is unknown or whose provider does not suit it. A key is
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
}
