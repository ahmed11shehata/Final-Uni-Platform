using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Presistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationCourseInstructor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistrationCourseInstructors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RegistrationSettingsId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    InstructorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationCourseInstructors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrationCourseInstructors_AspNetUsers_InstructorId",
                        column: x => x.InstructorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistrationCourseInstructors_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistrationCourseInstructors_RegistrationSettings_RegistrationSettingsId",
                        column: x => x.RegistrationSettingsId,
                        principalTable: "RegistrationSettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationCourseInstructors_CourseId",
                table: "RegistrationCourseInstructors",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationCourseInstructors_InstructorId",
                table: "RegistrationCourseInstructors",
                column: "InstructorId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrationCourseInstructors_RegistrationSettingsId",
                table: "RegistrationCourseInstructors",
                column: "RegistrationSettingsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrationCourseInstructors");
        }
    }
}
