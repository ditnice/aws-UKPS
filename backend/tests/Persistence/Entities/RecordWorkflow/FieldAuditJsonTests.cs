using System.Text.Json;
using Shouldly;
using UKPS.Api.Persistence.Entities.RecordWorkflow;
using UKPS.Api.Persistence.Enums;

namespace UKPS.Api.Tests.Persistence.Entities.RecordWorkflow;

public sealed class FieldAuditJsonTests
{
    [Fact]
    public void NamesAndIdentifiers_ShouldHaveAKeyForEveryNameAndIdentifierTypeInEnumOrder()
    {
        string text = FieldAuditJson.NamesAndIdentifiers(["sofosbuvir"], ["PSI-7977"]);

        using JsonDocument document = JsonDocument.Parse(text);
        document
            .RootElement.EnumerateObject()
            .Select(static property => property.Name)
            .ShouldBe(Enum.GetNames<NameAndIdentifierType>());
    }
}
