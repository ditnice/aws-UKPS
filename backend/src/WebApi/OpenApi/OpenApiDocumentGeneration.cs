namespace UKPS.Api.WebApi.OpenApi;

/// <summary>
/// Detects when the application is being run by the build-time OpenAPI document generator.
/// </summary>
/// <remarks>
/// The <c>GenerateOpenApiDocuments</c> build target starts the application through the
/// <c>dotnet-getdocument</c> tool so it can read the endpoint metadata. In that case only the
/// checked-in configuration is available. Placeholder values have not been replaced, there is
/// no database, and no requests are served.
/// </remarks>
internal static class OpenApiDocumentGeneration
{
    /// <summary>
    /// Gets a value indicating whether the current process is generating OpenAPI documents.
    /// </summary>
    public static bool IsRunning { get; } =
        Environment.CommandLine.Contains("getdocument", StringComparison.OrdinalIgnoreCase);
}
