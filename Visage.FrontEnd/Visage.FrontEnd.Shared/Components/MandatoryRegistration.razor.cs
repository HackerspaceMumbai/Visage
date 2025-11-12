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
    private List<string> customErrors = new();
    private EditContext? editContext;
    private ValidationMessageStore? messageStore;
    private bool registrationSuccessful;
    // Local UI fields for government ID type (we only capture type + last4)
    private string govtIdType = string.Empty;
    private static readonly string[] userIdClaimTypes =
    [
        "sub",
        ClaimTypes.NameIdentifier,
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
        "http://schemas.microsoft.com/identity/claims/objectidentifier"
    ];

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    protected override async Task OnInitializedAsync()
    {
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
            govtIdType = Normalize(govtIdType);
            TrimUserInput();

            // T016: Validate context-dependent mandatory fields (FR-001)
            var contextErrors = ValidateContextDependentFields();
            if (contextErrors.Any())
            {
                customErrors.AddRange(contextErrors);
                editContext?.NotifyValidationStateChanged();
                return;
            }

            if (string.IsNullOrWhiteSpace(govtIdType))
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
            if (!string.IsNullOrWhiteSpace(govtIdType))
            {
                registrant.GovtIdType = govtIdType;
            }
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

            // T036: Invalidate cached completion status so home page reflects the latest state
            await ProfileService.InvalidateCacheAsync();
            _ = await ProfileService.GetCompletionStatusAsync();

        }
        catch (Exception ex)
        {
            customErrors.Add($"An unexpected error occurred: {ex.Message}");
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
    /// </summary>
    private List<string> ValidateContextDependentFields()
    {
        var errors = new List<string>();

        if (registrant.OccupationStatus == "Employed" || registrant.OccupationStatus == "Self-Employed")
        {
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
        }

        if (registrant.OccupationStatus == "Student")
        {
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
        }

        return errors;
    }

    private static string? ResolveUserId(ClaimsPrincipal? user)
    {
        if (user is null || user.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        foreach (var claimType in userIdClaimTypes)
        {
            var claimValue = user.FindFirst(claimType)?.Value;
            if (!string.IsNullOrWhiteSpace(claimValue))
            {
                return claimValue;
            }
        }

        return null;
    }

    private void NavigateToHome() => Navigation.NavigateTo("/");

    private void NavigateToProfileEdit() => Navigation.NavigateTo("/profile/edit");
}

