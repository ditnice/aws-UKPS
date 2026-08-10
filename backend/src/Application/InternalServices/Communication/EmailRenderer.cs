namespace UKPS.Api.Application.InternalServices.Communication;

internal static class EmailRenderer
{
    public static string RenderAsHtml(IEmail email)
    {
        return $"<p>{email.Content}</p>";
    }
}
