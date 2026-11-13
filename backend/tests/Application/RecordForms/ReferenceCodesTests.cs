using Shouldly;
using UKPS.Api.Application.RecordForms.Conditions;
using UKPS.Api.Persistence.Entities.ReferenceData;

namespace UKPS.Api.Tests.Application.RecordForms;

/// <summary>
/// Translating between reference data IDs, which answers hold, and the codes conditions name.
/// </summary>
public class ReferenceCodesTests
{
    private readonly ReferenceCodes _codes = new(
        new Dictionary<Type, IReadOnlyList<(int Id, string Code)>>
        {
            [typeof(MhraProcedureType)] =
            [
                (7, MhraProcedureTypeCodes.InternationalRecognition),
                (8, MhraProcedureTypeCodes.National),
            ],
        }
    );

    [Fact]
    public void CodesOf_TranslatesIds_AndLeavesOutUnknownOnes()
    {
        _codes
            .CodesOf(typeof(MhraProcedureType), ["8", "9", "7"])
            .ShouldBe([
                MhraProcedureTypeCodes.National,
                MhraProcedureTypeCodes.InternationalRecognition,
            ]);
    }

    [Fact]
    public void IdsOf_TranslatesCodes_ForTheBrowser()
    {
        _codes
            .IdsOf(
                typeof(MhraProcedureType),
                [MhraProcedureTypeCodes.InternationalRecognition, "gone"]
            )
            .ShouldBe(["7"]);
    }

    [Fact]
    public void CodesOf_TypeNotLoaded_Throws()
    {
        Should
            .Throw<InvalidOperationException>(() =>
                ReferenceCodes.None.CodesOf(typeof(MhraProcedureType), ["7"])
            )
            .Message.ShouldBe("The codes of MhraProcedureType were not loaded.");
    }
}
