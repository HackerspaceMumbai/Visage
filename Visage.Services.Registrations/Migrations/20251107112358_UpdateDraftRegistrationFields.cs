using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visage.Services.Registrations.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDraftRegistrationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DataHash",
                table: "DraftRegistrations",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApplied",
                table: "DraftRegistrations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Section",
                table: "DraftRegistrations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "DraftRegistrations",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_DraftRegistrations_IsApplied",
                table: "DraftRegistrations",
                column: "IsApplied");

            migrationBuilder.CreateIndex(
                name: "IX_DraftRegistrations_UserId_Section",
                table: "DraftRegistrations",
                columns: new[] { "UserId", "Section" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DraftRegistrations_IsApplied",
                table: "DraftRegistrations");

            migrationBuilder.DropIndex(
                name: "IX_DraftRegistrations_UserId_Section",
                table: "DraftRegistrations");

            migrationBuilder.DropColumn(
                name: "DataHash",
                table: "DraftRegistrations");

            migrationBuilder.DropColumn(
                name: "IsApplied",
                table: "DraftRegistrations");

            migrationBuilder.DropColumn(
                name: "Section",
                table: "DraftRegistrations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "DraftRegistrations");
        }
    }
}
