using Microsoft.AspNetCore.Authentication;
using UKPS.Api.WebApi.Controllers;

namespace UKPS.Api.WebApi.InternalServices.Authentication;

internal class HttpContextAccessTokenProvider : IAccessTokenProvider
{
    private readonly IHttpContextAccessor _accessor;

    public HttpContextAccessTokenProvider(IHttpContextAccessor accessor) => _accessor = accessor;

    public async Task<string> GetAccessTokenAsync()
    {
        var context = _accessor.HttpContext ?? throw new InvalidOperationException();
        return await context.GetTokenAsync(CookieNames.AccessCookieName)
            ?? throw new InvalidOperationException();
    }
}
