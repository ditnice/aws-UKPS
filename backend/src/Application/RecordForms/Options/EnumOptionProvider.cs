using UKPS.Api.Persistence;

namespace UKPS.Api.Application.RecordForms.Options;

/// <summary>A fixed list of enum members with labels, offered in the order given.</summary>
internal sealed class EnumOptionProvider<TEnum>(
    string key,
    IReadOnlyList<(TEnum Value, string Label, string? Hint)> items
) : IOptionProvider
    where TEnum : struct, Enum
{
    private readonly IReadOnlyList<FormOption> _options = items
        .Select(static item => new FormOption(item.Value.ToString(), item.Label, [], item.Hint))
        .ToArray();

    public string Key => key;
    public OptionProviderKind Kind => OptionProviderKind.Enum;
    public Type SourceType => typeof(TEnum);

    public string Description =>
        $"enum:{typeof(TEnum).Name}:"
        + string.Join(
            '|',
            _options.Select(static o =>
                o.Hint is null ? $"{o.Value}={o.Label}" : $"{o.Value}={o.Label} [{o.Hint}]"
            )
        );

    public Task<IReadOnlyList<FormOption>> GetOptionsAsync(
        AppDbContext dbContext,
        IReadOnlyCollection<string> savedValues,
        CancellationToken cancellationToken
    ) => Task.FromResult(_options);
}
