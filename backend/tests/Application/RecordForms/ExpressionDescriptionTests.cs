using System.Linq.Expressions;
using Shouldly;
using UKPS.Api.Application.RecordForms.Options;
using UKPS.Api.Persistence.Entities.ReferenceData;

namespace UKPS.Api.Tests.Application.RecordForms;

public class ExpressionDescriptionTests
{
    [Fact]
    public void Describe_Query_ListsTheCallsInOrder()
    {
        Expression<Func<IQueryable<BnfChapter>, IQueryable<BnfChapter>>> query = q =>
            q.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id);

        ExpressionDescription
            .Describe(query)
            .ShouldBe("(q) => q.OrderBy((x) => x.DisplayOrder).ThenBy((x) => x.Id)");
    }

    [Fact]
    public void Describe_Projection_IncludesConstructorArgumentsAndLiterals()
    {
        Expression<Func<BnfChapter, ReferenceOptionRow>> projection = x => new ReferenceOptionRow(
            x.Id,
            x.Code + ": " + x.Label,
            x.IsArchived,
            x.ParentId
        );

        ExpressionDescription
            .Describe(projection)
            .ShouldBe(
                "(x) => new ReferenceOptionRow(x.Id, ((x.Code Add \": \") Add x.Label), x.IsArchived, x.ParentId)"
            );
    }

    [Fact]
    public void Describe_CapturedVariable_IsNotSupported()
    {
        string separator = ": ";
        Expression<Func<BnfChapter, string>> projection = x => x.Code + separator;

        Should.Throw<NotSupportedException>(() => ExpressionDescription.Describe(projection));
    }
}
