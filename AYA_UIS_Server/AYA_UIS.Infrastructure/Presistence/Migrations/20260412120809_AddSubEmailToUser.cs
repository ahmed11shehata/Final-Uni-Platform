using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Presistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubEmailToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // These columns already exist in the database — add them only if missing
            // (they were added manually before being tracked in migrations)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'Registrations') AND name = N'IsEquivalency'
                )
                BEGIN
                    ALTER TABLE [Registrations] ADD [IsEquivalency] bit NOT NULL DEFAULT 0
                END");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'Registrations') AND name = N'NumericTotal'
                )
                BEGIN
                    ALTER TABLE [Registrations] ADD [NumericTotal] int NULL
                END");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'AdminCourseLocks') AND name = N'Reason'
                )
                BEGIN
                    ALTER TABLE [AdminCourseLocks] ADD [Reason] nvarchar(max) NULL
                END");

            // New column — SubEmail on AspNetUsers
            migrationBuilder.AddColumn<string>(
                name: "SubEmail",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Only drop SubEmail — the other columns pre-existed this migration
            migrationBuilder.DropColumn(
                name: "SubEmail",
                table: "AspNetUsers");
        }
    }
}
