using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;

namespace Visage.FrontEnd.Web.Services;

public class AuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationDelegatingHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
        Console.WriteLine("Outgoing Access Token " + accessToken);
        return await base.SendAsync(request, cancellationToken);
    }
}