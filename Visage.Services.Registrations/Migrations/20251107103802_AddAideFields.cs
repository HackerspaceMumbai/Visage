using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visage.Services.Registrations.Migrations
{
    /// <inheritdoc />
    public partial class AddAideFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CaregivingResponsibilities",
                table: "Registrants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Neurodiversity",
                table: "Registrants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SocioeconomicBackground",
                table: "Registrants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaregivingResponsibilities",
                table: "Registrants");

            migrationBuilder.DropColumn(
                name: "Neurodiversity",
                table: "Registrants");

            migrationBuilder.DropColumn(
                name: "SocioeconomicBackground",
                table: "Registrants");
        }
    }
}
