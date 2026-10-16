using System.Globalization;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using UKPS.Api.Persistence;

namespace UKPS.Api.Application.Forms.Options;

/// <summary>
/// Options loaded from a reference data table. <c>query</c> filters and orders the table;
/// <c>select</c> projects each row. Both must be translatable by EF Core. They are expressions
/// so their text is part of the definition fingerprint.
/// </summary>
internal sealed class ReferenceOptions<TReference> : QuestionOptions
    where TReference : class
{
    private readonly Expression<Func<IQueryable<TReference>, IQueryable<TReference>>> _query;
    private readonly Func<IQueryable<TReference>, IQueryable<TReference>> _compiledQuery;
    private readonly Expression<Func<TReference, ReferenceOptionRow>> _select;

    public ReferenceOptions(
        Expression<Func<IQueryable<TReference>, IQueryable<TReference>>> query,
        Expression<Func<TReference, ReferenceOptionRow>> select
    )
    {
        _query = query;
        _compiledQuery = query.Compile();
        _select = select;
    }

    public override Type? EnumType => null;

    public override string Description =>
        $"Reference {typeof(TReference).Name}: {_query} {_select}";

    public override async Task<IReadOnlyList<QuestionOption>> LoadAsync(
        AppDbContext dbContext,
        IReadOnlyCollection<string> savedValues,
        CancellationToken cancellationToken
    )
    {
        var rows = await _compiledQuery(dbContext.Set<TReference>().AsNoTracking())
            .Select(_select)
            .ToListAsync(cancellationToken);

        return
        [
            .. rows.Select(row => (Value: ToValue(row.Id), Row: row))
                .Where(option =>
                    !option.Row.IsArchived
                    || savedValues.Contains(option.Value, StringComparer.Ordinal)
                )
                .Select(option => new QuestionOption(option.Value, option.Row.Label)),
        ];
    }

    private static string ToValue(int id) => id.ToString(CultureInfo.InvariantCulture);
}
