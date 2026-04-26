using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Presistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFinalGradeConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. rowversion column for optimistic concurrency on Update
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "FinalGrades",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            // 2. Unique index — prevents two concurrent inserts for the same
            //    (StudentId, CourseId) pair from both succeeding silently.
            migrationBuilder.CreateIndex(
                name: "IX_FinalGrades_StudentId_CourseId",
                table: "FinalGrades",
                columns: new[] { "StudentId", "CourseId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FinalGrades_StudentId_CourseId",
                table: "FinalGrades");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "FinalGrades");
        }
    }
}
