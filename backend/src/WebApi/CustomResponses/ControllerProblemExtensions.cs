using Microsoft.AspNetCore.Mvc;

namespace UKPS.Api.WebApi.CustomResponses;

/// <summary>
/// Problem responses shared by controllers, so each status is shaped the same way everywhere.
/// </summary>
internal static class ControllerProblemExtensions
{
    /// <summary>The problem details member that says which problem occurred.</summary>
    public const string CodeExtension = "code";

    /// <summary>A 404 problem with the given detail.</summary>
    /// <param name="controller">The controller handling the request.</param>
    /// <param name="detail">What could not be found.</param>
    /// <returns>The 404 result.</returns>
    public static ObjectResult NotFoundProblem(this ControllerBase controller, string detail) =>
        controller.Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Not Found",
            detail: detail
        );

    /// <summary>A 409 problem with a <c>code</c> member saying which conflict occurred.</summary>
    /// <param name="controller">The controller handling the request.</param>
    /// <param name="code">The machine-readable conflict code.</param>
    /// <param name="detail">What conflicted.</param>
    /// <returns>The 409 result.</returns>
    public static ObjectResult ConflictProblem(
        this ControllerBase controller,
        string code,
        string detail
    )
    {
        ProblemDetails problem = controller.ProblemDetailsFactory.CreateProblemDetails(
            controller.HttpContext,
            statusCode: StatusCodes.Status409Conflict,
            title: "Conflict",
            detail: detail
        );
        problem.Extensions[CodeExtension] = code;
        return new ObjectResult(problem) { StatusCode = StatusCodes.Status409Conflict };
    }
}
