using Shouldly;
using UKPS.Api.Application.Forms;
using UKPS.Api.Application.Forms.Definitions;
using UKPS.Api.Application.Forms.Medicines;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Tests.Application.Forms;

public class MedicineFormDefinitionTests
{
    /// <summary>
    /// Pinned with <see cref="MedicineFormDefinition.Version"/>. If this test fails you have
    /// changed the definition: bump <see cref="MedicineFormDefinition.Version"/>, then update
    /// both constants here.
    /// </summary>
    private const string ExpectedVersion = "2026.10.2";

    private const string ExpectedFingerprint =
        "ce1df37cc8d7501e3a466fc031d6632002c8285f61a749308a819a0126ee98af";

    private readonly FormDefinition _form = MedicineFormDefinition.Create();

    [Fact]
    public void Definition_IsValid() => FormDefinitionValidator.Validate(_form).ShouldBeEmpty();

    [Fact]
    public void Definition_HasTheIndicationDetailsPagesInOrder()
    {
        _form
            .Pages.Select(page => page.Id)
            .ShouldBe(["indication", "bnf-chapter", "therapeutic-area", "paediatric", "cancer"]);
        _form.Pages.ShouldAllBe(page => page.SectionTitle == "Indication details");
    }

    [Fact]
    public void Definition_ChangesRequireAVersionBump()
    {
        var fingerprint = FormDefinitionFingerprint.Compute(_form);

        (_form.Version, fingerprint).ShouldBe(
            (ExpectedVersion, ExpectedFingerprint),
            "The medicine form definition changed. Bump MedicineFormDefinition.Version and update ExpectedVersion and ExpectedFingerprint."
        );
    }

    [Fact]
    public void NextAndPreviousPage_FollowFormOrder()
    {
        var first = _form.FirstPage;
        var last = _form.Pages[^1];

        _form.PreviousPage(first).ShouldBeNull();
        _form.NextPage(first)!.Id.ShouldBe("bnf-chapter");
        _form.PreviousPage(last)!.Id.ShouldBe("paediatric");
        _form.NextPage(last).ShouldBeNull();
    }

    [Fact]
    public void Registry_ServesTheMedicineFormOnly()
    {
        var registry = FormDefinitionRegistry.CreateDefault();

        registry.Get(RecordType.Medicine)!.Version.ShouldBe(MedicineFormDefinition.Version);
        registry.Get(RecordType.Vaccine).ShouldBeNull();
    }
}
