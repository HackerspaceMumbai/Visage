using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visage.Services.Registrations.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkedInSubject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LinkedInSubject",
                table: "Registrants",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Registrants_LinkedInSubject",
                table: "Registrants",
                column: "LinkedInSubject",
                unique: true,
                filter: "[IsLinkedInVerified] = 1 AND [LinkedInSubject] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Registrants_LinkedInSubject",
                table: "Registrants");

            migrationBuilder.DropColumn(
                name: "LinkedInSubject",
                table: "Registrants");
        }
    }
}
