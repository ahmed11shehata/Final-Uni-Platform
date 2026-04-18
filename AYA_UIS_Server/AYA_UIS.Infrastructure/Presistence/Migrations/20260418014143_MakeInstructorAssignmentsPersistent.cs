using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Presistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeInstructorAssignmentsPersistent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationCourseInstructors_Courses_CourseId",
                table: "RegistrationCourseInstructors");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationCourseInstructors_RegistrationSettings_RegistrationSettingsId",
                table: "RegistrationCourseInstructors");

            migrationBuilder.DropIndex(
                name: "IX_RegistrationCourseInstructors_CourseId",
                table: "RegistrationCourseInstructors");

            migrationBuilder.DropIndex(
                name: "IX_RegistrationCourseInstructors_RegistrationSettingsId",
                table: "RegistrationCourseInstructors");

            migrationBuilder.DropColumn(
                name: "RegistrationSettingsId",
                table: "RegistrationCourseInstructors");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationCourseInstructors_CourseId_InstructorId",
                table: "RegistrationCourseInstructors",
                columns: new[] { "CourseId", "InstructorId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationCourseInstructors_Courses_CourseId",
                table: "RegistrationCourseInstructors",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistrationCourseInstructors_Courses_CourseId",
                table: "RegistrationCourseInstructors");

            migrationBuilder.DropIndex(
                name: "IX_RegistrationCourseInstructors_CourseId_InstructorId",
                table: "RegistrationCourseInstructors");

            migrationBuilder.AddColumn<int>(
                name: "RegistrationSettingsId",
                table: "RegistrationCourseInstructors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationCourseInstructors_CourseId",
                table: "RegistrationCourseInstructors",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationCourseInstructors_RegistrationSettingsId",
                table: "RegistrationCourseInstructors",
                column: "RegistrationSettingsId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationCourseInstructors_Courses_CourseId",
                table: "RegistrationCourseInstructors",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistrationCourseInstructors_RegistrationSettings_RegistrationSettingsId",
                table: "RegistrationCourseInstructors",
                column: "RegistrationSettingsId",
                principalTable: "RegistrationSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
