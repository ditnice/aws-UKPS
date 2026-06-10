using UKPS.Data.Enums;

namespace UKPS.Data.Entities.MedicinesRevisionContent;

public class MedicinesActiveSubstance
{
    public int Id { get; set; }
    public int MedicinesProductDetailId { get; set; }
    public string Name { get; set; } = null!;
    public SubstanceNameType NameType { get; set; }
    public int? DisplayOrder { get; set; }

    // Navigation
    public MedicinesProductDetail MedicinesProductDetail { get; set; } = null!;
}
