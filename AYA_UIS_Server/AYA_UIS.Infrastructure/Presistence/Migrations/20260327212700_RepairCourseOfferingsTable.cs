using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Presistence.Migrations
{
    /// <inheritdoc />
    public partial class RepairCourseOfferingsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The original AddCourseOfferings migration had empty Up/Down,
            // so the table was never created despite EF marking it as applied.
            // This repair migration creates the table idempotently.

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[CourseOfferings]', N'U') IS NULL
                BEGIN
                    CREATE TABLE [CourseOfferings] (
                        [Id]          int           NOT NULL IDENTITY(1,1),
                        [CourseId]    int           NOT NULL,
                        [StudyYearId] int           NOT NULL,
                        [SemesterId]  int           NOT NULL,
                        [Level]       int           NOT NULL,
                        [IsOpen]      bit           NOT NULL CONSTRAINT [DF_CourseOfferings_IsOpen] DEFAULT 0,
                        CONSTRAINT [PK_CourseOfferings] PRIMARY KEY ([Id]),
                        CONSTRAINT [FK_CourseOfferings_Courses_CourseId]
                            FOREIGN KEY ([CourseId]) REFERENCES [Courses]([Id])
                            ON DELETE NO ACTION,
                        CONSTRAINT [FK_CourseOfferings_Semesters_SemesterId]
                            FOREIGN KEY ([SemesterId]) REFERENCES [Semesters]([Id])
                            ON DELETE NO ACTION,
                        CONSTRAINT [FK_CourseOfferings_StudyYears_StudyYearId]
                            FOREIGN KEY ([StudyYearId]) REFERENCES [StudyYears]([Id])
                            ON DELETE NO ACTION
                    );

                    CREATE INDEX [IX_CourseOfferings_SemesterId]
                        ON [CourseOfferings] ([SemesterId]);

                    CREATE INDEX [IX_CourseOfferings_StudyYearId]
                        ON [CourseOfferings] ([StudyYearId]);

                    CREATE UNIQUE INDEX [IX_CourseOfferings_CourseId_StudyYearId_SemesterId_Level]
                        ON [CourseOfferings] ([CourseId], [StudyYearId], [SemesterId], [Level]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[CourseOfferings]', N'U') IS NOT NULL
                BEGIN
                    DROP TABLE [CourseOfferings];
                END
            ");
        }
    }
}
