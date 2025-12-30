using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Authorization;
using Visage.Shared.Models;
using Visage.FrontEnd.Shared.Services;
using StrictId;
using Visage.FrontEnd.Shared;
using System.Linq;
using System.Security.Claims;

namespace Visage.FrontEnd.Shared.Components;

/// <summary>
/// T016: MandatoryRegistration component code-behind.
/// Handles form validation, submission, and server-side validation feedback.
/// Uses ValidationMessageStore for custom server-side error messages.
/// T036: Invalidates ProfileService cache after successful submission.
/// </summary>
public partial class MandatoryRegistration : ComponentBase
{
    private Registrant registrant = new();
    private bool isSubmitting = false;
    private readonly List<string> customErrors = new();
    private readonly List<string> successMessages = new();
    private EditContext? editContext;
    private ValidationMessageStore? messageStore;
    private bool registrationSuccessful;

    // T087: Social connection status tracking
    private SocialConnectionStatusDto? socialStatus;

    private bool _shouldCleanOAuthQueryParams;

    private bool IsLinkedInConnected =>
        socialStatus?.LinkedIn.IsConnected == true
        || registrant.IsLinkedInVerified
        || !string.IsNullOrWhiteSpace(registrant.LinkedInProfile);

    private string? LinkedInProfileUrl =>
        socialStatus?.LinkedIn.ProfileUrl
        ?? registrant.LinkedInProfile;

    private bool IsGitHubConnected =>
        socialStatus?.GitHub.IsConnected == true
        || registrant.IsGitHubVerified
        || !string.IsNullOrWhiteSpace(registrant.GitHubProfile);

    private string? GitHubProfileUrl =>
        socialStatus?.GitHub.ProfileUrl
        ?? registrant.GitHubProfile;
    
    private static readonly string[] userIdClaimTypes =
    [
        "sub",
        ClaimTypes.NameIdentifier,
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
        "http://schemas.microsoft.com/identity/claims/objectidentifier"
    ];

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    [Inject]
    private ISocialAuthService SocialAuthService { get; set; } = default!;

    [Inject]
    private IRegistrationDraftService RegistrationDraftService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _shouldCleanOAuthQueryParams = false;

        // T016: Initialize EditContext and ValidationMessageStore
        editContext = new EditContext(registrant);
        editContext.SetFieldCssClassProvider(new DaisyUIInputError());
        messageStore = new ValidationMessageStore(editContext);

        // T016: Clear custom errors when field values change
        editContext.OnFieldChanged += (sender, args) =>
        {
            messageStore?.Clear(args.FieldIdentifier);
            customErrors.Clear();
        };

        // T087: Handle OAuth redirect query params from direct provider flow FIRST (before loading draft)
        // This ensures we capture the successful OAuth connection before draft restoration
        bool hasOAuthRedirect = false;
        string? oauthProvider = null;
        try
        {
            var uri = new Uri(Navigation.Uri);
            var q = uri.Query.TrimStart('?');
            var query = q.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(p => p.Split(new[] { '=' }, 2))
                         .Where(kv => kv.Length >= 1)
                         .ToDictionary(kv => Uri.UnescapeDataString(kv[0]), kv => kv.Length > 1 ? Uri.UnescapeDataString(kv[1]) : string.Empty, StringComparer.OrdinalIgnoreCase);

            if (query.TryGetValue("social", out var socialProvider) && query.TryGetValue("result", out var result))
            {
                hasOAuthRedirect = true;
                oauthProvider = socialProvider;
                var res = result;

                if (res == "success")
                {
                    successMessages.Add($"{oauthProvider.ToUpperInvariant()} connected successfully.");
                }
                else if (res == "conflict")
                {
                    customErrors.Add($"This {oauthProvider} account is already verified for another registrant.");
                }
                else
                {
                    if (query.TryGetValue("reason", out var reason) && !string.IsNullOrWhiteSpace(reason))
                    {
                        var display = reason.Length > 200 ? reason.Substring(0, 200) + "..." : reason;
                        customErrors.Add($"Failed to verify social profile. Reason: {display}");
                    }
                    else
                    {
                        customErrors.Add("Failed to verify social profile. Please try again.");
                    }
                }
            }
        }
        catch (Exception)
        {
            // ignore query parsing errors
        }

        // T087: Load social connection status (best-effort; may be 404 until registrant exists)
        socialStatus = await SocialAuthService.GetSocialStatusAsync();

        // T087: Restore registration draft if it exists (saved before navigating to OAuth)
        var draft = await RegistrationDraftService.GetDraftAsync();
        if (draft is not null)
        {
            registrant = draft;
            // Re-initialize EditContext with the restored registrant
            editContext = new EditContext(registrant);
            editContext.SetFieldCssClassProvider(new DaisyUIInputError());
            messageStore = new ValidationMessageStore(editContext);
            // Re-attach field changed handler
            editContext.OnFieldChanged += (sender, args) =>
            {
                messageStore?.Clear(args.FieldIdentifier);
                customErrors.Clear();
            };
        }

        // T087: Update registrant with OAuth-verified URLs from social status (takes precedence over draft)
        if (socialStatus?.LinkedIn.IsConnected == true && !string.IsNullOrWhiteSpace(socialStatus.LinkedIn.ProfileUrl))
        {
            registrant.LinkedInProfile = socialStatus.LinkedIn.ProfileUrl;
            registrant.IsLinkedInVerified = true;
        }
        if (socialStatus?.GitHub.IsConnected == true && !string.IsNullOrWhiteSpace(socialStatus.GitHub.ProfileUrl))
        {
            registrant.GitHubProfile = socialStatus.GitHub.ProfileUrl;
            registrant.IsGitHubVerified = true;
        }

        // T087: (Optional) Check for pending social profiles in session (captured during OAuth callback).
        // Note: In Blazor Server, server-to-server calls don't automatically include browser cookies,
        // so this may not be reliable. Draft persistence is the primary mechanism.
        var pendingProfiles = await SocialAuthService.GetPendingProfilesAsync();
        if (pendingProfiles is not null)
        {
            if (!string.IsNullOrWhiteSpace(pendingProfiles.LinkedInProfile) || !string.IsNullOrWhiteSpace(pendingProfiles.LinkedInSubject) || !string.IsNullOrWhiteSpace(pendingProfiles.LinkedInRawProfileJson))
            {
                if (!string.IsNullOrWhiteSpace(pendingProfiles.LinkedInProfile))
                {
                    registrant.LinkedInProfile = pendingProfiles.LinkedInProfile;
                }

                registrant.LinkedInSubject = pendingProfiles.LinkedInSubject;
                registrant.LinkedInRawProfileJson = pendingProfiles.LinkedInRawProfileJson;
                registrant.LinkedInRawEmailJson = pendingProfiles.LinkedInRawEmailJson;
                registrant.LinkedInPayloadFetchedAt ??= DateTime.UtcNow;

                registrant.IsLinkedInVerified = true;

                // Also reflect in the UI status model when Registrations API status isn't available yet.
                socialStatus ??= new SocialConnectionStatusDto();
                socialStatus.LinkedIn.IsConnected = true;
                socialStatus.LinkedIn.ProfileUrl ??= pendingProfiles.LinkedInProfile;

                // Pre-fill email if not already set
                if (string.IsNullOrWhiteSpace(registrant.Email) && !string.IsNullOrWhiteSpace(pendingProfiles.LinkedInEmail))
                {
                    registrant.Email = pendingProfiles.LinkedInEmail;
                }
            }
            if (!string.IsNullOrWhiteSpace(pendingProfiles.GitHubProfile))
            {
                registrant.GitHubProfile = pendingProfiles.GitHubProfile;
                registrant.IsGitHubVerified = true;

                socialStatus ??= new SocialConnectionStatusDto();
                socialStatus.GitHub.IsConnected = true;
                socialStatus.GitHub.ProfileUrl ??= pendingProfiles.GitHubProfile;

                // Pre-fill email if not already set
                if (string.IsNullOrWhiteSpace(registrant.Email) && !string.IsNullOrWhiteSpace(pendingProfiles.GitHubEmail))
                {
                    registrant.Email = pendingProfiles.GitHubEmail;
                }
            }
        }

        // Remove OAuth query params by navigating to clean URL (must be AFTER all data loading)
        if (hasOAuthRedirect)
        {
            _shouldCleanOAuthQueryParams = true;
        }

        // T016: Pre-populate from Auth0 claims if available
        if (AuthenticationStateTask is not null)
        {
            var authState = await AuthenticationStateTask;
            var user = authState.User;

            if (user.Identity?.IsAuthenticated == true)
            {
                // Pre-fill email from Auth0 email claim
                var emailClaim = user.FindFirst(c => c.Type == "email" || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
                if (emailClaim is not null && !string.IsNullOrWhiteSpace(emailClaim.Value))
                {
                    registrant.Email = emailClaim.Value;
                }

                // Intentionally leave name, address, and mobile fields blank so users can verify details manually
                // Email is the only field pre-filled to reduce typing friction.

                // Intentionally do not pre-fill full GovtId for privacy reasons
                // If the DB has only last4 we leave GovtId empty and prefill last4 if present
                if (!string.IsNullOrWhiteSpace(registrant.GovtIdLast4Digits))
                {
                    // keep existing last4 in the bound registrant model
                }
            }
        }
    }

    protected override Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || !_shouldCleanOAuthQueryParams)
        {
            return Task.CompletedTask;
        }

        var currentUri = new Uri(Navigation.Uri);
        if (string.IsNullOrEmpty(currentUri.Query))
        {
            return Task.CompletedTask;
        }

        var cleanUri = currentUri.GetLeftPart(UriPartial.Path);
        if (!string.Equals(Navigation.Uri, cleanUri, StringComparison.Ordinal))
        {
            Navigation.NavigateTo(cleanUri, replace: true);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// T016: Handle valid form submission.
    /// Posts registrant data to API and marks profile as complete.
    /// </summary>
    private async Task HandleValidSubmit()
    {
        try
        {
            isSubmitting = true;
            customErrors.Clear();
            messageStore?.Clear();
            registrant.GovtIdType = Normalize(registrant.GovtIdType);
            TrimUserInput();

            BuildLinkedInProfileFromVanityIfPresent();

            // T016: Validate context-dependent mandatory fields (FR-001)
            var contextErrors = ValidateContextDependentFields();
            if (contextErrors.Any())
            {
                customErrors.AddRange(contextErrors);
                editContext?.NotifyValidationStateChanged();
                return;
            }

            if (string.IsNullOrWhiteSpace(registrant.GovtIdType))
            {
                customErrors.Add("Please select a government ID type.");
                editContext?.NotifyValidationStateChanged();
                return;
            }

            // T016/T083: Re-evaluate authentication state to capture freshest claims
            var authState = AuthenticationStateTask is not null
                ? await AuthenticationStateTask
                : await AuthenticationStateProvider.GetAuthenticationStateAsync();

            var userId = ResolveUserId(authState.User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                customErrors.Add("Authentication error: Unable to identify user. Please log in again.");
                editContext?.NotifyValidationStateChanged();
                return;
            }

            // T016: Try parse userId to StrictId; if it's an external provider id (e.g. auth0|...) we'll submit without client-side Id
            Id<Registrant>? parsedUserId = null;
            if (Id<Registrant>.TryParse(userId, out var tmpParsed))
            {
                parsedUserId = tmpParsed;
            }

            // Only capture type + last-4; do not store full GovtId to avoid PII leakage
            registrant.GovtId = string.Empty;

            var registrantToSubmit = BuildRegistrantPayload(parsedUserId);

            var registrationResult = await RegistrationService.RegisterAsync(registrantToSubmit);

            if (!registrationResult.IsSuccess)
            {
                var statusCode = registrationResult.StatusCode;
                if (statusCode == default)
                {
                    customErrors.Add("We could not save your profile right now. Please review the required fields and try again.");
                }
                else
                {
                    customErrors.Add($"We could not save your profile right now. (Status {(int)statusCode} {statusCode}) Please review the required fields and try again.");
                }

                if (!string.IsNullOrWhiteSpace(registrationResult.ErrorMessage))
                {
                    customErrors.Add($"Server response: {registrationResult.ErrorMessage}");
                }

                editContext?.NotifyValidationStateChanged();
                return;
            }

            var savedRegistrant = registrationResult.Registrant;

            if (savedRegistrant is null)
            {
                customErrors.Add("Profile saved but server returned an unexpected response. Please refresh and verify your details.");
                editContext?.NotifyValidationStateChanged();
                return;
            }

            registrant = savedRegistrant;
            registrationSuccessful = true;

            // T087: Clear the registration draft after successful submission
            await RegistrationDraftService.ClearDraftAsync();

            // T036: Invalidate cached completion status so home page reflects the latest state
            await ProfileService.InvalidateCacheAsync();
            _ = await ProfileService.GetCompletionStatusAsync();

        }
        catch (HttpRequestException)
        {
            customErrors.Add("Network error. Please check your connection and try again.");
            editContext?.NotifyValidationStateChanged();
        }
        catch (TaskCanceledException)
        {
            customErrors.Add("Request timeout. Please try again.");
            editContext?.NotifyValidationStateChanged();
        }
        catch (Exception)
        {
            customErrors.Add("An unexpected error occurred. Please try again.");
            editContext?.NotifyValidationStateChanged();
        }
        finally
        {
            isSubmitting = false;
            editContext?.NotifyValidationStateChanged();
        }
    }

    private Registrant BuildRegistrantPayload(Id<Registrant>? parsedUserId)
    {
        var payload = parsedUserId.HasValue
            ? new Registrant { Id = parsedUserId.Value }
            : new Registrant();

        payload.FirstName = registrant.FirstName;
        payload.LastName = registrant.LastName;
        payload.MiddleName = registrant.MiddleName;
        payload.Email = registrant.Email;
        payload.MobileNumber = registrant.MobileNumber;
        payload.Address = registrant.Address;
        payload.AddressLine1 = registrant.AddressLine1;
        payload.AddressLine2 = registrant.AddressLine2;
        payload.City = registrant.City;
        payload.State = registrant.State;
        payload.PostalCode = registrant.PostalCode;
        payload.GovtId = registrant.GovtId;
        payload.GovtIdLast4Digits = registrant.GovtIdLast4Digits;
        payload.GovtIdType = registrant.GovtIdType;
        payload.OccupationStatus = registrant.OccupationStatus;
        payload.CompanyName = registrant.CompanyName;
        payload.EducationalInstituteName = registrant.EducationalInstituteName;
        payload.LinkedInProfile = registrant.LinkedInProfile;
        payload.LinkedInVanityName = registrant.LinkedInVanityName;
        payload.LinkedInSubject = registrant.LinkedInSubject;
        payload.LinkedInRawProfileJson = registrant.LinkedInRawProfileJson;
        payload.LinkedInRawEmailJson = registrant.LinkedInRawEmailJson;
        payload.LinkedInPayloadFetchedAt = registrant.LinkedInPayloadFetchedAt;

        payload.GitHubProfile = registrant.GitHubProfile;
        payload.IsLinkedInVerified = registrant.IsLinkedInVerified;
        payload.IsGitHubVerified = registrant.IsGitHubVerified;
        payload.IsProfileComplete = true;
        payload.ProfileCompletedAt = DateTime.UtcNow;
        payload.IsAideProfileComplete = registrant.IsAideProfileComplete;
        payload.AideProfileCompletedAt = registrant.AideProfileCompletedAt;

        return payload;
    }

    private void TrimUserInput()
    {
        registrant.FirstName = Normalize(registrant.FirstName);
        registrant.LastName = Normalize(registrant.LastName);
        registrant.MiddleName = registrant.MiddleName?.Trim() ?? string.Empty;
        registrant.Email = Normalize(registrant.Email);
        registrant.MobileNumber = Normalize(registrant.MobileNumber);
        registrant.AddressLine1 = Normalize(registrant.AddressLine1);
        registrant.AddressLine2 = registrant.AddressLine2?.Trim() ?? string.Empty;
        registrant.City = Normalize(registrant.City);
        registrant.State = Normalize(registrant.State);
        registrant.PostalCode = Normalize(registrant.PostalCode);
        registrant.GovtIdLast4Digits = Normalize(registrant.GovtIdLast4Digits);
        registrant.CompanyName = registrant.CompanyName?.Trim() ?? string.Empty;
        registrant.EducationalInstituteName = registrant.EducationalInstituteName?.Trim() ?? string.Empty;
        registrant.OccupationStatus = Normalize(registrant.OccupationStatus);
    }

    private static string Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();

    /// <summary>
    /// T016: Handle invalid form submission.
    /// Provides user feedback when validation fails.
    /// </summary>
    private void HandleInvalidSubmit()
    {
        customErrors.Clear();
        if (editContext is not null)
        {
            var fieldMessages = editContext.GetValidationMessages().Distinct();
            customErrors.AddRange(fieldMessages);
        }

        if (!customErrors.Any())
        {
            customErrors.Add("Please fill in all required fields marked with *");
        }
    }

    /// <summary>
    /// T016: Validate context-dependent mandatory fields per FR-001.
    /// CompanyName required if Employed/Self-Employed, EducationalInstituteName required if Student.
    /// T087: LinkedIn required for Employed/Self-Employed, GitHub required for Student.
    /// </summary>
    private List<string> ValidateContextDependentFields()
    {
        var errors = new List<string>();

        switch (registrant.OccupationStatus)
        {
            case "Employed":
            case "Self-Employed":
                if (string.IsNullOrWhiteSpace(registrant.CompanyName))
                {
                    errors.Add("Company Name is required for employed individuals");
                    if (messageStore is not null && editContext is not null)
                    {
                        messageStore.Add(
                            editContext.Field(nameof(registrant.CompanyName)),
                            "Company Name is required for your occupation status");
                    }
                }
                // T087: LinkedIn mandatory for professionals
                if (string.IsNullOrWhiteSpace(registrant.LinkedInProfile))
                {
                    errors.Add("LinkedIn Profile is required for working professionals");
                    if (messageStore is not null && editContext is not null)
                    {
                        messageStore.Add(
                            editContext.Field(nameof(registrant.LinkedInProfile)),
                            "LinkedIn Profile is required for employed/self-employed individuals");
                    }
                }
                break;
            case "Student":
                if (string.IsNullOrWhiteSpace(registrant.EducationalInstituteName))
                {
                    errors.Add("Educational Institution is required for students");
                    if (messageStore is not null && editContext is not null)
                    {
                        messageStore.Add(
                            editContext.Field(nameof(registrant.EducationalInstituteName)),
                            "Educational Institution is required for students");
                    }
                }
                // T087: GitHub mandatory for students
                if (string.IsNullOrWhiteSpace(registrant.GitHubProfile))
                {
                    errors.Add("GitHub Profile is required for students");
                    if (messageStore is not null && editContext is not null)
                    {
                        messageStore.Add(
                            editContext.Field(nameof(registrant.GitHubProfile)),
                            "GitHub Profile is required for students");
                    }
                }
                break;
        }

        return errors;
    }

    private static string? ResolveUserId(ClaimsPrincipal? user)
    {
        if (user is null || user.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return userIdClaimTypes
            .Select(claimType => user.FindFirst(claimType)?.Value)
            .FirstOrDefault(claimValue => !string.IsNullOrWhiteSpace(claimValue));
    }

    private void NavigateToHome() => Navigation.NavigateTo("/");

    private void NavigateToProfileEdit() => Navigation.NavigateTo("/profile/edit");

    // T087: OAuth social connection handlers
    private async Task ConnectLinkedIn()
    {
        try
        {
            BuildLinkedInProfileFromVanityIfPresent();

            if (string.IsNullOrWhiteSpace(registrant.LinkedInVanityName))
            {
                customErrors.Add("Please enter your LinkedIn vanity name before connecting.");
                editContext?.NotifyValidationStateChanged();
                return;
            }

            // Save current form state as draft before navigating away
            await RegistrationDraftService.SaveDraftAsync(registrant);

            var authUrl = await SocialAuthService.GetLinkedInAuthUrlAsync();
            Navigation.NavigateTo(authUrl, forceLoad: true);
        }
        catch (Exception)
        {
            customErrors.Add("Failed to initiate LinkedIn connection. Please try again.");
            editContext?.NotifyValidationStateChanged();
        }
    }

    private async Task ConnectGitHub()
    {
        try
        {
            // Save current form state as draft before navigating away
            await RegistrationDraftService.SaveDraftAsync(registrant);

            var authUrl = await SocialAuthService.GetGitHubAuthUrlAsync();
            Navigation.NavigateTo(authUrl, forceLoad: true);
        }
        catch (Exception ex)
        {
            customErrors.Add("Failed to initiate GitHub connection. Please try again.");
            editContext?.NotifyValidationStateChanged();
        }
    }

    private async Task DisconnectLinkedIn()
    {
        // T080/T087: If registrant row doesn't exist yet, disconnect should clear pending/draft values.
        try
        {
            var shouldUsePendingClear = !(socialStatus?.LinkedIn.IsConnected == true);

            var ok = shouldUsePendingClear
                ? await SocialAuthService.ClearPendingAsync("linkedin")
                : await SocialAuthService.DisconnectAsync("linkedin");

            if (!ok)
            {
                customErrors.Add("Failed to disconnect LinkedIn. Please try again.");
                editContext?.NotifyValidationStateChanged();
                return;
            }

            socialStatus = await SocialAuthService.GetSocialStatusAsync();

            if (socialStatus is null)
            {
                registrant.LinkedInProfile = null;
                registrant.IsLinkedInVerified = false;
            }
            else
            {
                registrant.LinkedInProfile = socialStatus.LinkedIn.ProfileUrl;
                registrant.IsLinkedInVerified = socialStatus.LinkedIn.IsConnected;
            }

            if (shouldUsePendingClear)
            {
                registrant.LinkedInProfile = null;
                registrant.LinkedInVanityName = null;
                registrant.LinkedInSubject = null;
                registrant.LinkedInRawProfileJson = null;
                registrant.LinkedInRawEmailJson = null;
                registrant.LinkedInPayloadFetchedAt = null;
                registrant.IsLinkedInVerified = false;
            }

            StateHasChanged();
        }
        catch (Exception)
        {
            customErrors.Add("Failed to disconnect LinkedIn. Please try again.");
            editContext?.NotifyValidationStateChanged();
        }
    }

    private async Task DisconnectGitHub()
    {
        // T080/T087: If registrant row doesn't exist yet, disconnect should clear pending/draft values.
        try
        {
            var shouldUsePendingClear = !(socialStatus?.GitHub.IsConnected == true);

            var ok = shouldUsePendingClear
                ? await SocialAuthService.ClearPendingAsync("github")
                : await SocialAuthService.DisconnectAsync("github");

            if (!ok)
            {
                customErrors.Add("Failed to disconnect GitHub. Please try again.");
                editContext?.NotifyValidationStateChanged();
                return;
            }

            socialStatus = await SocialAuthService.GetSocialStatusAsync();

            if (socialStatus is null)
            {
                registrant.GitHubProfile = null;
                registrant.IsGitHubVerified = false;
            }
            else
            {
                registrant.GitHubProfile = socialStatus.GitHub.ProfileUrl;
                registrant.IsGitHubVerified = socialStatus.GitHub.IsConnected;
            }

            if (shouldUsePendingClear)
            {
                registrant.GitHubProfile = null;
                registrant.IsGitHubVerified = false;
            }

            StateHasChanged();
        }
        catch (Exception)
        {
            customErrors.Add("Failed to disconnect GitHub. Please try again.");
            editContext?.NotifyValidationStateChanged();
        }
    }

    private async Task DisconnectSocial(string provider)
    {
        // T087: Generic disconnect method (kept for backwards compatibility)
        if (provider.Equals("linkedin", StringComparison.OrdinalIgnoreCase))
        {
            await DisconnectLinkedIn();
        }
        else if (provider.Equals("github", StringComparison.OrdinalIgnoreCase))
        {
            await DisconnectGitHub();
        }
    }

    private const string LinkedInProfilePrefix = "https://www.linkedin.com/in/";

    private void BuildLinkedInProfileFromVanityIfPresent()
    {
        var vanity = Normalize(registrant.LinkedInVanityName);
        registrant.LinkedInVanityName = vanity;

        if (string.IsNullOrWhiteSpace(vanity))
        {
            return;
        }

        vanity = vanity.Trim().Trim('/');
        if (vanity.StartsWith(LinkedInProfilePrefix, StringComparison.OrdinalIgnoreCase))
        {
            vanity = vanity.Substring(LinkedInProfilePrefix.Length);
        }

        registrant.LinkedInVanityName = vanity;
        registrant.LinkedInProfile = $"{LinkedInProfilePrefix}{vanity}";
    }
}

