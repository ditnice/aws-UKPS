using System.Reflection;
using Microsoft.EntityFrameworkCore;
using UKPS.Api.Persistence;
using UKPS.Api.Persistence.FormAnnotations;

namespace UKPS.Api.Application.RecordForms.Model;

/// <summary>
/// A content entity that takes part in a form. Each revision has at most one row of it, found by
/// its <c>RevisionId</c>.
/// </summary>
internal sealed class FormEntity
{
    private static readonly MethodInfo _loadMethod = typeof(FormEntity).GetMethod(
        nameof(LoadRowAsync),
        BindingFlags.NonPublic | BindingFlags.Static
    )!;

    private readonly Func<
        AppDbContext,
        int,
        IReadOnlyList<string>,
        bool,
        CancellationToken,
        Task<object?>
    > _load;

    private readonly PropertyInfo _revisionIdProperty;

    public Type ClrType { get; }
    public FormRowSource RowSource { get; }

    /// <summary>Navigations loaded with the row, because junction questions read them.</summary>
    public IReadOnlyList<string> Includes { get; }

    public string Name => ClrType.Name;

    public FormEntity(
        Type clrType,
        FormRowSource rowSource,
        PropertyInfo revisionIdProperty,
        IReadOnlyList<string> includes
    )
    {
        ClrType = clrType;
        RowSource = rowSource;
        Includes = includes;
        _revisionIdProperty = revisionIdProperty;
        _load = _loadMethod
            .MakeGenericMethod(clrType)
            .CreateDelegate<
                Func<
                    AppDbContext,
                    int,
                    IReadOnlyList<string>,
                    bool,
                    CancellationToken,
                    Task<object?>
                >
            >();
    }

    /// <summary>
    /// Loads the revision's row, tracked when it will be written, or returns null when it has
    /// none yet.
    /// </summary>
    public Task<object?> LoadAsync(
        AppDbContext dbContext,
        int revisionId,
        bool track,
        CancellationToken cancellationToken
    ) => _load(dbContext, revisionId, Includes, track, cancellationToken);

    /// <summary>Creates and adds a new row for the revision.</summary>
    public object AddRow(AppDbContext dbContext, int revisionId)
    {
        object row = Activator.CreateInstance(ClrType)!;
        _revisionIdProperty.SetValue(row, revisionId);
        dbContext.Add(row);
        return row;
    }

    private static async Task<object?> LoadRowAsync<T>(
        AppDbContext dbContext,
        int revisionId,
        IReadOnlyList<string> includes,
        bool track,
        CancellationToken cancellationToken
    )
        where T : class
    {
        IQueryable<T> query = track ? dbContext.Set<T>() : dbContext.Set<T>().AsNoTracking();
        query = query.Where(x => EF.Property<int>(x, "RevisionId") == revisionId);
        foreach (string include in includes)
        {
            query = query.Include(include);
        }

        return await query.SingleOrDefaultAsync(cancellationToken);
    }
}
