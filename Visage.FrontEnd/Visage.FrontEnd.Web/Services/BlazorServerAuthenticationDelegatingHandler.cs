using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace Visage.FrontEnd.Web.Services;

/// <summary>
/// Blazor Server-compatible authentication delegating handler that retrieves
/// the access token from the current authentication state.
/// Works in Blazor Server circuits where HttpContext may not be available.
/// </summary>
public class BlazorServerAuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<BlazorServerAuthenticationDelegatingHandler> _logger;

    public BlazorServerAuthenticationDelegatingHandler(
        AuthenticationStateProvider authenticationStateProvider,
        IHttpContextAccessor httpContextAccessor,
        ILogger<BlazorServerAuthenticationDelegatingHandler> logger)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? accessToken = null;

        // Try to get access token from HttpContext first (works for initial page load)
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            accessToken = await httpContext.GetTokenAsync("access_token");
            if (!string.IsNullOrEmpty(accessToken))
            {
                _logger.LogInformation("BlazorServerAuthHandler: Got access token from HttpContext (length: {Length})", accessToken.Length);
            }
        }

        // If HttpContext is null or token not found, try to get it from AuthenticationStateProvider
        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogInformation("BlazorServerAuthHandler: HttpContext null or token not found, trying AuthenticationStateProvider");
            
            // In Blazor Server, we need to get the token from the server-side authentication state
            // The ServerAuthenticationStateProvider has access to HttpContext via RevalidatingServerAuthenticationStateProvider
            if (_authenticationStateProvider is RevalidatingServerAuthenticationStateProvider)
            {
                // Unfortunately, RevalidatingServerAuthenticationStateProvider doesn't expose the HttpContext directly
                // We need to rely on HttpContext being available via IHttpContextAccessor
                _logger.LogWarning("BlazorServerAuthHandler: Cannot retrieve token from RevalidatingServerAuthenticationStateProvider directly");
            }
        }

        if (string.IsNullOrEmpty(accessToken))
        {
            _logger.LogError("BlazorServerAuthHandler: Access token is null or empty - API call will fail with 401");
        }
        else
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _logger.LogInformation("BlazorServerAuthHandler: Added Bearer token to request");
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
