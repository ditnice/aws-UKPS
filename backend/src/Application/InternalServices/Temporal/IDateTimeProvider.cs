namespace UKPS.Api.Application.InternalServices.Temporal;

/// <summary>
/// Provides an abstraction for retrieving the current UTC date and time.
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current UTC date and time with an explicit offset.
    /// </summary>
    /// <returns>
    /// A <see cref="DateTimeOffset"/> value representing the current date and time in UTC.
    /// </returns>
    DateTimeOffset GetOffsetUtcNow();

    /// <summary>
    /// Gets the current UTC date and time.
    /// </summary>
    /// <returns>
    /// A <see cref="DateTime"/> value representing the current date and time in UTC.
    /// </returns>
    DateTime GetUtcNow();
}
