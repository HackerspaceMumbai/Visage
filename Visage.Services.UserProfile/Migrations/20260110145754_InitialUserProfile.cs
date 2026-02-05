using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visage.Services.UserProfile.Migrations
{
    /// <inheritdoc />
    public partial class InitialUserProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nchar(26)", fixedLength: true, maxLength: 26, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GovtId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GovtIdLast4Digits = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: false),
                    GovtIdType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OccupationStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EducationalInstituteName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LinkedInProfile = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LinkedInVanityName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkedInSubject = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LinkedInRawProfileJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkedInRawEmailJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LinkedInPayloadFetchedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GitHubProfile = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsLinkedInVerified = table.Column<bool>(type: "bit", nullable: false),
                    IsGitHubVerified = table.Column<bool>(type: "bit", nullable: false),
                    LinkedInVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GitHubVerifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    HowDidYouHear = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SelfDescribeHowDidYouHear = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AdditionalSupport = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Religion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Caste = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Neighborhood = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModeOfTransportation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SocioeconomicBackground = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Neurodiversity = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CaregivingResponsibilities = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsProfileComplete = table.Column<bool>(type: "bit", nullable: false),
                    ProfileCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsAideProfileComplete = table.Column<bool>(type: "bit", nullable: false),
                    AideProfileCompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DraftRegistrations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nchar(26)", fixedLength: true, maxLength: 26, nullable: false),
                    UserId = table.Column<string>(type: "nchar(26)", fixedLength: true, maxLength: 26, nullable: false),
                    Section = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DraftData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    IsApplied = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DraftRegistrations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventRegistrations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nchar(26)", fixedLength: true, maxLength: 26, nullable: false),
                    UserId = table.Column<string>(type: "nchar(26)", fixedLength: true, maxLength: 26, nullable: false),
                    EventId = table.Column<string>(type: "nchar(26)", fixedLength: true, maxLength: 26, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RegisteredAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CheckedInAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FirstTimeAttendee = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AreasOfInterest = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SelfDescribeAreasOfInterest = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VolunteerOpportunities = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TicketType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TicketNumber = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventRegistrations_Event_EventId",
                        column: x => x.EventId,
                        principalTable: "Event",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventRegistrations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SocialVerificationEvents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nchar(26)", fixedLength: true, maxLength: 26, nullable: false),
                    UserId = table.Column<string>(type: "nchar(26)", fixedLength: true, maxLength: 26, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProfileUrl = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    OccurredAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Outcome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SocialVerificationEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SocialVerificationEvents_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nchar(26)", fixedLength: true, maxLength: 26, nullable: false),
                    AideBannerDismissedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_UserPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DraftRegistrations_ExpiresAt",
                table: "DraftRegistrations",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_DraftRegistrations_IsApplied",
                table: "DraftRegistrations",
                column: "IsApplied");

            migrationBuilder.CreateIndex(
                name: "IX_DraftRegistrations_UserId",
                table: "DraftRegistrations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DraftRegistrations_UserId_Section",
                table: "DraftRegistrations",
                columns: new[] { "UserId", "Section" });

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_EventId",
                table: "EventRegistrations",
                column: "EventId");

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_RegisteredAt",
                table: "EventRegistrations",
                column: "RegisteredAt");

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_Status",
                table: "EventRegistrations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EventRegistrations_UserId_EventId",
                table: "EventRegistrations",
                columns: new[] { "UserId", "EventId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SocialVerificationEvents_Provider_ProfileUrl",
                table: "SocialVerificationEvents",
                columns: new[] { "Provider", "ProfileUrl" });

            migrationBuilder.CreateIndex(
                name: "IX_SocialVerificationEvents_UserId_OccurredAtUtc",
                table: "SocialVerificationEvents",
                columns: new[] { "UserId", "OccurredAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_GitHubProfile",
                table: "Users",
                column: "GitHubProfile",
                unique: true,
                filter: "[IsGitHubVerified] = 1 AND [GitHubProfile] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LinkedInProfile",
                table: "Users",
                column: "LinkedInProfile",
                unique: true,
                filter: "[IsLinkedInVerified] = 1 AND [LinkedInProfile] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_LinkedInSubject",
                table: "Users",
                column: "LinkedInSubject",
                unique: true,
                filter: "[IsLinkedInVerified] = 1 AND [LinkedInSubject] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DraftRegistrations");

            migrationBuilder.DropTable(
                name: "EventRegistrations");

            migrationBuilder.DropTable(
                name: "SocialVerificationEvents");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropTable(
                name: "Event");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
