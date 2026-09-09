namespace UKPS.Api.Persistence.Enums;

/// <summary>
/// Bodies a medicine may be submitted to for health technology assessment.
/// Multi-select — see MedicinesHtaBody.
/// </summary>
public enum MedicineHtaAssessor
{
    /// <summary>National Institute for Health and Care Excellence.</summary>
    Nice = 0,

    /// <summary>Scottish Medicines Consortium.</summary>
    Smc = 1,

    /// <summary>All Wales Medicines Strategy Group.</summary>
    Awmsg = 2,

    /// <summary>No HTA submission is planned.</summary>
    NotPlanningHtaSubmission = 3,
}
