using System.ComponentModel.DataAnnotations;

namespace Visage.FrontEnd.Web.Configuration;

/// <summary>
/// OAuth configuration options for direct provider verification flows.
/// These options are bound from configuration section "OAuth".
/// </summary>
public sealed class OAuthOptions
{
    public const string SectionName = "OAuth";

    /// <summary>
    /// Optional base URL used for deterministic redirect_uri generation. When set, this value
    /// overrides the request-derived base URL. Example: https://localhost:7400
    /// </summary>
    public string? BaseUrl { get; set; }

    [Required]
    public LinkedInOAuthOptions LinkedIn { get; set; } = new();

    [Required]
    public GitHubOAuthOptions GitHub { get; set; } = new();
}

public sealed class LinkedInOAuthOptions
{
    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string ClientSecret { get; set; } = string.Empty;

    public string AuthorizationEndpoint { get; set; } = "https://www.linkedin.com/oauth/v2/authorization";
    public string TokenEndpoint { get; set; } = "https://www.linkedin.com/oauth/v2/accessToken";

    // Use the OpenID Connect userinfo endpoint for LinkedIn when possible
    public string UserInfoEndpoint { get; set; } = "https://api.linkedin.com/v2/userinfo";

    // Recommended scopes for LinkedIn profile/email access
    public string Scope { get; set; } = "openid email profile";
    public string CallbackPath { get; set; } = "/oauth/linkedin/callback";
}

public sealed class GitHubOAuthOptions
{
    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string ClientSecret { get; set; } = string.Empty;

    public string AuthorizationEndpoint { get; set; } = "https://github.com/login/oauth/authorize";
    public string TokenEndpoint { get; set; } = "https://github.com/login/oauth/access_token";
    public string UserInfoEndpoint { get; set; } = "https://api.github.com/user";

    public string Scope { get; set; } = "read:user user:email";
    public string CallbackPath { get; set; } = "/oauth/github/callback";
}
