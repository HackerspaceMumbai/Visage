using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visage.Services.Registrations.Migrations
{
    /// <inheritdoc />
    public partial class AddProfileCompletionTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Registrants",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nchar(26)", fixedLength: true, maxLength: 26, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GovtId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GovtIdLast4Digits = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    OccupationStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinkedInProfile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GitHubProfile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EducationalInstituteName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GenderIdentity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SelfDescribeGender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgeRange = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ethnicity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SelfDescribeEthnicity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LanguageProficiency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SelfDescribeLanguage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EducationalBackground = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SelfDescribeEducation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Disability = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DisabilityDetails = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DietaryRequirements = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SelfDescribeDietary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LgbtqIdentity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentalStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstTimeAttendee = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HowDidYouHear = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SelfDescribeHowDidYouHear = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AreasOfInterest = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SelfDescribeAreasOfInterest = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VolunteerOpportunities = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdditionalSupport = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Religion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Caste = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Neighborhood = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModeOfTransportation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsProfileComplete = table.Column<bool>(type: "bit", nullable: false),
                    ProfileCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsAideProfileComplete = table.Column<bool>(type: "bit", nullable: false),
                    AideProfileCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registrants", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Registrants");
        }
    }
}
