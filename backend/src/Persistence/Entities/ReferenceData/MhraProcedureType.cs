using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Entities.ReferenceData;

/// <summary>
/// 6 values — shared. Access Consortium and Project Orbis relevant to medicines only.
/// </summary>
internal sealed class MhraProcedureType : ReferenceDataBase
{
    public ReferenceDataType? RelevantTo { get; set; }
}
