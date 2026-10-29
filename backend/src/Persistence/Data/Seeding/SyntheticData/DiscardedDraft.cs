using Bogus;
using UKPS.Api.Persistence.Data.Fakers;
using UKPS.Api.Persistence.Entities.Identity;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Persistence.Data.Seeding.SyntheticData;

/// <summary>
/// Fills in the start of a draft that is later discarded: the product details page in one save,
/// then a few indication answers saved one at a time, auditing each save.
/// </summary>
internal sealed class DiscardedDraft(Faker faker, RecordRevision revision, User author)
{
    private readonly List<RecordFieldAudit> _audits = [];

    /// <summary>Gets the audit of every field saved to the draft.</summary>
    public IReadOnlyList<RecordFieldAudit> Audits => _audits;

    /// <summary>Gets the time of the latest save to the draft.</summary>
    public DateTime LastSavedAt { get; private set; } = revision.CreatedAt;

    /// <summary>Saves the product details page with generated values.</summary>
    public RecordProductDetail SaveProductDetail()
    {
        RecordProductDetail productDetail = new RecordProductDetailFaker(RecordType.Medicine)
            .RuleFor(x => x.Revision, revision)
            .UseSeed(faker.Random.Int())
            .Generate();
        productDetail.NamesAndIdentifiers.Add(
            new RecordNameAndIdentifier
            {
                RecordProductDetail = productDetail,
                Name = faker.Lorem.Word(),
                NameType = NameAndIdentifierType.GenericName,
                DisplayOrder = 1,
            }
        );

        DateTime savedAt = NextSaveTime();
        Audit("record_product_detail.record_title", productDetail.RecordTitle, savedAt);
        Audit("record_product_detail.company_code", productDetail.CompanyCode, savedAt);
        Audit("record_product_detail.branded_name", productDetail.BrandedName, savedAt);
        return productDetail;
    }

    /// <summary>Answers between one and six indication questions, saving each in turn.</summary>
    public MedicinesIndicationDetail SaveIndicationAnswers()
    {
        MedicinesIndicationDetail indicationDetail = new() { Revision = revision };
        IndicationQuestion[] questions =
        [
            new("indication", d => d.Indication = faker.Lorem.Sentence(), d => d.Indication),
            new(
                "indication_is_paediatric",
                d => d.IndicationIsPaediatric = faker.PickRandom<IndicationPaediatricStatus>(),
                d => d.IndicationIsPaediatric?.ToString()
            ),
            new(
                "indication_is_cancer",
                d => d.IndicationIsCancer = faker.PickRandom<YesNoUnknown>(),
                d => d.IndicationIsCancer?.ToString()
            ),
            new(
                "indication_is_rare_disease",
                d => d.IndicationIsRareDisease = faker.PickRandom<YesNoUnknown>(),
                d => d.IndicationIsRareDisease?.ToString()
            ),
            new("presentation", d => d.Presentation = faker.Lorem.Sentence(), d => d.Presentation),
            new(
                "mode_of_action",
                d => d.ModeOfAction = faker.Lorem.Sentence(),
                d => d.ModeOfAction
            ),
            new(
                "proposed_dose_regimen",
                d => d.ProposedDoseRegimen = faker.Lorem.Sentence(),
                d => d.ProposedDoseRegimen
            ),
            new(
                "is_personalised_medicine",
                d => d.IsPersonalisedMedicine = faker.PickRandom<YesNoUnknown>(),
                d => d.IsPersonalisedMedicine?.ToString()
            ),
        ];

        foreach (IndicationQuestion question in faker.PickRandom(questions, faker.Random.Int(1, 6)))
        {
            question.Answer(indicationDetail);
            Audit(
                $"medicines_indication_detail.{question.Field}",
                question.SavedValue(indicationDetail),
                NextSaveTime()
            );
        }
        return indicationDetail;
    }

    private DateTime NextSaveTime()
    {
        LastSavedAt = LastSavedAt.AddMinutes(faker.Random.Int(1, 20));
        return LastSavedAt;
    }

    /// <summary>Audits a field's first save. Fields left empty are not audited.</summary>
    private void Audit(string fieldPath, string? value, DateTime savedAt)
    {
        if (value is null)
        {
            return;
        }

        _audits.Add(
            new RecordFieldAudit
            {
                Record = revision.Record,
                Revision = revision,
                FieldPath = fieldPath,
                OldValue = null,
                NewValue = value,
                ChangedByUser = author,
                ChangedAt = savedAt,
            }
        );
    }

    private sealed record IndicationQuestion(
        string Field,
        Action<MedicinesIndicationDetail> Answer,
        Func<MedicinesIndicationDetail, string?> SavedValue
    );
}
