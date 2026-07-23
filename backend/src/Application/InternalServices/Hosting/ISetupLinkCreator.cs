namespace UKPS.Api.Application.InternalServices.Hosting;

/// <summary>
/// Provides an abstraction for creating setup links for user account configuration.
/// </summary>
public interface ISetupLinkCreator
{
    /// <summary>
    /// Creates a setup link associated with the specified setup token.
    /// </summary>
    /// <param name="setupToken">
    /// The unique token used to identify the setup process.
    /// </param>
    /// <returns>
    /// A URL that allows the user to complete the setup process.
    /// </returns>
    string GetSetupLink(Guid setupToken);
}
