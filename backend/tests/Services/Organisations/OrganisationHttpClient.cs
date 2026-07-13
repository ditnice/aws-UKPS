using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using UKPS.Api.Common;
using UKPS.Api.DTOs;
using UKPS.Api.Services.Errors;
using UKPS.Api.Services.Interfaces;
using UKPS.Api.Tests.Fixtures;
using GetOrganisationResult = UKPS.Api.Common.Result<
    UKPS.Api.DTOs.OrganisationDetailsDto,
    UKPS.Api.Services.Errors.GetOrganisationByIdError
>;
using UpdateOrganisationResult = UKPS.Api.Common.Result<
    UKPS.Api.DTOs.OrganisationDetailsDto,
    UKPS.Api.Services.Errors.UpdateOrganisationDetailsError
>;

namespace UKPS.Api.Tests.Services.Organisations;

internal sealed class OrganisationHttpClient : IOrganisationService
{
    private readonly HttpClient _httpClient;

    public IOrganisationMembershipService Memberships =>
        throw new InvalidOperationException(
            "The membership service is not implemented on this class."
        );

    public OrganisationHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GetOrganisationResult> GetOrganisationById(int id)
    {
        var uri = new Uri($"/organisations/{id}", UriKind.Relative);
        HttpResponseMessage response = await _httpClient.GetAsync(uri);
        return await ConvertGetOrganisationByIdResponse(id, response);
    }

    public async Task<UpdateOrganisationResult> UpdateOrganisationDetails(
        int id,
        UpdateOrganisationDetailsDto organisationDetails
    )
    {
        var uri = new Uri($"/organisations/{id}", UriKind.Relative);
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync(uri, organisationDetails);
        return await ConvertUpdateOrganisationResponse(id, response);
    }

    private static async Task<GetOrganisationResult> ConvertGetOrganisationByIdResponse(
        int id,
        HttpResponseMessage response
    )
    {
        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => GetOrganisationResult.Err(
                new GetOrganisationByIdError.NotFound(id)
            ),
            HttpStatusCode.Forbidden => GetOrganisationResult.Err(
                new GetOrganisationByIdError.NotAllowed(id)
            ),
            HttpStatusCode.OK => await ParseOkResultFromBody<
                OrganisationDetailsDto,
                GetOrganisationByIdError
            >(response),
            _ => throw new InvalidOperationException(
                "Unexpected response status code: " + response.StatusCode
            ),
        };
    }

    private static async Task<UpdateOrganisationResult> ConvertUpdateOrganisationResponse(
        int id,
        HttpResponseMessage response
    )
    {
        return response.StatusCode switch
        {
            HttpStatusCode.NotFound => UpdateOrganisationResult.Err(
                new UpdateOrganisationDetailsError.NotFound(id)
            ),
            HttpStatusCode.Forbidden => UpdateOrganisationResult.Err(
                new UpdateOrganisationDetailsError.NotAllowed(id)
            ),
            HttpStatusCode.OK => await ParseOkResultFromBody<
                OrganisationDetailsDto,
                UpdateOrganisationDetailsError
            >(response),
            _ => throw new InvalidOperationException(
                "Unexpected response status code: " + response.StatusCode
            ),
        };
    }

    private static async Task<Result<TValue, TError>> ParseOkResultFromBody<TValue, TError>(
        HttpResponseMessage response
    )
        where TValue : notnull
        where TError : notnull
    {
        var dto =
            await response.Content.ReadFromJsonAsync<TValue>(TestJsonOptions.Default)
            ?? throw new InvalidOperationException("Failed to parse response object from body.");
        return Result<TValue, TError>.Ok(dto);
    }
}
