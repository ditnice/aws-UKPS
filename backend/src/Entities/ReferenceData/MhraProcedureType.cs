using UKPS.Data.Enums;

namespace UKPS.Data.Entities.ReferenceData;

/// <summary>
/// 6 values — shared. Access Consortium and Orbis relevant to medicines only.
/// </summary>
public class MhraProcedureType : ReferenceDataBase
{
    public ReferenceDataType? RelevantTo { get; set; }
}
