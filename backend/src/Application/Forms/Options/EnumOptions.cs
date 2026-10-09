using Microsoft.EntityFrameworkCore;
using UKPS.Api.Persistence;

namespace UKPS.Api.Application.Forms.Options;

/// <summary>Options for a question bound to an enum column, in the order given.</summary>
internal sealed class EnumOptions<TEnum> : QuestionOptions
    where TEnum : struct, Enum
{
    private readonly IReadOnlyList<QuestionOption> _options;

    public EnumOptions(IEnumerable<(TEnum Value, string Label)> options)
    {
        _options = [.. options.Select(o => new QuestionOption(o.Value.ToString(), o.Label))];
    }

    public override Type EnumType => typeof(TEnum);

    public override string Description =>
        $"Enum {typeof(TEnum).Name}: {string.Join(", ", _options.Select(o => $"{o.Value}={o.Label}"))}";

    public override Task<IReadOnlyList<QuestionOption>> LoadAsync(
        AppDbContext dbContext,
        IReadOnlyCollection<string> savedValues,
        CancellationToken cancellationToken
    ) => Task.FromResult(_options);
}
