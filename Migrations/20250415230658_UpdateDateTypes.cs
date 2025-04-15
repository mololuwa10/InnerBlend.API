using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InnerBlend.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDateTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"ALTER TABLE ""Users"" 
                ALTER COLUMN ""DateModified"" 
                TYPE timestamp with time zone 
                USING ""DateModified""::timestamp with time zone;");

            migrationBuilder.Sql(
                @"ALTER TABLE ""Users"" 
                ALTER COLUMN ""DateCreated"" 
                TYPE timestamp with time zone 
                USING ""DateCreated""::timestamp with time zone;");

            migrationBuilder.Sql(
                @"ALTER TABLE ""Journals"" 
                ALTER COLUMN ""DateModified"" 
                TYPE timestamp with time zone 
                USING ""DateModified""::timestamp with time zone;");

            migrationBuilder.Sql(
                @"ALTER TABLE ""Journals"" 
                ALTER COLUMN ""DateCreated"" 
                TYPE timestamp with time zone 
                USING ""DateCreated""::timestamp with time zone;");

            migrationBuilder.Sql(
                @"ALTER TABLE ""JournalEntries"" 
                ALTER COLUMN ""DateModified"" 
                TYPE timestamp with time zone 
                USING ""DateModified""::timestamp with time zone;");

            migrationBuilder.Sql(
                @"ALTER TABLE ""JournalEntries"" 
                ALTER COLUMN ""DateCreated"" 
                TYPE timestamp with time zone 
                USING ""DateCreated""::timestamp with time zone;");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DateModified",
                table: "Users",
                type: "text",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DateCreated",
                table: "Users",
                type: "text",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DateModified",
                table: "Journals",
                type: "text",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DateCreated",
                table: "Journals",
                type: "text",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DateModified",
                table: "JournalEntries",
                type: "text",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DateCreated",
                table: "JournalEntries",
                type: "text",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
