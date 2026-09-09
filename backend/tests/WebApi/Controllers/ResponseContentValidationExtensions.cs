using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace UKPS.Api.Tests.WebApi.Controllers;

public static class ResponseContentValidationExtensions
{
    public static async Task<IDictionary<string, string[]>> ShouldContainValidationErrors(
        this HttpContent httpContent
    )
    {
        var problemDetails = await httpContent.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.ShouldNotBeNull();
        problemDetails.Errors.ShouldNotBeNull();
        return problemDetails.Errors;
    }
}
