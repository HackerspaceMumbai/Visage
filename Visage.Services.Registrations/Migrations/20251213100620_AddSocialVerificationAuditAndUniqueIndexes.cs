using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visage.Services.Registrations.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialVerificationAuditAndUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "LinkedInProfile",
                table: "Registrants",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GitHubProfile",
                table: "Registrants",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "SocialVerificationEvents",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nchar(26)", fixedLength: true, maxLength: 26, nullable: false),
                    RegistrantId = table.Column<string>(type: "nchar(26)", fixedLength: true, maxLength: 26, nullable: false),
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
                        name: "FK_SocialVerificationEvents_Registrants_RegistrantId",
                        column: x => x.RegistrantId,
                        principalTable: "Registrants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Registrants_GitHubProfile",
                table: "Registrants",
                column: "GitHubProfile",
                unique: true,
                filter: "[IsGitHubVerified] = 1 AND [GitHubProfile] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Registrants_LinkedInProfile",
                table: "Registrants",
                column: "LinkedInProfile",
                unique: true,
                filter: "[IsLinkedInVerified] = 1 AND [LinkedInProfile] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SocialVerificationEvents_Provider_ProfileUrl",
                table: "SocialVerificationEvents",
                columns: new[] { "Provider", "ProfileUrl" });

            migrationBuilder.CreateIndex(
                name: "IX_SocialVerificationEvents_RegistrantId_OccurredAtUtc",
                table: "SocialVerificationEvents",
                columns: new[] { "RegistrantId", "OccurredAtUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SocialVerificationEvents");

            migrationBuilder.DropIndex(
                name: "IX_Registrants_GitHubProfile",
                table: "Registrants");

            migrationBuilder.DropIndex(
                name: "IX_Registrants_LinkedInProfile",
                table: "Registrants");

            migrationBuilder.AlterColumn<string>(
                name: "LinkedInProfile",
                table: "Registrants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GitHubProfile",
                table: "Registrants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
