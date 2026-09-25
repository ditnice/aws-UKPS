namespace UKPS.Api.Application.Records.Dtos.PublishedRecord;

/// <summary>
/// Represents the data held on the latest published revision of a medicine record.
/// </summary>
/// <remarks>
/// Each section property corresponds to a revision data table, so a field's JSON path maps to its
/// field path, e.g. <c>medicinesProductDetail.indication</c> is
/// <c>medicines_product_detail.indication</c>. A section is <c>null</c> when the revision has no
/// data for it.
/// </remarks>
public sealed record PublishedMedicineRecordDto : PublishedRecordDto
{
    /// <summary>
    /// Gets the product detail section.
    /// </summary>
    public MedicinesProductDetailDto? MedicinesProductDetail { get; init; }

    /// <summary>
    /// Gets the medicine detail section.
    /// </summary>
    public MedicinesDetailDto? MedicinesDetail { get; init; }

    /// <summary>
    /// Gets the company information section.
    /// </summary>
    public MedicinesCompanyInfoDto? MedicinesCompanyInfo { get; init; }

    /// <summary>
    /// Gets the treatment detail section.
    /// </summary>
    public MedicinesTreatmentDetailDto? MedicinesTreatmentDetail { get; init; }

    /// <summary>
    /// Gets the patient identification section.
    /// </summary>
    public MedicinesPatientIdentificationDto? MedicinesPatientIdentification { get; init; }

    /// <summary>
    /// Gets the laboratory testing section.
    /// </summary>
    public MedicinesLaboratoryTestingDto? MedicinesLaboratoryTesting { get; init; }

    /// <summary>
    /// Gets the NHS service impact section.
    /// </summary>
    public MedicinesServiceImpactDto? MedicinesServiceImpact { get; init; }

    /// <summary>
    /// Gets the budget impact section.
    /// </summary>
    public MedicinesBudgetImpactDto? MedicinesBudgetImpact { get; init; }

    /// <summary>
    /// Gets the EAMS and PIM section.
    /// </summary>
    public MedicinesEamsPimDto? MedicinesEamsPim { get; init; }

    /// <summary>
    /// Gets the EU status section.
    /// </summary>
    public MedicinesEuStatusDto? MedicinesEuStatus { get; init; }

    /// <summary>
    /// Gets the global submission section.
    /// </summary>
    public MedicinesGlobalSubmissionDto? MedicinesGlobalSubmission { get; init; }

    /// <summary>
    /// Gets the international recognition section.
    /// </summary>
    public MedicinesIntlRecognitionDto? MedicinesIntlRecognition { get; init; }

    /// <summary>
    /// Gets the clinical trials.
    /// </summary>
    public required IReadOnlyCollection<RecordClinicalTrialDto> RecordClinicalTrials { get; init; }

    /// <summary>
    /// Gets the health technology assessment section.
    /// </summary>
    public RecordHtaDto? RecordHta { get; init; }

    /// <summary>
    /// Gets the MHRA dates section.
    /// </summary>
    public RecordMhraDateDto? RecordMhraDate { get; init; }

    /// <summary>
    /// Gets the MHRA procedure section.
    /// </summary>
    public RecordMhraProcedureDto? RecordMhraProcedure { get; init; }
}
