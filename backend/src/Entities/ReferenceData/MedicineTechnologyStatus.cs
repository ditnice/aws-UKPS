using UKPS.Api.Enums;

namespace UKPS.Api.Entities.ReferenceData;

/// <summary>
/// 7 values — medicines only:
/// Biosimilar, New chemical / biological entity, New dosing regimen,
/// New formulation, New indication, New presentation,
/// SPC amendment without indication change.
/// </summary>
internal sealed class MedicineTechnologyStatus : ReferenceDataBase { }
