using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.ReferenceData;

/// <summary>
/// 6 values — shared. Access Consortium and Orbis relevant to medicines only.
/// </summary>
internal sealed class MhraProcedureType : ReferenceDataBase
{
    /// <summary>
    /// A stable key for the procedure that never changes with the label, so form conditions can
    /// name it. One of <see cref="MhraProcedureTypeCodes"/>.
    /// </summary>
    public required string Code { get; set; }

    public ReferenceDataType? RelevantTo { get; set; }
}
