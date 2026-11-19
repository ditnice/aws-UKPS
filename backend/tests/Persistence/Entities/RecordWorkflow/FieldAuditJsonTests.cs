using Shouldly;
using UKPS.Api.Persistence.Entities.RecordWorkflow;

namespace UKPS.Api.Tests.Persistence.Entities.RecordWorkflow;

public sealed class FieldAuditJsonTests
{
    [Fact]
    public void TextList_ShouldBeTheEntriesInOrderUnescaped()
    {
        FieldAuditJson
            .TextList(["sofosbuvir", "Mölnlycke", "A+B's <C>"])
            .ShouldBe("""["sofosbuvir","Mölnlycke","A+B's <C>"]""");
    }

    [Fact]
    public void TextList_WithNoEntries_ShouldBeNull()
    {
        FieldAuditJson.TextList([]).ShouldBeNull();
    }
}
