using System.ComponentModel.DataAnnotations;

namespace Visage.FrontEnd.Web.Configuration;

/// <summary>
/// T087: OAuth configuration options for social profile validation
/// These are used for direct OAuth flows to verify profile ownership
/// </summary>
public class OAuthOptions
{
    public const string SectionName = "OAuth";

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
    public string UserInfoEndpoint { get; set; } = "https://api.linkedin.com/v2/people/~:(id,firstName,lastName,profilePicture(displayImage~digitalmediaAsset:playableStreams))";
    public string Scope { get; set; } = "r_liteprofile r_emailaddress";
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