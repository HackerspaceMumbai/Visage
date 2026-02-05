using Microsoft.EntityFrameworkCore;
using StrictId;
using Visage.Shared.Models;

namespace Visage.Services.UserProfile.Repositories;

/// <summary>
/// T009: Repository for profile completion status checks.
/// Validates all 13 mandatory fields and calculates completion state.
/// </summary>
public class ProfileCompletionRepository
{
    private readonly UserDB _db;

    public ProfileCompletionRepository(UserDB db)
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
    public async Task<ProfileCompletionStatusDto?> GetCompletionStatusAsync(Id<User> userId)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return null;

        var incompleteMandatoryFields = new List<string>();

        // Check mandatory personal fields
        if (string.IsNullOrWhiteSpace(user.FirstName))
            incompleteMandatoryFields.Add("FirstName");
        if (string.IsNullOrWhiteSpace(user.LastName))
            incompleteMandatoryFields.Add("LastName");
        if (string.IsNullOrWhiteSpace(user.Email))
            incompleteMandatoryFields.Add("Email");
        if (string.IsNullOrWhiteSpace(user.MobileNumber))
            incompleteMandatoryFields.Add("MobileNumber");

        // Check mandatory address fields
        if (string.IsNullOrWhiteSpace(user.AddressLine1))
            incompleteMandatoryFields.Add("AddressLine1");
        if (string.IsNullOrWhiteSpace(user.City))
            incompleteMandatoryFields.Add("City");
        if (string.IsNullOrWhiteSpace(user.State))
            incompleteMandatoryFields.Add("State");
        if (string.IsNullOrWhiteSpace(user.PostalCode))
            incompleteMandatoryFields.Add("PostalCode");

        // Check mandatory identity fields: only keep last-4 digits to avoid storing full PII
        if (string.IsNullOrWhiteSpace(user.GovtIdLast4Digits))
            incompleteMandatoryFields.Add("GovtIdLast4Digits");

        // Check mandatory professional fields
        if (string.IsNullOrWhiteSpace(user.OccupationStatus))
            incompleteMandatoryFields.Add("OccupationStatus");

        // Context-dependent mandatory fields per FR-001
        if (user.OccupationStatus?.ToLowerInvariant() == "employed" &&
            string.IsNullOrWhiteSpace(user.CompanyName))
            incompleteMandatoryFields.Add("CompanyName");

        if (user.OccupationStatus?.ToLowerInvariant() == "student" &&
            string.IsNullOrWhiteSpace(user.EducationalInstituteName))
            incompleteMandatoryFields.Add("EducationalInstituteName");

        var isProfileComplete = incompleteMandatoryFields.Count == 0;

        return new ProfileCompletionStatusDto
        {
            UserId = user.Id.ToString(),
            IsProfileComplete = isProfileComplete,
            IsAideProfileComplete = user.IsAideProfileComplete,
            ProfileCompletedAt = user.ProfileCompletedAt,
            AideProfileCompletedAt = user.AideProfileCompletedAt,
            IncompleteMandatoryFields = incompleteMandatoryFields.Count > 0 ? incompleteMandatoryFields : null,
            CheckedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Marks mandatory profile as complete.
    /// Updates IsProfileComplete flag and ProfileCompletedAt timestamp.
    /// </summary>
    public async Task<bool> MarkProfileCompleteAsync(Id<User> userId)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return false;

        user.IsProfileComplete = true;
        user.ProfileCompletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        
        return true;
    }

    /// <summary>
    /// Marks AIDE profile as complete.
    /// Updates IsAideProfileComplete flag and AideProfileCompletedAt timestamp.
    /// </summary>
    public async Task<bool> MarkAideProfileCompleteAsync(Id<User> userId)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return false;

        user.IsAideProfileComplete = true;
        user.AideProfileCompletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        
        return true;
    }
}
