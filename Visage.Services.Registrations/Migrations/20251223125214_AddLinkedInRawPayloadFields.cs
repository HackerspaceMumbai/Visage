using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visage.Services.Registrations.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkedInRawPayloadFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LinkedInPayloadFetchedAt",
                table: "Registrants",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedInRawEmailJson",
                table: "Registrants",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkedInRawProfileJson",
                table: "Registrants",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkedInPayloadFetchedAt",
                table: "Registrants");

            migrationBuilder.DropColumn(
                name: "LinkedInRawEmailJson",
                table: "Registrants");

            migrationBuilder.DropColumn(
                name: "LinkedInRawProfileJson",
                table: "Registrants");
        }
    }
}
