namespace UKPS.Api.Application.RecordForms.Scanning;

/// <summary>Thrown at startup when the form attributes describe an invalid form.</summary>
internal sealed class FormModelException : InvalidOperationException
{
    public IReadOnlyList<string> Problems { get; } = [];

    public FormModelException() { }

    public FormModelException(string? message)
        : base(message) { }

    public FormModelException(string? message, Exception? innerException)
        : base(message, innerException) { }

    public FormModelException(IReadOnlyList<string> problems)
        : base(
            $"The record content form attributes are invalid ({problems.Count} problem(s)):"
                + Environment.NewLine
                + string.Join(Environment.NewLine, problems.Select(static p => $"  - {p}"))
        )
    {
        Problems = problems;
    }
}
