using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visage.Services.Registrations.Migrations
{
    /// <inheritdoc />
    public partial class AddGovtIdTypeColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GovtIdType",
                table: "Registrants",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GovtIdType",
                table: "Registrants");
        }
    }
}
