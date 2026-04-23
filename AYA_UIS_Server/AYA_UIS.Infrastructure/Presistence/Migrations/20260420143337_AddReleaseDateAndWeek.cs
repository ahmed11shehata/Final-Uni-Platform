using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Presistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReleaseDateAndWeek : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReleaseDate",
                table: "CourseUploads",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Week",
                table: "CourseUploads",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReleaseDate",
                table: "Assignments",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReleaseDate",
                table: "CourseUploads");

            migrationBuilder.DropColumn(
                name: "Week",
                table: "CourseUploads");

            migrationBuilder.DropColumn(
                name: "ReleaseDate",
                table: "Assignments");
        }
    }
}
