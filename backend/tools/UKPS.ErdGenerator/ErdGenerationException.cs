using System.Diagnostics.CodeAnalysis;

namespace UKPS.ErdGenerator;

/// <summary>
/// Raised when the ERD cannot be generated because the grouping configuration is
/// missing, inconsistent, or out of step with the EF Core model.
/// </summary>
[SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "Sonar S3871 requires exception types to be public."
)]
public sealed class ErdGenerationException : Exception
{
    public ErdGenerationException() { }

    public ErdGenerationException(string message)
        : base(message) { }

    public ErdGenerationException(string message, Exception innerException)
        : base(message, innerException) { }
}
