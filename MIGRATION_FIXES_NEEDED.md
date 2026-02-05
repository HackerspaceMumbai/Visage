# Database Migration Fixes Required

## Overview
The `InitialUserProfile` migration (20260110145754) has several issues that need to be corrected with a new migration.

## Issues Identified in Code Review

### 1. Cross-Service Coupling - Event Table
**Problem**: The UserProfile service migration creates an `Event` table, which should only be managed by the Eventing service.

**Impact**: 
- Violates microservice boundaries
- Creates CASCADE delete dependencies across services
- Causes data synchronization issues

**Fix**: Remove the Event table from UserProfile database. The EventId in EventRegistrations should be a simple string reference without a foreign key constraint.

### 2. Missing Auth0Subject Column in Users Table
**Problem**: The Users table is missing the `Auth0Subject` column, which is defined in `Visage.Shared.Models.User.cs` with `[StringLength(255)]`.

**Impact**: 
- Runtime errors when persisting user data
- Authentication flow breaks
- Cannot enforce authenticated ownership

**Fix**: Add `Auth0Subject` column:
```sql
Auth0Subject nvarchar(255) NOT NULL
```

### 3. Missing Auth0Subject Column in EventRegistrations Table
**Problem**: The EventRegistrations table is missing the `Auth0Subject` column, which is defined in `Visage.Shared.Models.EventRegistration.cs`.

**Impact**:
- Cannot enforce authenticated ownership for registrations
- Eventing service EventDB expects this column (see indexes at lines 72-74 of EventDB.cs)
- Schema mismatch between services

**Fix**: Add `Auth0Subject` column:
```sql
Auth0Subject nvarchar(255) NOT NULL
```

## Recommended Migration Steps

### Option 1: Create New Migration (Recommended)
```bash
# Navigate to UserProfile service directory
cd Visage.Services.UserProfile

# Enable aspire exec feature
aspire config set features.execCommandEnabled true

# Create the migration
aspire exec --resource userprofile-api --workdir /path/to/Visage.Services.UserProfile -- dotnet ef migrations add FixCrossServiceCouplingAndAuth0Subject
```

### Option 2: Manual Migration File
If EF Core tools are not available, create a new migration file manually:

**Filename**: `Visage.Services.UserProfile/Migrations/YYYYMMDDHHMMSS_FixCrossServiceCouplingAndAuth0Subject.cs`

**Up Method**:
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Remove FK constraint from EventRegistrations to Event
    migrationBuilder.DropForeignKey(
        name: "FK_EventRegistrations_Event_EventId",
        table: "EventRegistrations");

    // Drop the Event table (should only exist in Eventing service)
    migrationBuilder.DropTable(
        name: "Event");

    // Add Auth0Subject to Users table
    migrationBuilder.AddColumn<string>(
        name: "Auth0Subject",
        table: "Users",
        type: "nvarchar(255)",
        maxLength: 255,
        nullable: false,
        defaultValue: "");

    // Add Auth0Subject to EventRegistrations table
    migrationBuilder.AddColumn<string>(
        name: "Auth0Subject",
        table: "EventRegistrations",
        type: "nvarchar(255)",
        maxLength: 255,
        nullable: false,
        defaultValue: "");
}
```

**Down Method**:
```csharp
protected override void Down(MigrationBuilder migrationBuilder)
{
    // Remove Auth0Subject columns
    migrationBuilder.DropColumn(
        name: "Auth0Subject",
        table: "EventRegistrations");

    migrationBuilder.DropColumn(
        name: "Auth0Subject",
        table: "Users");

    // Recreate Event table
    migrationBuilder.CreateTable(
        name: "Event",
        columns: table => new
        {
            Id = table.Column<string>(type: "nchar(26)", fixedLength: true, maxLength: 26, nullable: false),
            Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
            Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
            StartDate = table.Column<DateOnly>(type: "date", nullable: false),
            StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
            EndDate = table.Column<DateOnly>(type: "date", nullable: false),
            EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
            Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
            CoverPicture = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
            AttendeesPercentage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
            Hashtag = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
            Theme = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_Event", x => x.Id);
        });

    // Recreate FK constraint
    migrationBuilder.AddForeignKey(
        name: "FK_EventRegistrations_Event_EventId",
        table: "EventRegistrations",
        column: "EventId",
        principalTable: "Event",
        principalColumn: "Id",
        onDelete: ReferentialAction.Cascade);
}
```

## Testing After Migration

1. Verify the migration applies successfully:
```bash
aspire exec --resource userprofile-api --workdir /path/to/Visage.Services.UserProfile -- dotnet ef database update
```

2. Run integration tests:
```bash
dotnet test tests/Visage.Test.Aspire/Visage.Test.Aspire.csproj
```

3. Verify User creation works with Auth0Subject:
```bash
# Test the POST /api/users endpoint with Auth0 authentication
```

4. Verify Event registration works:
```bash
# Test the POST /api/registrations endpoint
```

## Related Files
- `/home/runner/work/Visage/Visage/Visage.Services.UserProfile/Migrations/20260110145754_InitialUserProfile.cs`
- `/home/runner/work/Visage/Visage/Visage.Shared/Models/User.cs` (line 22: Auth0Subject property)
- `/home/runner/work/Visage/Visage/Visage.Shared/Models/EventRegistration.cs` (line 35: Auth0Subject property)
- `/home/runner/work/Visage/Visage/services/Visage.Services.Eventing/EventDB.cs` (lines 72-74: Auth0Subject indexes)

## Status
⚠️ **Action Required**: These database schema changes cannot be completed in this environment due to missing EF Core tooling. They need to be applied in the actual development environment where:
- EF Core tools are installed
- Aspire CLI is available
- Database connections are configured

All other code review suggestions have been addressed in the current PR.
