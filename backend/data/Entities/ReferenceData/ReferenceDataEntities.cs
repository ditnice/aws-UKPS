using UKPS.Data.Enums;

namespace UKPS.Data.Entities.ReferenceData;

public abstract class ReferenceDataBase
{
    public int Id { get; set; }
    public string Label { get; set; } = null!;
    public bool IsArchived { get; set; }
}

/// <summary>60 values — medicines only.</summary>
public class FormulationType : ReferenceDataBase { }

/// <summary>
/// 7 values — medicines only:
/// Biosimilar, New chemical / biological entity, New dosing regimen,
/// New formulation, New indication, New presentation,
/// SPC amendment without indication change.
/// </summary>
public class MedicineTechnologyStatus : ReferenceDataBase { }

/// <summary>
/// 6 values — shared. Access Consortium and Orbis relevant to medicines only.
/// </summary>
public class MhraProcedureType : ReferenceDataBase
{
    public ReferenceDataType? RelevantTo { get; set; }
}

/// <summary>
/// 7 values — shared:
/// European Union, United States, Australia, Canada, Singapore, Japan, Switzerland.
/// </summary>
public class IrpReferenceRegulator : ReferenceDataBase { }

/// <summary>
/// 4 values — medicines only:
/// Route A, Route B, Not yet confirmed, Not applicable.
/// </summary>
public class IrpRoute : ReferenceDataBase { }

/// <summary>
/// 5 values — medicines only:
/// Ex-vivo gene therapy medicine, In-vivo gene therapy medicine,
/// Somatic-cell therapy medicine, Tissue-engineered medicine, Combined ATMP.
/// </summary>
public class AtmpClassification : ReferenceDataBase { }

/// <summary>
/// 4 values — medicines only:
/// Tissue, Liquid, Any sample, Other.
/// </summary>
public class GenomicSampleType : ReferenceDataBase { }

/// <summary>
/// 4 values — medicines only:
/// Initial presentation, Progression, Longitudinal testing, Other.
/// </summary>
public class PatientPathwayPoint : ReferenceDataBase { }

/// <summary>13 values — medicines only.</summary>
public class UkPatientPopulationRange : ReferenceDataBase
{
    public int SortOrder { get; set; }
}

/// <summary>
/// 4 values — medicines only:
/// England, Wales, Scotland, Northern Ireland.
/// </summary>
public class PasRegion : ReferenceDataBase { }

/// <summary>Values TBC — vaccines only.</summary>
public class VaccineAdministrationRoute : ReferenceDataBase { }

/// <summary>Values TBC — vaccines only.</summary>
public class VaccineDiseaseArea : ReferenceDataBase { }

/// <summary>
/// Values TBC — vaccines only.
/// e.g. Refrigerated (2-8°C), Frozen (-15°C to -50°C),
/// Ultra-cold (-60°C to -80°C), Ambient, Not yet confirmed.
/// </summary>
public class VaccineStorageRequirement : ReferenceDataBase { }

/// <summary>Values TBC — vaccines only. e.g. mRNA, Viral vector, Live attenuated, Inactivated, Other.</summary>
public class VaccinePlatform : ReferenceDataBase { }

/// <summary>
/// BNF chapter hierarchy — medicines only.
/// 15 top-level chapters, 125 total nodes including subsections.
/// Self-referencing for arbitrary depth.
/// </summary>
public class BnfChapter
{
    public int Id { get; set; }

    /// <summary>Null for top-level chapters.</summary>
    public int? ParentId { get; set; }

    /// <summary>e.g. "1", "1.1", "2.5"</summary>
    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;
    public int? DisplayOrder { get; set; }
    public bool IsArchived { get; set; }

    // Navigation
    public BnfChapter? Parent { get; set; }
    public ICollection<BnfChapter> Children { get; set; } = [];
}

/// <summary>
/// Therapeutic area hierarchy — medicines only. Currently flat; supports future sub-levels.
/// 20 values.
/// </summary>
public class TherapeuticArea
{
    public int Id { get; set; }

    /// <summary>Null for top-level areas. Currently flat; structure supports future sub-levels.</summary>
    public int? ParentId { get; set; }

    public string Label { get; set; } = null!;
    public int? DisplayOrder { get; set; }
    public bool IsArchived { get; set; }

    // Navigation
    public TherapeuticArea? Parent { get; set; }
    public ICollection<TherapeuticArea> Children { get; set; } = [];
}
