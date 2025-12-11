namespace Visage.FrontEnd.Web.Services;

/// <summary>
/// Scoped service that stores the Auth0 access token for use throughout
/// the Blazor Server circuit where HttpContext is not available.
/// </summary>
using Microsoft.Extensions.Logging;

public class CircuitAccessTokenProvider
{
    private readonly ILogger<CircuitAccessTokenProvider> _logger;
    private string? _accessToken;

    public CircuitAccessTokenProvider(ILogger<CircuitAccessTokenProvider> logger)
    {
        _logger = logger;
    }

    public string? AccessToken
    {
        get
        {
            _logger.LogDebug("CircuitAccessTokenProvider: AccessToken retrieved (length: {Length})", _accessToken?.Length ?? 0);
            return _accessToken;
        }
        set
        {
            _logger.LogInformation("CircuitAccessTokenProvider: AccessToken set (length: {Length})", value?.Length ?? 0);
            _accessToken = value;
        }
    }
}
