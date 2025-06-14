    using StrictId;
    using System.ComponentModel.DataAnnotations;

    namespace Visage.Shared.Models;

    public class Registrant
    {

        [Required]
        //[StringLength(50, ErrorMessage = "Event ID must be less than 50 characters.")]
        public Id<Registrant> Id { get; init; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        public string MiddleName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;


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

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string MobileNumber { get; set; } = string.Empty;

        [Required]
        public string GovtId { get; set; } = string.Empty;

        [Required]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "Must be 4 digits only.")]
        public string GovtIdLast4Digits { get; set; } = string.Empty;

        [Required]
        public string OccupationStatus { get; set; } = string.Empty;

        public string CompanyName { get; set; } = string.Empty;

        [Url]
        public string? LinkedInProfile { get; set; } = null;

        [Url]
        public string? GitHubProfile { get; set; } = null;

        public string EducationalInstituteName { get; set; } = string.Empty;

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

        public string FirstTimeAttendee { get; set; } = string.Empty;

        public string HowDidYouHear { get; set; } = string.Empty;

        public string SelfDescribeHowDidYouHear { get; set; } = string.Empty;

        public string AreasOfInterest { get; set; } = string.Empty;

        public string SelfDescribeAreasOfInterest { get; set; } = string.Empty;

        public string VolunteerOpportunities { get; set; } = string.Empty;

        public string AdditionalSupport { get; set; } = string.Empty;

        public string Religion { get; set; } = string.Empty;
        public string Caste { get; set; } = string.Empty;
        public string Neighborhood { get; set; } = string.Empty;
        public string ModeOfTransportation { get; set; } = string.Empty;
    }
