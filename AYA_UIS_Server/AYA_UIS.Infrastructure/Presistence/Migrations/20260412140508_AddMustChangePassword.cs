using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Presistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMustChangePassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // MustChangePassword — new column
            migrationBuilder.AddColumn<bool>(
                name: "MustChangePassword",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // SubEmail already exists (added by previous migration) — skip if present
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM sys.columns
                    WHERE object_id = OBJECT_ID(N'AspNetUsers') AND name = N'SubEmail'
                )
                BEGIN
                    ALTER TABLE [AspNetUsers] ADD [SubEmail] nvarchar(max) NULL
                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MustChangePassword",
                table: "AspNetUsers");

            // SubEmail was owned by the previous migration — do not drop here
        }
    }
}
