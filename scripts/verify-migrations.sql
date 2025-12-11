-- Verify EF Core Migrations Applied
-- Run this against the RegistrantDB database

-- Check migration history
SELECT MigrationId, ProductVersion 
FROM __EFMigrationsHistory 
ORDER BY MigrationId DESC;

-- Expected migrations:
-- 20250128xxxxxx_AddProfileCompletionTracking
-- 20250128xxxxxx_AddDraftRegistrationTable
-- 20250128xxxxxx_AddUserPreferencesTable

-- Verify Registrants table has new columns
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Registrants'
  AND COLUMN_NAME IN ('IsProfileComplete', 'IsAideProfileComplete', 'ProfileCompletedAt', 'AideProfileCompletedAt')
ORDER BY COLUMN_NAME;

-- Verify DraftRegistrations table exists
SELECT COUNT(*) AS DraftRegistrationsTableExists
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'DraftRegistrations';

-- Verify UserPreferences table exists
SELECT COUNT(*) AS UserPreferencesTableExists
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME = 'UserPreferences';

-- Check UserPreferences schema
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'UserPreferences'
ORDER BY ORDINAL_POSITION;
-- Expected columns: Id, UserId, AideBannerDismissedAt, CreatedAt, UpdatedAt
