using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Presistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserCreatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Safe idempotent check — will not fail if column already exists
            migrationBuilder.Sql(@"
                IF COL_LENGTH('AspNetUsers', 'CreatedAt') IS NULL
                BEGIN
                    ALTER TABLE [AspNetUsers]
                        ADD [CreatedAt] datetime2 NOT NULL
                        CONSTRAINT [DF_AspNetUsers_CreatedAt] DEFAULT GETUTCDATE();
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF COL_LENGTH('AspNetUsers', 'CreatedAt') IS NOT NULL
                BEGIN
                    ALTER TABLE [AspNetUsers] DROP CONSTRAINT IF EXISTS [DF_AspNetUsers_CreatedAt];
                    ALTER TABLE [AspNetUsers] DROP COLUMN [CreatedAt];
                END
            ");
        }
    }
}
