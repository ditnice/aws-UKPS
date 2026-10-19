namespace UKPS.Api.Persistence.FormAnnotations;

/// <summary>
/// Names of the option providers registered in <c>Application/RecordForms/Options</c>. Plain
/// strings so entities can refer to providers without depending on the Application layer.
/// </summary>
internal static class OptionKeys
{
    public const string YesNoUnknown = "yes-no-unknown";
    public const string PaediatricStatus = "paediatric-status";
    public const string BnfChapters = "bnf-chapters";
    public const string TherapeuticAreas = "therapeutic-areas";
}
