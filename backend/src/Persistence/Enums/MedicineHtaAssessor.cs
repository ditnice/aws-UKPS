namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Bodies a medicine may be submitted to for health technology assessment.
/// Multi-select — see RecordHta.MedicineHtaBodies. Only populated when
/// RecordHta.MedicineHtaSubmissionIntended is Yes.
/// </summary>
[Flags]
public enum MedicineHtaAssessor
{
    /// <summary>National Institute for Health and Care Excellence.</summary>
    Nice = 1,

    /// <summary>Scottish Medicines Consortium.</summary>
    Smc = 2,

    /// <summary>All Wales Medicines Strategy Group.</summary>
    Awmsg = 4,
}
