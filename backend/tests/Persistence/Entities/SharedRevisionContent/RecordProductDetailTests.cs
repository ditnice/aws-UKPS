using Shouldly;
using UKPS.Api.Persistence.Entities.SharedRevisionContent;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Tests.Persistence.Entities.SharedRevisionContent;

public sealed class RecordProductDetailTests
{
    [Fact]
    public void GenericNames_ShouldBeInDisplayOrderWithNoneLastThenById()
    {
        RecordProductDetail productDetail = new()
        {
            CompanyCode = "ABC-123",
            RecordTitle = "Title",
            NamesAndIdentifiers =
            [
                Name(1, "no order", NameAndIdentifierType.GenericName, null),
                Name(2, "second", NameAndIdentifierType.GenericName, 2),
                Name(3, "other", NameAndIdentifierType.OtherIdentifier, 1),
                Name(4, "first", NameAndIdentifierType.GenericName, 1),
                Name(5, "second by ID", NameAndIdentifierType.GenericName, 2),
            ],
        };

        productDetail.GenericNames.ShouldBe(["first", "second", "second by ID", "no order"]);
        productDetail.OtherIdentifiers.ShouldBe(["other"]);
    }

    private static RecordNameAndIdentifier Name(
        int id,
        string name,
        NameAndIdentifierType type,
        int? displayOrder
    ) =>
        new()
        {
            Id = id,
            Name = name,
            NameType = type,
            DisplayOrder = displayOrder,
        };
}
