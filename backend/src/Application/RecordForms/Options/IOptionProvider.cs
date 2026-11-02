using UKPS.Api.Persistence;

namespace UKPS.Api.Application.RecordForms.Options;

/// <summary>A named source of options for choice questions.</summary>
internal interface IOptionProvider
{
    string Key { get; }
    OptionProviderKind Kind { get; }

    /// <summary>The enum offered by an enum provider, or the entity queried by a reference provider.</summary>
    Type SourceType { get; }

    /// <summary>A stable description of what is offered, for the form fingerprint.</summary>
    string Description { get; }

    /// <summary>
    /// Loads the options. Archived reference items are left out unless they are in
    /// <paramref name="savedValues"/>.
    /// </summary>
    Task<IReadOnlyList<FormOption>> GetOptionsAsync(
        AppDbContext dbContext,
        IReadOnlyCollection<string> savedValues,
        CancellationToken cancellationToken
    );
}
