using StrictId;
using System.ComponentModel.DataAnnotations;

namespace Visage.Shared.Models;

/// <summary>
/// Represents a user of the Visage system, tied to Auth0 identity.
/// A User can register for multiple events and maintains registration/profile state.
/// </summary>
public class User
{
    /// <summary>
    /// Internal system ID (GUID-based StrictId)
    /// </summary>
    [Required]
    public Id<User> Id { get; init; }

    /// <summary>
    /// Auth0 subject claim (e.g., "auth0|abc123") - stable external identity
    /// </summary>
    [StringLength(255)]
    public string Auth0Subject { get; set; } = string.Empty;

    // Personal Information
    /// <summary>
    /// User's first name
    /// </summary>
    [Required]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's middle name or initial
    /// </summary>
    public string MiddleName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name
    /// </summary>
    [Required]
    public string LastName { get; set; } = string.Empty;

    // Contact Information
    /// <summary>
    /// User's primary email address (from Auth0)
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's mobile phone number
    /// </summary>
    [Required]
    [Phone]
    public string MobileNumber { get; set; } = string.Empty;

    // Address Information
    /// <summary>
    /// User's full address
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// User's primary address line
    /// </summary>
    [Required]
    public string AddressLine1 { get; set; } = string.Empty;

    /// <summary>
    /// User's secondary address line (optional)
    /// </summary>
    public string AddressLine2 { get; set; } = string.Empty;

    /// <summary>
    /// User's city of residence
    /// </summary>
    [Required]
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// User's state or province
    /// </summary>
    [Required]
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// User's postal or ZIP code
    /// </summary>
    [Required]
    public string PostalCode { get; set; } = string.Empty;

    // Government ID (anonymized)
    /// <summary>
    /// User's government-issued ID (anonymized)
    /// </summary>
    public string GovtId { get; set; } = string.Empty;

    /// <summary>
    /// Type of government ID provided (e.g., SSN, TIN)
    /// </summary>
    [MaxLength(32)]
    public string GovtIdType { get; set; } = string.Empty;

    /// <summary>
    /// Last 4 digits of the government ID (for verification)
    /// </summary>
    [StringLength(4, MinimumLength = 4, ErrorMessage = "Must be 4 digits only.")]
    public string GovtIdLast4Digits { get; set; } = string.Empty;

    // Occupation
    /// <summary>
    /// User's current occupation status
    /// </summary>
    [Required]
    public string OccupationStatus { get; set; } = string.Empty;

    /// <summary>
    /// Name of the user's employer or business
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Name of the educational institution user attended or attends
    /// </summary>
    public string EducationalInstituteName { get; set; } = string.Empty;

    // Social Profiles with OAuth verification
    /// <summary>
    /// URL of the user's LinkedIn profile
    /// </summary>
    [Url]
    public string? LinkedInProfile { get; set; }

    /// <summary>
    /// User's LinkedIn vanity name (custom URL segment)
    /// </summary>
    public string? LinkedInVanityName { get; set; }

    /// <summary>
    /// LinkedIn subject identifier for the user
    /// </summary>
    public string? LinkedInSubject { get; set; }

    /// <summary>
    /// Raw JSON profile data from LinkedIn
    /// </summary>
    public string? LinkedInRawProfileJson { get; set; }

    /// <summary>
    /// Raw JSON email data from LinkedIn
    /// </summary>
    public string? LinkedInRawEmailJson { get; set; }

    /// <summary>
    /// Timestamp when LinkedIn payload was last fetched
    /// </summary>
    public DateTime? LinkedInPayloadFetchedAt { get; set; }

    /// <summary>
    /// URL of the user's GitHub profile
    /// </summary>
    [Url]
    public string? GitHubProfile { get; set; }

    /// <summary>
    /// Is the LinkedIn profile verified by the user?
    /// </summary>
    public bool IsLinkedInVerified { get; set; }

    /// <summary>
    /// Is the GitHub profile verified by the user?
    /// </summary>
    public bool IsGitHubVerified { get; set; }

    /// <summary>
    /// Timestamp when LinkedIn profile was verified
    /// </summary>
    public DateTime? LinkedInVerifiedAt { get; set; }

    /// <summary>
    /// Timestamp when GitHub profile was verified
    /// </summary>
    public DateTime? GitHubVerifiedAt { get; set; }

    // AIDE (Accessibility, Inclusiveness, Diversity, Equity) Profile Fields
    /// <summary>
    /// User's gender identity
    /// </summary>
    public string GenderIdentity { get; set; } = string.Empty;

    /// <summary>
    /// User's self-description of their gender
    /// </summary>
    public string SelfDescribeGender { get; set; } = string.Empty;

    /// <summary>
    /// User's age range (e.g., "18-24", "25-34")
    /// </summary>
    public string AgeRange { get; set; } = string.Empty;

    /// <summary>
    /// User's ethnicity
    /// </summary>
    public string Ethnicity { get; set; } = string.Empty;

    /// <summary>
    /// User's self-description of their ethnicity
    /// </summary>
    public string SelfDescribeEthnicity { get; set; } = string.Empty;

    /// <summary>
    /// User's language proficiency
    /// </summary>
    public string LanguageProficiency { get; set; } = string.Empty;

    /// <summary>
    /// User's self-description of their language skills
    /// </summary>
    public string SelfDescribeLanguage { get; set; } = string.Empty;

    /// <summary>
    /// User's educational background
    /// </summary>
    public string EducationalBackground { get; set; } = string.Empty;

    /// <summary>
    /// User's self-description of their education
    /// </summary>
    public string SelfDescribeEducation { get; set; } = string.Empty;

    /// <summary>
    /// Does the user have a disability?
    /// </summary>
    public string Disability { get; set; } = string.Empty;

    /// <summary>
    /// Details about the user's disability
    /// </summary>
    public string DisabilityDetails { get; set; } = string.Empty;

    /// <summary>
    /// User's dietary requirements
    /// </summary>
    public string DietaryRequirements { get; set; } = string.Empty;

    /// <summary>
    /// User's self-description of their dietary restrictions
    /// </summary>
    public string SelfDescribeDietary { get; set; } = string.Empty;

    /// <summary>
    /// User's LGBTQ+ identity
    /// </summary>
    public string LgbtqIdentity { get; set; } = string.Empty;

    /// <summary>
    /// User's parental status
    /// </summary>
    public string ParentalStatus { get; set; } = string.Empty;

    /// <summary>
    /// How did the user hear about us?
    /// </summary>
    public string HowDidYouHear { get; set; } = string.Empty;

    /// <summary>
    /// User's self-description of how they heard about us
    /// </summary>
    public string SelfDescribeHowDidYouHear { get; set; } = string.Empty;

    /// <summary>
    /// Does the user require additional support?
    /// </summary>
    public string AdditionalSupport { get; set; } = string.Empty;

    /// <summary>
    /// User's religion
    /// </summary>
    public string Religion { get; set; } = string.Empty;

    /// <summary>
    /// User's caste (if applicable)
    /// </summary>
    public string Caste { get; set; } = string.Empty;

    /// <summary>
    /// User's neighborhood or locality
    /// </summary>
    public string Neighborhood { get; set; } = string.Empty;

    /// <summary>
    /// User's mode of transportation
    /// </summary>
    public string ModeOfTransportation { get; set; } = string.Empty;

    /// <summary>
    /// User's socioeconomic background
    /// </summary>
    public string SocioeconomicBackground { get; set; } = string.Empty;

    /// <summary>
    /// Is the user neurodiverse?
    /// </summary>
    public string Neurodiversity { get; set; } = string.Empty;

    /// <summary>
    /// Does the user have caregiving responsibilities?
    /// </summary>
    public string CaregivingResponsibilities { get; set; } = string.Empty;

    // Completion tracking
    /// <summary>
    /// Is the user's profile complete?
    /// </summary>
    public bool IsProfileComplete { get; set; }

    /// <summary>
    /// Timestamp when the profile was completed
    /// </summary>
    public DateTime? ProfileCompletedAt { get; set; }

    /// <summary>
    /// Is the user's AIDE profile complete?
    /// </summary>
    public bool IsAideProfileComplete { get; set; }

    /// <summary>
    /// Timestamp when the AIDE profile was completed
    /// </summary>
    public DateTime? AideProfileCompletedAt { get; set; }

    // Account timestamps
    /// <summary>
    /// Timestamp when the user first authenticated with the system
    /// </summary>
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the user data was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Last authentication timestamp
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    // Navigation collections (used in EF model configuration)
    public ICollection<EventRegistration> EventRegistrations { get; set; } = new List<EventRegistration>();
}
