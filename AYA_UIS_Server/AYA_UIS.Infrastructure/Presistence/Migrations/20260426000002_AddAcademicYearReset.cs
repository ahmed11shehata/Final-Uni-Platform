using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Presistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicYearReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Registration archive flags ─────────────────────────────
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Registrations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ArchivedAt",
                table: "Registrations",
                type: "datetime2",
                nullable: true);

            // ── AcademicYearResets (audit log) ─────────────────────────
            migrationBuilder.CreateTable(
                name: "AcademicYearResets",
                columns: table => new
                {
                    Id                    = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    AdminId               = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ExecutedAt            = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StudentsCount         = table.Column<int>(nullable: false),
                    ForceReset            = table.Column<bool>(nullable: false),
                    SelectAll             = table.Column<bool>(nullable: false),
                    SourceStudyYearId     = table.Column<int>(nullable: true),
                    SourceSemesterId      = table.Column<int>(nullable: true),
                    TargetStudyYearId     = table.Column<int>(nullable: true),
                    TargetSemesterId      = table.Column<int>(nullable: true),
                    SourceTerm            = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TargetTerm            = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ArchivedRegistrations = table.Column<int>(nullable: false),
                    PassedCount           = table.Column<int>(nullable: false),
                    FailedCount           = table.Column<int>(nullable: false),
                    UnassignedFailedCount = table.Column<int>(nullable: false),
                    FinalGradesPurged     = table.Column<int>(nullable: false),
                    QuizAttemptsPurged    = table.Column<int>(nullable: false),
                    SubmissionsPurged     = table.Column<int>(nullable: false),
                    MidtermsPurged        = table.Column<int>(nullable: false),
                    NotificationsSent     = table.Column<int>(nullable: false),
                    SummaryJson           = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicYearResets", x => x.Id);
                });

            // ── AcademicYearResetSnapshots (per-student backup payload) ─
            migrationBuilder.CreateTable(
                name: "AcademicYearResetSnapshots",
                columns: table => new
                {
                    Id                  = table.Column<int>(nullable: false).Annotation("SqlServer:Identity", "1, 1"),
                    ResetId             = table.Column<int>(nullable: false),
                    StudentId           = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CapturedAt          = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SourceLevel         = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceSemester      = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TargetLevel         = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TargetSemester      = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationsCount  = table.Column<int>(nullable: false),
                    FinalGradesCount    = table.Column<int>(nullable: false),
                    SubmissionsCount    = table.Column<int>(nullable: false),
                    QuizAttemptsCount   = table.Column<int>(nullable: false),
                    PayloadJson         = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicYearResetSnapshots", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYearResetSnapshots_ResetId",
                table: "AcademicYearResetSnapshots",
                column: "ResetId");

            migrationBuilder.CreateIndex(
                name: "IX_AcademicYearResetSnapshots_StudentId",
                table: "AcademicYearResetSnapshots",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "AcademicYearResetSnapshots");
            migrationBuilder.DropTable(name: "AcademicYearResets");

            migrationBuilder.DropColumn(name: "ArchivedAt", table: "Registrations");
            migrationBuilder.DropColumn(name: "IsArchived", table: "Registrations");
        }
    }
}
