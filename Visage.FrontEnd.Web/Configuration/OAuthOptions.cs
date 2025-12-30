using System.ComponentModel.DataAnnotations;

namespace Visage.FrontEnd.Web.Configuration;

/// <summary>
/// T087: OAuth configuration options for social profile validation
/// These are used for direct OAuth flows to verify profile ownership
/// </summary>
public class OAuthOptions
{
    public const string SectionName = "OAuth";

    /// <summary>
    /// Optional explicitly-configured base URL used when constructing redirect_uri values.
    /// If set, this value will be used instead of request-derived scheme/host (useful when
    /// running behind proxies or when you want deterministic redirect URIs in dev/CI).
    /// Example: "https://localhost:7400"
    /// </summary>
    public string? BaseUrl { get; set; }

    [Required]
    public LinkedInOAuthOptions LinkedIn { get; set; } = new();

    [Required]
    public GitHubOAuthOptions GitHub { get; set; } = new();
}

public class LinkedInOAuthOptions
{
    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string ClientSecret { get; set; } = string.Empty;

    public string AuthorizationEndpoint { get; set; } = "https://www.linkedin.com/oauth/v2/authorization";
    public string TokenEndpoint { get; set; } = "https://www.linkedin.com/oauth/v2/accessToken";
    // Use the OpenID Connect userinfo endpoint for LinkedIn when possible
    public string UserInfoEndpoint { get; set; } = "https://api.linkedin.com/v2/userinfo";
    // Default scope used for LinkedIn social verification (OpenID profile + email)
    public string Scope { get; set; } = "openid email profile";
    public string CallbackPath { get; set; } = "/oauth/linkedin/callback";
}

public class GitHubOAuthOptions
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