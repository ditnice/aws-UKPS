namespace UKPS.Api.Application.InternalServices.Communication;

/// <summary>
/// Represents data used when rendering email templates.
/// </summary>
public record EmailContextData
{
    /// <summary>
    /// Gets the email address for the UK PharmaScan helpdesk.
    /// </summary>
    public string HelpDeskEmail { get; init; } = "**PLACEHOLDER**";

    /// <summary>
    /// Gets the helpdesk email address formatted as an HTML mailto link.
    /// </summary>
    public string HelpDeskEmailLink => $"""<a href="mailto:{HelpDeskEmail}">{HelpDeskEmail}</a>""";
}
