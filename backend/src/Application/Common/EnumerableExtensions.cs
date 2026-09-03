namespace UKPS.Api.Application.Common;

/// <summary>
/// Provides extension methods for <see cref="IEnumerable{T}"/>.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Enumerates the elements of the sequence, returning each element together
    /// with its zero-based index.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the elements in the sequence.
    /// </typeparam>
    /// <param name="source">
    /// The sequence to enumerate.
    /// </param>
    /// <returns>
    /// A sequence of tuples containing each element and its zero-based index.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="source"/> is <see langword="null"/>.
    /// </exception>
    public static IEnumerable<(T Value, int Index)> Enumerate<T>(this IEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Select((value, index) => (value, index));
    }
}
