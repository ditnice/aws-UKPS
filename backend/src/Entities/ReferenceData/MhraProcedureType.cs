using UKPS.Api.Enums;

namespace UKPS.Api.Entities.ReferenceData;

/// <summary>
/// 6 values — shared. Access Consortium and Orbis relevant to medicines only.
/// </summary>
public class MhraProcedureType : ReferenceDataBase
{
    public ReferenceDataType? RelevantTo { get; set; }
}
