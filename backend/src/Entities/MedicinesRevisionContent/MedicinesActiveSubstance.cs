using UKPS.Api.Enums;

namespace UKPS.Api.Entities.MedicinesRevisionContent;

internal sealed class MedicinesActiveSubstance
{
    public int Id { get; set; }
    public int MedicinesProductDetailId { get; set; }
    public required string Name { get; set; }
    public SubstanceNameType NameType { get; set; }
    public int? DisplayOrder { get; set; }

    // Navigation
    public MedicinesProductDetail? MedicinesProductDetail { get; set; }
}
