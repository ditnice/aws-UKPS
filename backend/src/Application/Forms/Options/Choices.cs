using System.Linq.Expressions;

namespace UKPS.Api.Application.Forms.Options;

/// <summary>Factory methods for question options.</summary>
internal static class Choices
{
    public static QuestionOptions Enum<TEnum>(params (TEnum Value, string Label)[] options)
        where TEnum : struct, System.Enum => new EnumOptions<TEnum>(options);

    public static QuestionOptions Reference<TReference>(
        Expression<Func<IQueryable<TReference>, IQueryable<TReference>>> query,
        Expression<Func<TReference, ReferenceOptionRow>> select
    )
        where TReference : class => new ReferenceOptions<TReference>(query, select);
}
