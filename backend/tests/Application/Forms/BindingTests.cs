using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using UKPS.Api.Application.Forms.Bindings;
using UKPS.Api.Persistence.Entities.MedicinesRevisionContent;
using UKPS.Api.Persistence.Entities.ReferenceData;
using UKPS.Api.Persistence.Enums;
using UKPS.Api.Tests.Utilities.Data;
using UKPS.Api.Tests.Utilities.Fixtures;

namespace UKPS.Api.Tests.Application.Forms;

[Collection(DatabaseCollection.Name)]
public class BindingTests(PostgresFixture fixture) : DatabaseTestBase(fixture)
{
    private static readonly IQuestionBinding _indication = Bind.Column(
        (MedicinesProductDetail x) => x.Indication
    );

    private static readonly IQuestionBinding _isCancer = Bind.Column(
        (MedicinesProductDetail x) => x.IndicationIsCancer
    );

    private static readonly IQuestionBinding _bnfChapter = Bind.Column(
        (MedicinesProductDetail x) => x.BnfChapterId
    );

    private static readonly IQuestionBinding _therapeuticAreas = Bind.Junction<
        MedicinesProductDetail,
        MedicinesProductDetailTherapeuticArea
    >(x => x.MedicinesProductDetailId, x => x.TherapeuticAreaId);

    private static readonly string[] _areaLabels = ["Addiction", "Allergy", "Oncology"];

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private async Task<JsonNode?> ReadFreshAsync(IQuestionBinding binding, int revisionId)
    {
        await using var context = Fixture.CreateContext();
        return await binding.ReadAsync(new BindingContext(context, revisionId), Ct);
    }

    private async Task WriteAndSaveAsync(IQuestionBinding binding, int revisionId, JsonNode? value)
    {
        await using var context = Fixture.CreateContext();
        await binding.WriteAsync(new BindingContext(context, revisionId), value, Ct);
        await context.SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task Column_Text_RoundTrips()
    {
        var record = await MedicineRecordTestData.CreateAsync(Context, Ct);

        (await ReadFreshAsync(_indication, record.Revision.Id)).ShouldBeNull();

        await WriteAndSaveAsync(_indication, record.Revision.Id, JsonValue.Create("Hepatitis C"));

        (await ReadFreshAsync(_indication, record.Revision.Id))!
            .GetValue<string>()
            .ShouldBe("Hepatitis C");
    }

    [Fact]
    public async Task Column_Enum_IsStoredAsTheEnumAndReadAsItsName()
    {
        var record = await MedicineRecordTestData.CreateAsync(Context, Ct);

        await WriteAndSaveAsync(_isCancer, record.Revision.Id, JsonValue.Create("Unknown"));

        await using var context = Fixture.CreateContext();
        var stored = await context.MedicinesProductDetails.SingleAsync(
            x => x.RevisionId == record.Revision.Id,
            Ct
        );
        stored.IndicationIsCancer.ShouldBe(YesNoUnknown.Unknown);
        (await ReadFreshAsync(_isCancer, record.Revision.Id))!
            .GetValue<string>()
            .ShouldBe("Unknown");
    }

    [Fact]
    public async Task Column_ReferenceId_IsReadAsAString()
    {
        var chapter = await AddEntity(
            new BnfChapter { Code = "1", Label = "Gastro-intestinal" },
            Ct
        );
        var record = await MedicineRecordTestData.CreateAsync(Context, Ct);

        await WriteAndSaveAsync(_bnfChapter, record.Revision.Id, JsonValue.Create($"{chapter.Id}"));

        (await ReadFreshAsync(_bnfChapter, record.Revision.Id))!
            .GetValue<string>()
            .ShouldBe($"{chapter.Id}");
    }

    [Fact]
    public async Task Column_WritingNull_ClearsTheValue()
    {
        var record = await MedicineRecordTestData.CreateAsync(Context, Ct);
        await WriteAndSaveAsync(_isCancer, record.Revision.Id, JsonValue.Create("Yes"));

        await WriteAndSaveAsync(_isCancer, record.Revision.Id, null);

        (await ReadFreshAsync(_isCancer, record.Revision.Id)).ShouldBeNull();
    }

    [Fact]
    public async Task Column_MissingContentRow_Throws()
    {
        await Should.ThrowAsync<InvalidOperationException>(() =>
            ReadFreshAsync(_indication, 999_999)
        );
    }

    [Fact]
    public async Task Junction_InsertsAndDeletesOnlyWhatChanged()
    {
        var areas = _areaLabels.Select(label => new TherapeuticArea { Label = label }).ToList();
        await AddEntities(areas, Ct);
        var record = await MedicineRecordTestData.CreateAsync(Context, Ct);
        var ids = areas.Select(area => $"{area.Id}").ToList();

        (await ReadFreshAsync(_therapeuticAreas, record.Revision.Id))!.AsArray().ShouldBeEmpty();

        await WriteAndSaveAsync(
            _therapeuticAreas,
            record.Revision.Id,
            new JsonArray(ids[0], ids[1])
        );
        await WriteAndSaveAsync(
            _therapeuticAreas,
            record.Revision.Id,
            new JsonArray(ids[2], ids[1])
        );

        var stored = await ReadFreshAsync(_therapeuticAreas, record.Revision.Id);
        stored!.AsArray().Select(node => node!.GetValue<string>()).ShouldBe([ids[1], ids[2]]);

        await using var context = Fixture.CreateContext();
        (
            await context.MedicinesProductDetailTherapeuticAreas.CountAsync(
                x => x.MedicinesProductDetailId == record.ProductDetail.Id,
                Ct
            )
        ).ShouldBe(2);
    }

    [Fact]
    public async Task Junction_WritingEmpty_RemovesAllRows()
    {
        var area = await AddEntity(new TherapeuticArea { Label = "Allergy" }, Ct);
        var record = await MedicineRecordTestData.CreateAsync(Context, Ct);
        await WriteAndSaveAsync(_therapeuticAreas, record.Revision.Id, new JsonArray($"{area.Id}"));

        await WriteAndSaveAsync(_therapeuticAreas, record.Revision.Id, new JsonArray());

        (await ReadFreshAsync(_therapeuticAreas, record.Revision.Id))!.AsArray().ShouldBeEmpty();
    }
}
