using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visage.Services.Registrations.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialOAuthVerificationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "GitHubVerifiedAt",
                table: "Registrants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsGitHubVerified",
                table: "Registrants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLinkedInVerified",
                table: "Registrants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LinkedInVerifiedAt",
                table: "Registrants",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GitHubVerifiedAt",
                table: "Registrants");

            migrationBuilder.DropColumn(
                name: "IsGitHubVerified",
                table: "Registrants");

            migrationBuilder.DropColumn(
                name: "IsLinkedInVerified",
                table: "Registrants");

            migrationBuilder.DropColumn(
                name: "LinkedInVerifiedAt",
                table: "Registrants");
        }
    }
}
