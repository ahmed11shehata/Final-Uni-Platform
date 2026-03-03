using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Presistence.Migrations
{
    /// <inheritdoc />
    public partial class DeleteCol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CoursePrerequisites_Courses_CourseId1",
                table: "CoursePrerequisites");

            migrationBuilder.DropIndex(
                name: "IX_CoursePrerequisites_CourseId1",
                table: "CoursePrerequisites");

            migrationBuilder.DropColumn(
                name: "CourseId1",
                table: "CoursePrerequisites");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CourseId1",
                table: "CoursePrerequisites",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CoursePrerequisites_CourseId1",
                table: "CoursePrerequisites",
                column: "CourseId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CoursePrerequisites_Courses_CourseId1",
                table: "CoursePrerequisites",
                column: "CourseId1",
                principalTable: "Courses",
                principalColumn: "Id");
        }
    }
}
