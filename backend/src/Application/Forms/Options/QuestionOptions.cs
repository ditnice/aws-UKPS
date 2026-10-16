using UKPS.Api.Persistence;

namespace UKPS.Api.Application.Forms.Options;

/// <summary>
/// Where a question's options come from. Options are loaded per request so that reference
/// data changes are picked up without a restart.
/// </summary>
internal abstract class QuestionOptions
{
    /// <summary>
    /// The enum type for enum options, or <c>null</c> for reference data options.
    /// </summary>
    public abstract Type? EnumType { get; }

    /// <summary>A stable description used in the definition fingerprint.</summary>
    public abstract string Description { get; }

    /// <summary>
    /// Loads the options a user may choose. Archived reference data is excluded unless its value
    /// is in <paramref name="savedValues"/> (i.e. it is already stored on the revision).
    /// </summary>
    public abstract Task<IReadOnlyList<QuestionOption>> LoadAsync(
        AppDbContext dbContext,
        IReadOnlyCollection<string> savedValues,
        CancellationToken cancellationToken
    );
}
