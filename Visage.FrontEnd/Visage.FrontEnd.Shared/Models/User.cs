using StrictId;
using System.ComponentModel.DataAnnotations;

namespace Visage.FrontEnd.Shared.Models;

/// <summary>
/// User entity representing a person who can register for multiple events.
/// Contains identity, contact, and demographic profile information.
/// Frontend model matching the backend User entity.
/// </summary>
public class User
{
    [Required]
    public Id<User> Id { get; init; }

    // Personal Information
    [Required]
    public string FirstName { get; set; } = string.Empty;

    public string MiddleName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    // Contact Information
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string MobileNumber { get; set; } = string.Empty;

    // Address Information
    public string Address { get; set; } = string.Empty;

    [Required]
    public string AddressLine1 { get; set; } = string.Empty;

    public string AddressLine2 { get; set; } = string.Empty;

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string State { get; set; } = string.Empty;

    [Required]
    public string PostalCode { get; set; } = string.Empty;

    // Government ID (anonymized)
    [Required]
    public string GovtId { get; set; } = string.Empty;

    [Required]
    public string GovtIdType { get; set; } = string.Empty;

    [Required]
    [StringLength(4, MinimumLength = 4, ErrorMessage = "Must be 4 digits only.")]
    public string GovtIdLast4Digits { get; set; } = string.Empty;

    // Occupation
    [Required]
    public string OccupationStatus { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string EducationalInstituteName { get; set; } = string.Empty;

    // Social Profiles with OAuth verification
    [Url]
    public string? LinkedInProfile { get; set; } = null;

    public string? LinkedInVanityName { get; set; } = null;

    public string? LinkedInSubject { get; set; } = null;

    public string? LinkedInRawProfileJson { get; set; } = null;

    public string? LinkedInRawEmailJson { get; set; } = null;

    public DateTime? LinkedInPayloadFetchedAt { get; set; }

    [Url]
    public string? GitHubProfile { get; set; } = null;

    // OAuth verification tracking
    public bool IsLinkedInVerified { get; set; } = false;

    public bool IsGitHubVerified { get; set; } = false;

    public DateTime? LinkedInVerifiedAt { get; set; }

    public DateTime? GitHubVerifiedAt { get; set; }

    // AIDE (Accessibility, Inclusiveness, Diversity, Equity) Profile Fields
    public string GenderIdentity { get; set; } = string.Empty;

    public string SelfDescribeGender { get; set; } = string.Empty;

    public string AgeRange { get; set; } = string.Empty;

    public string Ethnicity { get; set; } = string.Empty;

    public string SelfDescribeEthnicity { get; set; } = string.Empty;

    public string LanguageProficiency { get; set; } = string.Empty;

    public string SelfDescribeLanguage { get; set; } = string.Empty;

    public string EducationalBackground { get; set; } = string.Empty;

    public string SelfDescribeEducation { get; set; } = string.Empty;

    public string Disability { get; set; } = string.Empty;

    public string DisabilityDetails { get; set; } = string.Empty;

    public string DietaryRequirements { get; set; } = string.Empty;

    public string SelfDescribeDietary { get; set; } = string.Empty;

    public string LgbtqIdentity { get; set; } = string.Empty;

    public string ParentalStatus { get; set; } = string.Empty;

    public string HowDidYouHear { get; set; } = string.Empty;

    public string SelfDescribeHowDidYouHear { get; set; } = string.Empty;

    public string AdditionalSupport { get; set; } = string.Empty;

    public string Religion { get; set; } = string.Empty;

    public string Caste { get; set; } = string.Empty;

    public string Neighborhood { get; set; } = string.Empty;

    public string ModeOfTransportation { get; set; } = string.Empty;

    public string SocioeconomicBackground { get; set; } = string.Empty;

    public string Neurodiversity { get; set; } = string.Empty;

    public string CaregivingResponsibilities { get; set; } = string.Empty;

    // Profile completion tracking
    public bool IsProfileComplete { get; set; } = false;

    public DateTime? ProfileCompletedAt { get; set; }

    public bool IsAideProfileComplete { get; set; } = false;

    public DateTime? AideProfileCompletedAt { get; set; }

    // Account timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
