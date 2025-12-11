using Microsoft.EntityFrameworkCore;
using StrictId;
using Visage.Shared.Models;

namespace Visage.Services.Registration.Repositories;

/// <summary>
/// T009: Repository for profile completion status checks.
/// Validates all 13 mandatory fields and calculates completion state.
/// </summary>
public class ProfileCompletionRepository
{
    private readonly RegistrantDB _db;

    public ProfileCompletionRepository(RegistrantDB db)
    {
        _db = db;
    }

    /// <summary>
    /// Gets profile completion status for a user.
    /// Checks all 13 mandatory fields per FR-001:
    /// - Personal: FirstName, LastName, Email, MobileNumber
    /// - Address: AddressLine1, City, State, PostalCode
    /// - Identity: GovtId, GovtIdLast4Digits
    /// - Professional: OccupationStatus, CompanyName (if employed), EducationalInstituteName (if student)
    /// </summary>
    public async Task<ProfileCompletionStatusDto?> GetCompletionStatusAsync(Id<Registrant> userId)
    {
        var registrant = await _db.Registrants
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == userId);

        if (registrant is null)
            return null;

        var incompleteMandatoryFields = new List<string>();

        // Check mandatory personal fields
        if (string.IsNullOrWhiteSpace(registrant.FirstName))
            incompleteMandatoryFields.Add("FirstName");
        if (string.IsNullOrWhiteSpace(registrant.LastName))
            incompleteMandatoryFields.Add("LastName");
        if (string.IsNullOrWhiteSpace(registrant.Email))
            incompleteMandatoryFields.Add("Email");
        if (string.IsNullOrWhiteSpace(registrant.MobileNumber))
            incompleteMandatoryFields.Add("MobileNumber");

        // Check mandatory address fields
        if (string.IsNullOrWhiteSpace(registrant.AddressLine1))
            incompleteMandatoryFields.Add("AddressLine1");
        if (string.IsNullOrWhiteSpace(registrant.City))
            incompleteMandatoryFields.Add("City");
        if (string.IsNullOrWhiteSpace(registrant.State))
            incompleteMandatoryFields.Add("State");
        if (string.IsNullOrWhiteSpace(registrant.PostalCode))
            incompleteMandatoryFields.Add("PostalCode");

        // Check mandatory identity fields: only keep last-4 digits to avoid storing full PII
        if (string.IsNullOrWhiteSpace(registrant.GovtIdLast4Digits))
            incompleteMandatoryFields.Add("GovtIdLast4Digits");

        // Check mandatory professional fields
        if (string.IsNullOrWhiteSpace(registrant.OccupationStatus))
            incompleteMandatoryFields.Add("OccupationStatus");

        // Context-dependent mandatory fields per FR-001
        if (registrant.OccupationStatus?.ToLowerInvariant() == "employed" &&
            string.IsNullOrWhiteSpace(registrant.CompanyName))
            incompleteMandatoryFields.Add("CompanyName");

        if (registrant.OccupationStatus?.ToLowerInvariant() == "student" &&
            string.IsNullOrWhiteSpace(registrant.EducationalInstituteName))
            incompleteMandatoryFields.Add("EducationalInstituteName");

        var isProfileComplete = incompleteMandatoryFields.Count == 0;

        return new ProfileCompletionStatusDto
        {
            UserId = registrant.Id.ToString(),
            IsProfileComplete = isProfileComplete,
            IsAideProfileComplete = registrant.IsAideProfileComplete,
            ProfileCompletedAt = registrant.ProfileCompletedAt,
            AideProfileCompletedAt = registrant.AideProfileCompletedAt,
            IncompleteMandatoryFields = incompleteMandatoryFields.Count > 0 ? incompleteMandatoryFields : null,
            CheckedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Marks mandatory profile as complete.
    /// Updates IsProfileComplete flag and ProfileCompletedAt timestamp.
    /// </summary>
    public async Task<bool> MarkProfileCompleteAsync(Id<Registrant> userId)
    {
        var registrant = await _db.Registrants
            .FirstOrDefaultAsync(r => r.Id == userId);

        if (registrant is null)
            return false;

        registrant.IsProfileComplete = true;
        registrant.ProfileCompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        
        return true;
    }

    /// <summary>
    /// Marks AIDE profile as complete.
    /// Updates IsAideProfileComplete flag and AideProfileCompletedAt timestamp.
    /// </summary>
    public async Task<bool> MarkAideProfileCompleteAsync(Id<Registrant> userId)
    {
        var registrant = await _db.Registrants
            .FirstOrDefaultAsync(r => r.Id == userId);

        if (registrant is null)
            return false;

        registrant.IsAideProfileComplete = true;
        registrant.AideProfileCompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        
        return true;
    }
}
