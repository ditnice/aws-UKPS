namespace UKPS.Api.Tests.Utilities.Data;

internal static class EntityModificationExtensions
{
    public static T Update<T>(this T entity, Action<T> updateAction)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(updateAction);

        updateAction(entity);
        return entity;
    }
}
