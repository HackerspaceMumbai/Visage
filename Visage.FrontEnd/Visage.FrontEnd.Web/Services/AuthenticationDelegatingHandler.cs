using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace Visage.FrontEnd.Web.Services;

/// <summary>
/// DelegatingHandler that attaches Auth0 access token to outgoing HTTP requests.
/// Follows Microsoft's recommended pattern for Blazor Server authentication.
/// </summary>
public class AuthenticationDelegatingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CircuitAccessTokenProvider _tokenProvider;
    private readonly ILogger<AuthenticationDelegatingHandler> _logger;

    public AuthenticationDelegatingHandler(
        IHttpContextAccessor httpContextAccessor,
        CircuitAccessTokenProvider tokenProvider,
        ILogger<AuthenticationDelegatingHandler> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _tokenProvider = tokenProvider;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? accessToken = null;
        
        // First, try to get from HttpContext (available during initial page load/SSR)
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            try
            {
                accessToken = await httpContext.GetTokenAsync("access_token");
                if (!string.IsNullOrEmpty(accessToken))
                {
                    // Cache token in circuit-scoped provider for subsequent calls
                    _tokenProvider.AccessToken = accessToken;
                    _logger.LogInformation("AuthenticationDelegatingHandler: Retrieved and cached token from HttpContext (length: {Length})", accessToken.Length);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "AuthenticationDelegatingHandler: Failed to get token from HttpContext");
            }
        }
        else
        {
            // HttpContext not available (circuit call) - use cached token
            accessToken = _tokenProvider.AccessToken;
            if (!string.IsNullOrEmpty(accessToken))
            {
                _logger.LogDebug("AuthenticationDelegatingHandler: Using cached token from circuit provider (length: {Length})", accessToken.Length);
            }
            else
            {
                _logger.LogWarning("AuthenticationDelegatingHandler: HttpContext null and no cached token available");
            }
        }

        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            _logger.LogError("[TOKEN DEBUG] Handler: Added Bearer token to request to {Uri}", request.RequestUri);
        }
        else
        {
            _logger.LogError("[TOKEN DEBUG] Handler: NO TOKEN - Request to {Uri} will fail with 401", request.RequestUri);
        }
        
        return await base.SendAsync(request, cancellationToken);
    }
}