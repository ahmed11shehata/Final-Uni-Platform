using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Presistence.Migrations
{
    public partial class AddRegistrationSettingsAndCourseLock : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistrationSettings",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    IsOpen        = table.Column<bool>(nullable: false, defaultValue: false),
                    Semester      = table.Column<string>(maxLength: 50,   nullable: false, defaultValue: ""),
                    AcademicYear  = table.Column<string>(maxLength: 20,   nullable: false, defaultValue: ""),
                    StartDate     = table.Column<DateTime>(nullable: true),
                    Deadline      = table.Column<DateTime>(nullable: true),
                    OpenYears     = table.Column<string>(maxLength: 20,   nullable: false, defaultValue: ""),
                    EnabledCourses= table.Column<string>(maxLength: 5000, nullable: false, defaultValue: ""),
                    OpenedAt      = table.Column<DateTime>(nullable: true),
                    ClosedAt      = table.Column<DateTime>(nullable: true)
                },
                constraints: t => t.PrimaryKey("PK_RegistrationSettings", x => x.Id));

            migrationBuilder.CreateTable(
                name: "AdminCourseLocks",
                columns: table => new
                {
                    Id       = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    UserId   = table.Column<string>(maxLength: 450, nullable: false),
                    CourseId = table.Column<int>(nullable: false),
                    LockedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: t =>
                {
                    t.PrimaryKey("PK_AdminCourseLocks", x => x.Id);
                    t.ForeignKey("FK_AdminCourseLocks_AspNetUsers_UserId",   x => x.UserId,   "AspNetUsers", "Id", onDelete: ReferentialAction.Cascade);
                    t.ForeignKey("FK_AdminCourseLocks_Courses_CourseId",     x => x.CourseId, "Courses",     "Id", onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex("IX_AdminCourseLocks_UserId_CourseId",  "AdminCourseLocks", new[] { "UserId", "CourseId" }, unique: true);
            migrationBuilder.CreateIndex("IX_AdminCourseLocks_CourseId",          "AdminCourseLocks", "CourseId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("AdminCourseLocks");
            migrationBuilder.DropTable("RegistrationSettings");
        }
    }
}
