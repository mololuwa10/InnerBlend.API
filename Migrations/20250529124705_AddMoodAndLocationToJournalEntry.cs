using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InnerBlend.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMoodAndLocationToJournalEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "JournalEntries",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Mood",
                table: "JournalEntries",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "Mood",
                table: "JournalEntries");
        }
    }
}
