using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Visage.Services.Registrations.Migrations
{
    /// <inheritdoc />
    public partial class AddDraftRegistrationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DraftRegistrations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nchar(26)", fixedLength: true, maxLength: 26, nullable: false),
                    UserId = table.Column<string>(type: "nchar(26)", fixedLength: true, maxLength: 26, nullable: false),
                    DraftData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DraftRegistrations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DraftRegistrations_Registrants_UserId",
                        column: x => x.UserId,
                        principalTable: "Registrants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DraftRegistrations_ExpiresAt",
                table: "DraftRegistrations",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_DraftRegistrations_UserId",
                table: "DraftRegistrations",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DraftRegistrations");
        }
    }
}
